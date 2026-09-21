using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ClosedXML.Excel;
using Microsoft.Extensions.Logging;
using NewFeature.Models;
using NewFeature.Services;

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

            // Accepts Gregorian or Hijri dates, in any of the usual written formats - see
            // FlexibleDateParser for the calendar-detection rules.
            return FlexibleDateParser.Parse(cell.GetString());
        }

        // Reads a clock time out of a cell that may hold a real Excel time (a fraction of a day),
        // a full date/time, or free text typed by hand ("7:30", "07:30 ص", "19:45"). Returns null
        // when the cell is blank or unreadable, so the caller decides whether that is fatal.
        public TimeSpan? GetTime(string key)
        {
            if (!_columnMap.TryGetValue(key, out var col)) return null;
            var cell = _row.Cell(col);

            if (cell.DataType == XLDataType.DateTime && cell.TryGetValue<DateTime>(out var asDateTime))
                return asDateTime.TimeOfDay;

            // A bare time in Excel is stored as a number: the fraction of a 24-hour day.
            if (cell.DataType == XLDataType.Number && cell.TryGetValue<double>(out var asNumber))
            {
                var fraction = asNumber - Math.Floor(asNumber);
                return TimeSpan.FromDays(fraction);
            }

            var raw = Normalize(cell.GetString());
            if (string.IsNullOrEmpty(raw)) return null;

            // Arabic AM/PM markers never parse with the invariant culture - fold them first.
            bool isPm = raw.Contains("م") || raw.IndexOf("pm", StringComparison.OrdinalIgnoreCase) >= 0;
            bool isAm = raw.Contains("ص") || raw.IndexOf("am", StringComparison.OrdinalIgnoreCase) >= 0;
            var cleaned = new string(raw.Where(c => char.IsDigit(c) || c == ':').ToArray());
            if (string.IsNullOrEmpty(cleaned)) return null;

            if (!TimeSpan.TryParse(cleaned, CultureInfo.InvariantCulture, out var parsed))
            {
                // "7" / "0730" style entries.
                if (!int.TryParse(cleaned, NumberStyles.Integer, CultureInfo.InvariantCulture, out var digits)) return null;
                parsed = digits <= 23
                    ? TimeSpan.FromHours(digits)
                    : new TimeSpan(digits / 100, digits % 100, 0);
            }

            if (parsed.TotalHours >= 24) return null;
            if (isPm && parsed.Hours < 12) parsed = parsed.Add(TimeSpan.FromHours(12));
            if (isAm && parsed.Hours == 12) parsed = parsed.Subtract(TimeSpan.FromHours(12));
            return parsed;
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
                result.Message = "تعذّرت قراءة الملف المرفوع كملف إكسل. تأكد من أنه ملف .xlsx أو .xls صالح وغير تالف.";
                return result;
            }

            using (workbook)
            {
                if (workbook.Worksheets.Count == 0)
                {
                    result.Success = false;
                    result.Message = "الملف المرفوع لا يحتوي على أي ورقة عمل.";
                    return result;
                }

                // A definition that names its sheet must find that sheet; anything else reads the
                // first one, which is what every single-sheet department template relies on.
                IXLWorksheet? worksheet;
                if (template.SheetNameAliases.Length > 0)
                {
                    worksheet = workbook.Worksheets.FirstOrDefault(ws =>
                        template.SheetNameAliases.Any(alias =>
                            ws.Name.Trim().IndexOf(alias, StringComparison.OrdinalIgnoreCase) >= 0));

                    if (worksheet == null)
                    {
                        var sheetLabel = string.IsNullOrEmpty(template.SheetDisplayName)
                            ? template.SheetNameAliases[0]
                            : template.SheetDisplayName;
                        result.Success = false;
                        result.Message = $"الملف المرفوع لا يحتوي على ورقة \"{sheetLabel}\". يرجى استخدام {template.TemplateName} المعتمد كما هو دون حذف أي ورقة منه.";
                        return result;
                    }
                }
                else
                {
                    worksheet = workbook.Worksheets.First();
                }

                var headerRow = worksheet.Row(1);
                var lastHeaderCell = headerRow.LastCellUsed();
                if (lastHeaderCell == null)
                {
                    result.Success = false;
                    result.Message = "الورقة المرفوعة لا تحتوي على صف عناوين في السطر الأول.";
                    return result;
                }

                // Read the header row once.
                var rawHeaders = new List<(string Text, int Column)>();
                for (int i = 1; i <= lastHeaderCell.Address.ColumnNumber; i++)
                {
                    var text = headerRow.Cell(i).GetString().Trim();
                    if (!string.IsNullOrEmpty(text)) rawHeaders.Add((text, i));
                }

                // Match each template column against the actual header text, exact names first and
                // keyword/alias second. The exact pass is what lets a real sheet carry headers that
                // are prefixes of one another - "Direction" beside "Direction Name", "Location"
                // beside "From Location"/"To Location" - without every one of them matching every
                // other by substring and being reported as a duplicated column. Whatever the exact
                // pass claims is then off-limits to the looser pass, so the keyword fallback can
                // never steal a column that already belongs to another field.
                var columnMap = new Dictionary<string, int>();
                var duplicateColumns = new List<string>();
                var claimedColumns = new HashSet<int>();

                foreach (var col in template.Columns)
                {
                    var exact = rawHeaders
                        .Where(h => col.HeaderAliases.Any(alias => string.Equals(h.Text, alias.Trim(), StringComparison.OrdinalIgnoreCase)))
                        .ToList();

                    if (exact.Count > 0)
                    {
                        columnMap[col.Key] = exact[0].Column;
                        claimedColumns.Add(exact[0].Column);
                    }
                }

                foreach (var col in template.Columns)
                {
                    if (columnMap.ContainsKey(col.Key)) continue;

                    var matches = rawHeaders
                        .Where(h => !claimedColumns.Contains(h.Column))
                        .Where(h => col.HeaderAliases.Any(alias => h.Text.IndexOf(alias.Trim(), StringComparison.OrdinalIgnoreCase) >= 0))
                        .ToList();

                    if (matches.Count > 1)
                    {
                        duplicateColumns.Add(col.DisplayName);
                    }
                    if (matches.Count > 0)
                    {
                        columnMap[col.Key] = matches[0].Column;
                        claimedColumns.Add(matches[0].Column);
                    }
                }

                if (duplicateColumns.Any())
                {
                    result.Success = false;
                    result.Message = $"صف العناوين يحتوي على تكرار للأعمدة التالية: {string.Join("، ", duplicateColumns)}. يرجى استخدام {template.TemplateName} المعتمد دون تعديل صف العناوين.";
                    return result;
                }

                // The identity column is what proves this is the right sheet at all.
                if (!columnMap.ContainsKey(template.IdentityColumnKey))
                {
                    var identityCol = template.GetColumn(template.IdentityColumnKey);
                    result.Success = false;
                    result.Message = $"النموذج غير صحيح: لم يتم العثور على العمود الأساسي \"{identityCol?.DisplayName ?? template.IdentityColumnKey}\". يرجى استخدام {template.TemplateName} المعتمد.";
                    return result;
                }

                var missingRequired = template.Columns
                    .Where(c => c.Required && !columnMap.ContainsKey(c.Key))
                    .Select(c => c.DisplayName)
                    .ToList();

                if (missingRequired.Any())
                {
                    result.Success = false;
                    result.Message = $"الأعمدة الإلزامية التالية غير موجودة في صف العناوين: {string.Join("، ", missingRequired)}. يرجى استخدام {template.TemplateName} المعتمد.";
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
                                    Message = outcome.ErrorMessage ?? "تعذّر استيراد هذا الصف."
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
                            Message = "تعذّرت معالجة هذا الصف بسبب خطأ غير متوقع، وتم تجاهله."
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
                    result.Message = "تمت قراءة الملف والتحقق منه، لكن تعذّر حفظ النتائج. يرجى إعادة المحاولة أو التواصل مع الدعم الفني.";
                    return result;
                }

                result.Success = true;
                result.Message = result.Errors.Any()
                    ? $"تمت معالجة الملف مع تجاهل {result.SkippedRows} صف. التفاصيل بالأسفل."
                    : "تمت معالجة الملف بنجاح.";
                return result;
            }
        }
    }
}
