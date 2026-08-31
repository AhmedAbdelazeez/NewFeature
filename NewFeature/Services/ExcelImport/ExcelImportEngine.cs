using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ClosedXML.Excel;
using Microsoft.Extensions.Logging;
using NewFeature.Models;

namespace NewFeature.Services.ExcelImport
{
    public enum ExcelRowOutcome
    {
        Inserted,
        Updated,
        Skipped
    }

    // What a per-module row handler returns after looking at one physical Excel row: whether it
    // was inserted, updated, or skipped, and - when skipped - the reason to report back to the
    // uploader.
    public class ExcelRowOutcomeResult
    {
        public ExcelRowOutcome Outcome { get; set; }
        public string? ErrorColumn { get; set; }
        public string? ErrorMessage { get; set; }

        public static ExcelRowOutcomeResult Inserted() => new() { Outcome = ExcelRowOutcome.Inserted };
        public static ExcelRowOutcomeResult Updated() => new() { Outcome = ExcelRowOutcome.Updated };
        public static ExcelRowOutcomeResult Skipped(string message, string? column = null) =>
            new() { Outcome = ExcelRowOutcome.Skipped, ErrorMessage = message, ErrorColumn = column };
    }

    // Read-only view of one physical row handed to a module's row handler. Cells are looked up by
    // the template's column Key rather than a raw column index, so a module's row-handling code
    // never has to care what physical Excel column a field ended up in.
    public class ExcelRowContext
    {
        private readonly IXLRow _row;
        private readonly Dictionary<string, int> _columnMap; // Key -> 1-based column index

        public ExcelRowContext(IXLRow row, int rowNumber, Dictionary<string, int> columnMap)
        {
            _row = row;
            RowNumber = rowNumber;
            _columnMap = columnMap;
        }

        public int RowNumber { get; }

        public bool HasColumn(string key) => _columnMap.ContainsKey(key);

        public string GetString(string key)
        {
            if (!_columnMap.TryGetValue(key, out var col)) return string.Empty;
            return _row.Cell(col).GetString().Trim();
        }

        // Typed readers used by the department templates. Every one of them is deliberately
        // lenient: the approved templates are filled in by hand, so a cell can arrive as a real
        // Excel date/number or as free text ("15/03/2026", "12,500 ريال", "٤"). A cell that
        // can't be understood comes back null rather than throwing, and the calling row handler
        // decides whether that makes the row skippable or just leaves a default in place.
        public DateTime? GetDate(string key)
        {
            if (!_columnMap.TryGetValue(key, out var col)) return null;
            var cell = _row.Cell(col);

            if (cell.DataType == XLDataType.DateTime && cell.TryGetValue<DateTime>(out var typed))
                return typed;

            var raw = Normalize(cell.GetString());
            if (string.IsNullOrEmpty(raw)) return null;

            if (DateTime.TryParse(raw, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed)) return parsed;
            if (DateTime.TryParse(raw, out parsed)) return parsed;

            // Excel stores dates as day serial numbers; a cell formatted as General shows the raw
            // serial instead of a date string.
            if (double.TryParse(raw, NumberStyles.Any, CultureInfo.InvariantCulture, out var serial)
                && serial > 0 && serial < 2958466)
            {
                try { return DateTime.FromOADate(serial); } catch (ArgumentException) { return null; }
            }

            return null;
        }

        public decimal? GetDecimal(string key)
        {
            if (!_columnMap.TryGetValue(key, out var col)) return null;
            var cell = _row.Cell(col);

            if (cell.DataType == XLDataType.Number && cell.TryGetValue<double>(out var typed))
                return (decimal)typed;

            var raw = Normalize(cell.GetString());
            if (string.IsNullOrEmpty(raw)) return null;

            // Strip any currency wording typed alongside the figure.
            var cleaned = new string(raw.Where(c => char.IsDigit(c) || c == '.' || c == '-').ToArray());
            if (string.IsNullOrEmpty(cleaned)) return null;

            return decimal.TryParse(cleaned, NumberStyles.Any, CultureInfo.InvariantCulture, out var value)
                ? value
                : null;
        }

        public int? GetInt(string key)
        {
            var value = GetDecimal(key);
            if (value == null) return null;
            if (value < int.MinValue || value > int.MaxValue) return null;
            return (int)Math.Round(value.Value, MidpointRounding.AwayFromZero);
        }

        // Arabic-Indic digits are what a keyboard set to Arabic produces, and they never parse
        // with the invariant culture - fold them to ASCII before any TryParse sees the text.
        private static string Normalize(string? text)
        {
            if (string.IsNullOrWhiteSpace(text)) return string.Empty;

            var builder = new System.Text.StringBuilder(text.Length);
            foreach (var ch in text.Trim())
            {
                if (ch >= '\u0660' && ch <= '\u0669') builder.Append((char)('0' + (ch - '\u0660')));       // Arabic-Indic
                else if (ch >= '\u06F0' && ch <= '\u06F9') builder.Append((char)('0' + (ch - '\u06F0'))); // Extended Arabic-Indic
                else if (ch == ',' || ch == '\u066C') continue;                                           // thousands separators
                else builder.Append(ch);
            }
            return builder.ToString().Trim();
        }
    }

    // Shared bulk-Excel-import pipeline intended for every Fleet sub-department (and reusable by
    // any future module). File-type validation (.xlsx/.xls) is handled by the caller via
    // ExcelCompatibility before this runs; this engine owns template/header validation, empty-row
    // detection, per-row iteration with isolated error handling, statistics, and a single batched
    // save at the end so one invalid row never affects any valid row's commit.
    public static class ExcelImportEngine
    {
        public static async Task<ExcelImportResultDto> RunAsync(
            Stream excelStream,
            ExcelTemplateDefinition template,
            Func<ExcelRowContext, Task<ExcelRowOutcomeResult>> processRow,
            // Bare (non-generic) Task, unlike Task<T> above, is ambiguous in this file: this
            // project also defines NewFeature.Models.Task (a Task-management entity), and with
            // "using NewFeature.Models;" also in scope, an unqualified "Task" matches both. Fully
            // qualifying only this one non-generic usage resolves it without an alias.
            Func<System.Threading.Tasks.Task> saveChangesAsync,
            ILogger logger)
        {
            var result = new ExcelImportResultDto();

            IXLWorkbook workbook;
            try
            {
                workbook = new XLWorkbook(excelStream);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to open uploaded workbook for template {Template}.", template.TemplateName);
                result.Success = false;
                result.Message = "The uploaded file could not be read as an Excel workbook. Please make sure it is a valid .xlsx or .xls file.";
                return result;
            }

            using (workbook)
            {
                var worksheet = workbook.Worksheets.FirstOrDefault();
                if (worksheet == null)
                {
                    result.Success = false;
                    result.Message = "The uploaded Excel file has no worksheets.";
                    return result;
                }

                var headerRow = worksheet.Row(1);
                var lastHeaderCell = headerRow.LastCellUsed();
                if (lastHeaderCell == null)
                {
                    result.Success = false;
                    result.Message = "The uploaded Excel file has no header row.";
                    return result;
                }

                // Read the header row once.
                var rawHeaders = new List<(string Text, int Column)>();
                for (int i = 1; i <= lastHeaderCell.Address.ColumnNumber; i++)
                {
                    var text = headerRow.Cell(i).GetString().Trim();
                    if (!string.IsNullOrEmpty(text)) rawHeaders.Add((text, i));
                }

                // Match each template column against the actual header text by keyword/alias.
                var columnMap = new Dictionary<string, int>();
                var duplicateColumns = new List<string>();
                foreach (var col in template.Columns)
                {
                    var matches = rawHeaders
                        .Where(h => col.HeaderAliases.Any(alias => h.Text.IndexOf(alias, StringComparison.OrdinalIgnoreCase) >= 0))
                        .ToList();

                    if (matches.Count > 1)
                    {
                        duplicateColumns.Add(col.DisplayName);
                    }
                    if (matches.Count > 0)
                    {
                        columnMap[col.Key] = matches[0].Column;
                    }
                }

                if (duplicateColumns.Any())
                {
                    result.Success = false;
                    result.Message = $"Invalid Excel template for {template.TemplateName}. The following column(s) appear more than once in the header row: {string.Join(", ", duplicateColumns)}. Please use the approved {template.TemplateName} template.";
                    return result;
                }

                // The identity column is what proves this is the right sheet at all.
                if (!columnMap.ContainsKey(template.IdentityColumnKey))
                {
                    var identityCol = template.GetColumn(template.IdentityColumnKey);
                    result.Success = false;
                    result.Message = $"Invalid Excel template. The required column \"{identityCol?.DisplayName ?? template.IdentityColumnKey}\" is missing. Please use the approved {template.TemplateName} template.";
                    return result;
                }

                var missingRequired = template.Columns
                    .Where(c => c.Required && !columnMap.ContainsKey(c.Key))
                    .Select(c => c.DisplayName)
                    .ToList();

                if (missingRequired.Any())
                {
                    result.Success = false;
                    result.Message = $"Invalid Excel template for {template.TemplateName}. The following required column(s) are missing: {string.Join(", ", missingRequired)}. Please use the approved {template.TemplateName} template.";
                    return result;
                }

                var dataRows = worksheet.RowsUsed().Skip(1).ToList();
                result.TotalRows = dataRows.Count;

                foreach (var row in dataRows)
                {
                    // Empty-row detection: a row where every mapped template column is blank is
                    // not data. Excel sheets frequently carry many trailing blank rows and these
                    // must never be counted as data, inserted, or reported as skipped errors.
                    bool isEmpty = columnMap.Values.All(col => string.IsNullOrWhiteSpace(row.Cell(col).GetString()));
                    if (isEmpty) continue;

                    result.DataRows++;

                    var context = new ExcelRowContext(row, row.RowNumber(), columnMap);
                    try
                    {
                        var outcome = await processRow(context);
                        switch (outcome.Outcome)
                        {
                            case ExcelRowOutcome.Inserted:
                                result.InsertedRows++;
                                break;
                            case ExcelRowOutcome.Updated:
                                result.UpdatedRows++;
                                break;
                            case ExcelRowOutcome.Skipped:
                                result.SkippedRows++;
                                result.Errors.Add(new ExcelRowErrorDto
                                {
                                    RowNumber = context.RowNumber,
                                    Column = outcome.ErrorColumn,
                                    Message = outcome.ErrorMessage ?? "This row could not be imported."
                                });
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        // Technical exceptions never reach the user verbatim - logged in full
                        // here, reported back only as a generic, safe message tied to the row.
                        logger.LogError(ex, "Unexpected error processing row {RowNumber} of a {Template} upload.", context.RowNumber, template.TemplateName);
                        result.SkippedRows++;
                        result.Errors.Add(new ExcelRowErrorDto
                        {
                            RowNumber = context.RowNumber,
                            Message = "This row could not be processed due to an unexpected error and was skipped."
                        });
                    }
                }

                // One batched save for every valid row accepted above. Rows that failed
                // validation were never added to the change tracker, so a failure here is a
                // genuine, unexpected persistence error - never "one bad row rolling back the
                // rest," which this design avoids by construction.
                try
                {
                    await saveChangesAsync();
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to save a {Template} import batch.", template.TemplateName);
                    result.Success = false;
                    result.Message = "The file was validated and processed, but the results could not be saved. Please try again or contact support.";
                    return result;
                }

                result.Success = true;
                result.Message = result.Errors.Any()
                    ? $"Excel file processed with {result.SkippedRows} skipped row(s). See details below."
                    : "Excel file processed successfully.";
                return result;
            }
        }
    }
}
