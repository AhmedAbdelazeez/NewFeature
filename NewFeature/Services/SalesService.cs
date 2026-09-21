using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NewFeature.Models;
using NewFeature.Services.Repositories;

namespace NewFeature.Services
{
    // Sales Department bulk-upload + KPI service. Parsing is deliberately defensive: the real
    // workbooks supplied by the Sales department are AX/report exports, not clean database-style
    // sheets (report titles, blank rows, headers that start several rows down, Arabic column names,
    // totals/"Grand Total" footer rows, RTL control characters inside date strings, etc). See the
    // per-method comments below for exactly what each parser tolerates and why.
    public class SalesService : ISalesService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<SalesService> _logger;

        // Footer/summary labels that show up in the "Order account" column of the customer roster
        // export and must never be imported as a customer row.
        private static readonly string[] CustomerRosterFooterMarkers =
        {
            "grand total", "total customer turnover", "customer discount"
        };

        public SalesService(ApplicationDbContext context, ILogger<SalesService> logger)
        {
            _context = context;
            _logger = logger;
        }

        #region Duplicate detection

        private static string ComputeHash(byte[] rawBytes)
        {
            using var sha = SHA256.Create();
            var hash = sha.ComputeHash(rawBytes);
            return Convert.ToHexString(hash); // uppercase hex, e.g. "A1B2C3..."
        }

        private async Task<SalesImportBatch?> FindDuplicateBatchAsync(string fileHash, SalesFileType fileType)
        {
            // Must also match FileType: the combined-template upload runs all three parsers against
            // independent copies of the same raw bytes, so the file hash alone is identical across the
            // customer-roster, fleet-capacity and daily-operations sub-uploads of one request. Without
            // this filter, whichever type happens to import first "claims" the hash and every other type
            // gets wrongly skipped as a duplicate on every subsequent upload of the same combined file.
            return await _context.SalesImportBatches
                .Where(b => b.FileHash == fileHash && b.FileType == fileType && b.Status != SalesImportStatus.Failed)
                .OrderByDescending(b => b.UploadedAt)
                .FirstOrDefaultAsync();
        }

        #endregion

        #region File type detection

        public async Task<SalesFileType?> DetectFileTypeAsync(Stream excelStream)
        {
            var position = excelStream.CanSeek ? excelStream.Position : 0;
            try
            {
                using var workbook = new XLWorkbook(excelStream);

                // Customer roster: at least one sheet named after a 4-digit fiscal year whose header
                // area mentions "Order account".
                foreach (var ws in workbook.Worksheets)
                {
                    if (TryParseYearSheetName(ws.Name, out _))
                    {
                        var headerRow = FindHeaderRow(ws, new[] { "order account" }, 20);
                        if (headerRow > 0) return SalesFileType.CustomerRoster;
                    }
                }

                // Fleet capacity: a sheet whose header area mentions bus/category/seat columns.
                foreach (var ws in workbook.Worksheets)
                {
                    var headerRow = FindHeaderRow(ws, new[] { "الحافلة", "عدد الحافلات", "اجمالي المقاعد", "bus type" }, 10);
                    if (headerRow > 0) return SalesFileType.FleetCapacity;
                }

                // Daily operations: a sheet whose header area mentions the rental order number column.
                foreach (var ws in workbook.Worksheets)
                {
                    var headerRow = FindHeaderRow(ws, new[] { "رقم أمر الايجار", "رقم امر الايجار", "rental order" }, 20);
                    if (headerRow > 0) return SalesFileType.DailyOperations;
                }

                return null;
            }
            catch
            {
                return null;
            }
            finally
            {
                if (excelStream.CanSeek) excelStream.Position = position;
            }
        }

        private static bool TryParseYearSheetName(string sheetName, out int year)
        {
            var trimmed = sheetName.Trim();
            if (trimmed.Length == 4 && int.TryParse(trimmed, NumberStyles.None, CultureInfo.InvariantCulture, out year)
                && year is >= 1990 and <= 2100)
            {
                return true;
            }
            year = 0;
            return false;
        }

        // Scans the first `maxRows` rows of a sheet for a row containing any of the given keywords
        // (case-insensitive substring match against each cell's trimmed text) and returns its 1-based
        // row number, or -1 if none of the first `maxRows` rows match. Workbooks in this department
        // routinely have several title/date/blank rows before the real header row.
        private static int FindHeaderRow(IXLWorksheet ws, string[] keywords, int maxRows)
        {
            var lastRow = Math.Min(ws.LastRowUsed()?.RowNumber() ?? 0, maxRows);
            for (int r = 1; r <= lastRow; r++)
            {
                var row = ws.Row(r);
                var lastCell = row.LastCellUsed();
                if (lastCell == null) continue;

                for (int c = 1; c <= lastCell.Address.ColumnNumber; c++)
                {
                    var text = row.Cell(c).GetString().Trim();
                    if (text.Length == 0) continue;
                    foreach (var kw in keywords)
                    {
                        if (text.Contains(kw, StringComparison.OrdinalIgnoreCase)) return r;
                    }
                }
            }
            return -1;
        }

        private static Dictionary<string, int> BuildHeaderMap(IXLWorksheet ws, int headerRow)
        {
            var map = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            var row = ws.Row(headerRow);
            var lastCell = row.LastCellUsed();
            if (lastCell == null) return map;

            for (int c = 1; c <= lastCell.Address.ColumnNumber; c++)
            {
                var text = row.Cell(c).GetString().Trim();
                if (text.Length > 0 && !map.ContainsKey(text)) map[text] = c;
            }
            return map;
        }

        private static int FindColumn(Dictionary<string, int> headers, params string[] searchTerms)
        {
            foreach (var term in searchTerms)
            {
                var match = headers.Keys.FirstOrDefault(k => k.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0);
                if (match != null) return headers[match];
            }
            return -1;
        }

        private static string CellText(IXLWorksheet ws, int row, int col) =>
            col == -1 ? string.Empty : ws.Cell(row, col).GetString().Trim();

        #endregion

        #region Customer Roster upload (العملاء.xlsx)

        public async Task<SalesImportResultDto> BulkUploadCustomerRosterAsync(Stream excelStream, byte[] rawBytes, string fileName, string uploadedByUserId)
        {
            var hash = ComputeHash(rawBytes);
            var duplicate = await FindDuplicateBatchAsync(hash, SalesFileType.CustomerRoster);
            if (duplicate != null)
            {
                return DuplicateResult(duplicate, SalesFileType.CustomerRoster);
            }

            var errors = new List<string>();
            var yearsSeen = new List<int>();
            var sheetsSeen = new List<string>();
            var newRecords = new List<SalesCustomerRecord>();
            int rejectedCount = 0;

            try
            {
                using var workbook = new XLWorkbook(excelStream);

                var yearSheets = workbook.Worksheets
                    .Where(ws => TryParseYearSheetName(ws.Name, out _))
                    .Select(ws => new { Sheet = ws, Year = int.Parse(ws.Name.Trim(), CultureInfo.InvariantCulture) })
                    .OrderBy(x => x.Year)
                    .ToList();

                if (yearSheets.Count == 0)
                {
                    return FailedResult(SalesFileType.CustomerRoster, fileName, uploadedByUserId, hash,
                        "No fiscal-year sheets found (expected sheet names like \"2024\", \"2025\", ...).");
                }

                foreach (var entry in yearSheets)
                {
                    var ws = entry.Sheet;
                    var year = entry.Year;

                    var headerRow = FindHeaderRow(ws, new[] { "order account" }, 20);
                    if (headerRow == -1)
                    {
                        errors.Add($"Sheet '{ws.Name}': could not locate the header row (expected an 'Order account' column) - sheet skipped.");
                        continue;
                    }

                    var headers = BuildHeaderMap(ws, headerRow);
                    int accountCol = FindColumn(headers, "order account", "الحساب", "حساب العميل");
                    int groupCol = FindColumn(headers, "customer group", "مجموعة العملاء", "فئة العميل");
                    int nameCol = FindColumn(headers, "name", "الاسم", "اسم العميل");
                    int currencyCol = FindColumn(headers, "currency", "العملة");

                    if (accountCol == -1 && nameCol == -1)
                    {
                        errors.Add($"Sheet '{ws.Name}': neither an account code nor a name column could be found - sheet skipped.");
                        continue;
                    }

                    sheetsSeen.Add(ws.Name.Trim());
                    yearsSeen.Add(year);

                    var lastRow = ws.LastRowUsed()?.RowNumber() ?? headerRow;
                    for (int r = headerRow + 1; r <= lastRow; r++)
                    {
                        try
                        {
                            var account = CellText(ws, r, accountCol);
                            var name = CellText(ws, r, nameCol);

                            // Blank row (common padding in AX exports).
                            if (account.Length == 0 && name.Length == 0) continue;

                            // Footer/summary rows ("Grand Total", "Total customer turnover", "Customer Discount").
                            if (CustomerRosterFooterMarkers.Any(marker => account.Equals(marker, StringComparison.OrdinalIgnoreCase))) continue;

                            // This AX report export repeats the company-name banner (row 1's text) every
                            // ~60 rows as a mid-sheet page-header artifact, with every other column blank.
                            // A real "Order account" code is always short (e.g. "C000001"); anything this
                            // long can never be a valid code and would otherwise blow past CustomerCode's
                            // 50-char column limit and fail the whole import.
                            if (name.Length == 0 && account.Length > 50)
                            {
                                rejectedCount++;
                                errors.Add($"Sheet '{ws.Name}' row {r}: skipped a repeated report-banner row (not a customer).");
                                continue;
                            }

                            if (name.Length == 0) name = account; // fall back rather than reject - still a real roster row

                            newRecords.Add(new SalesCustomerRecord
                            {
                                CustomerCode = account.Length == 0 ? $"ROW{r}" : account,
                                CustomerName = name,
                                CustomerGroup = groupCol == -1 ? null : CellText(ws, r, groupCol),
                                Currency = currencyCol == -1 ? null : CellText(ws, r, currencyCol),
                                FiscalYear = year
                            });
                        }
                        catch (Exception rowEx)
                        {
                            rejectedCount++;
                            errors.Add($"Sheet '{ws.Name}' row {r}: {rowEx.Message}");
                        }
                    }
                }

                if (newRecords.Count == 0)
                {
                    return FailedResult(SalesFileType.CustomerRoster, fileName, uploadedByUserId, hash,
                        errors.Count > 0 ? string.Join(" | ", errors) : "No customer rows could be parsed from this file.");
                }

                using var tx = await _context.Database.BeginTransactionAsync();
                try
                {
                    // Re-uploading a corrected file for a year replaces that year's roster rather than
                    // appending to it, so customer counts never double up on a legitimate re-import.
                    foreach (var year in yearsSeen.Distinct())
                    {
                        var existing = _context.SalesCustomerRecords.Where(c => c.FiscalYear == year);
                        _context.SalesCustomerRecords.RemoveRange(existing);
                    }
                    await _context.SaveChangesAsync();

                    var batch = new SalesImportBatch
                    {
                        FileName = fileName,
                        FileType = SalesFileType.CustomerRoster,
                        UploadedByUserId = uploadedByUserId,
                        UploadedAt = DateTime.UtcNow,
                        SourceSheet = string.Join(",", sheetsSeen),
                        ReportingPeriod = string.Join(",", yearsSeen.Distinct().OrderBy(y => y)),
                        ImportedRowCount = newRecords.Count,
                        RejectedRowCount = rejectedCount,
                        Status = rejectedCount > 0 ? SalesImportStatus.PartialSuccess : SalesImportStatus.Success,
                        ErrorDetails = errors.Count > 0 ? Truncate(string.Join(" | ", errors), 4000) : null,
                        FileHash = hash
                    };
                    _context.SalesImportBatches.Add(batch);
                    await _context.SaveChangesAsync();

                    foreach (var rec in newRecords) rec.SalesImportBatchId = batch.Id;
                    // Saved in small chunks rather than one AddRange+SaveChanges: a several-thousand-row
                    // roster (this file has ~2600 rows across 5 fiscal years) makes EF Core batch the
                    // insert into multi-row MERGE...OUTPUT statements, which spuriously reports an FK
                    // violation against the just-inserted parent batch row under this connection's MARS
                    // setting (a known SqlClient/EF Core interaction, not a real constraint conflict -
                    // the parent row is committed and visible in this same transaction). Chunking keeps
                    // each MERGE small enough to avoid it.
                    const int chunkSize = 100;
                    for (int i = 0; i < newRecords.Count; i += chunkSize)
                    {
                        _context.SalesCustomerRecords.AddRange(newRecords.Skip(i).Take(chunkSize));
                        await _context.SaveChangesAsync();
                    }

                    await tx.CommitAsync();

                    return new SalesImportResultDto
                    {
                        BatchId = batch.Id,
                        FileType = batch.FileType.ToString(),
                        Status = batch.Status.ToString(),
                        SuccessCount = newRecords.Count,
                        RejectedCount = rejectedCount,
                        Errors = errors,
                        Message = $"Imported {newRecords.Count} customer roster rows across {yearsSeen.Distinct().Count()} fiscal year(s)."
                    };
                }
                catch
                {
                    await tx.RollbackAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Sales customer roster import failed for file {FileName}", fileName);
                return FailedResult(SalesFileType.CustomerRoster, fileName, uploadedByUserId, hash, $"Import failed: {ex.Message}");
            }
        }

        #endregion

        #region Fleet Capacity upload (عدد وانواع الحافلات.xlsx)

        public async Task<SalesImportResultDto> BulkUploadFleetCapacityAsync(Stream excelStream, byte[] rawBytes, string fileName, string uploadedByUserId)
        {
            var hash = ComputeHash(rawBytes);
            var duplicate = await FindDuplicateBatchAsync(hash, SalesFileType.FleetCapacity);
            if (duplicate != null)
            {
                return DuplicateResult(duplicate, SalesFileType.FleetCapacity);
            }

            var errors = new List<string>();
            var newRows = new List<SalesFleetCapacity>();
            int rejectedCount = 0;
            string sheetName = string.Empty;

            try
            {
                using var workbook = new XLWorkbook(excelStream);
                var ws = workbook.Worksheets.FirstOrDefault(w => FindHeaderRow(w, new[] { "الحافلة", "عدد الحافلات" }, 10) > 0)
                          ?? workbook.Worksheets.FirstOrDefault();

                if (ws == null)
                {
                    return FailedResult(SalesFileType.FleetCapacity, fileName, uploadedByUserId, hash, "Excel file has no worksheets.");
                }
                sheetName = ws.Name.Trim();

                var headerRow = FindHeaderRow(ws, new[] { "الحافلة", "عدد الحافلات", "bus type" }, 10);
                if (headerRow == -1)
                {
                    return FailedResult(SalesFileType.FleetCapacity, fileName, uploadedByUserId, hash,
                        "Could not locate the header row (expected columns like 'الحافلة' / 'عدد الحافلات').");
                }

                var headers = BuildHeaderMap(ws, headerRow);
                int busCodeCol = FindColumn(headers, "الحافلة", "bus");
                int busTypeCol = FindColumn(headers, "نوع الحافلة", "bus type");
                int categoryCol = FindColumn(headers, "الفئة", "category");
                int modelCol = FindColumn(headers, "الموديل", "model");
                int busCountCol = FindColumn(headers, "عدد الحافلات", "number of buses");
                int seatsCol = FindColumn(headers, "عدد المقاعد", "seats per bus");
                int totalSeatsCol = FindColumn(headers, "اجمالي المقاعد", "إجمالي المقاعد", "total seats");

                if (busTypeCol == -1 || busCountCol == -1)
                {
                    return FailedResult(SalesFileType.FleetCapacity, fileName, uploadedByUserId, hash,
                        "Required columns (bus type, number of buses) were not found.");
                }

                var lastRow = ws.LastRowUsed()?.RowNumber() ?? headerRow;
                for (int r = headerRow + 1; r <= lastRow; r++)
                {
                    try
                    {
                        var busType = CellText(ws, r, busTypeCol);
                        var category = CellText(ws, r, categoryCol);
                        var busCountText = CellText(ws, r, busCountCol);

                        // Blank padding row.
                        if (busType.Length == 0 && category.Length == 0 && busCountText.Length == 0) continue;

                        // Totals/footer row, e.g. "الإجمالي" in the category column with only the
                        // bus-count/seat totals filled in.
                        if (category.Contains("اجمالي", StringComparison.OrdinalIgnoreCase)
                            || category.Contains("إجمالي", StringComparison.OrdinalIgnoreCase)
                            || category.Contains("total", StringComparison.OrdinalIgnoreCase)
                            || busType.Contains("اجمالي", StringComparison.OrdinalIgnoreCase)
                            || busType.Contains("إجمالي", StringComparison.OrdinalIgnoreCase))
                        {
                            continue;
                        }

                        if (busType.Length == 0)
                        {
                            rejectedCount++;
                            errors.Add($"Row {r}: missing bus type - row skipped.");
                            continue;
                        }

                        if (!int.TryParse(busCountText, NumberStyles.Integer, CultureInfo.InvariantCulture, out var numberOfBuses))
                        {
                            rejectedCount++;
                            errors.Add($"Row {r}: invalid number of buses ('{busCountText}') - row skipped.");
                            continue;
                        }

                        int.TryParse(CellText(ws, r, seatsCol), NumberStyles.Integer, CultureInfo.InvariantCulture, out var seatsPerBus);
                        if (!int.TryParse(CellText(ws, r, totalSeatsCol), NumberStyles.Integer, CultureInfo.InvariantCulture, out var totalSeats))
                        {
                            totalSeats = seatsPerBus * numberOfBuses; // derive when the total-seats column is missing/blank
                        }

                        int? modelYear = null;
                        if (int.TryParse(CellText(ws, r, modelCol), NumberStyles.Integer, CultureInfo.InvariantCulture, out var modelYearParsed))
                        {
                            modelYear = modelYearParsed;
                        }

                        newRows.Add(new SalesFleetCapacity
                        {
                            BusCode = busCodeCol == -1 ? null : CellText(ws, r, busCodeCol),
                            BusType = busType,
                            Category = category.Length == 0 ? "غير محدد" : category,
                            ModelYear = modelYear,
                            NumberOfBuses = numberOfBuses,
                            SeatsPerBus = seatsPerBus,
                            TotalSeats = totalSeats,
                            SnapshotDate = DateTime.UtcNow
                        });
                    }
                    catch (Exception rowEx)
                    {
                        rejectedCount++;
                        errors.Add($"Row {r}: {rowEx.Message}");
                    }
                }

                if (newRows.Count == 0)
                {
                    return FailedResult(SalesFileType.FleetCapacity, fileName, uploadedByUserId, hash,
                        errors.Count > 0 ? string.Join(" | ", errors) : "No fleet capacity rows could be parsed from this file.");
                }

                using var tx = await _context.Database.BeginTransactionAsync();
                try
                {
                    // Fleet capacity is a point-in-time snapshot - a new successful upload replaces
                    // the previous one entirely so dashboard totals never double count.
                    _context.SalesFleetCapacities.RemoveRange(_context.SalesFleetCapacities);
                    await _context.SaveChangesAsync();

                    var batch = new SalesImportBatch
                    {
                        FileName = fileName,
                        FileType = SalesFileType.FleetCapacity,
                        UploadedByUserId = uploadedByUserId,
                        UploadedAt = DateTime.UtcNow,
                        SourceSheet = sheetName,
                        ReportingPeriod = DateTime.UtcNow.ToString("yyyy-MM-dd"),
                        ImportedRowCount = newRows.Count,
                        RejectedRowCount = rejectedCount,
                        Status = rejectedCount > 0 ? SalesImportStatus.PartialSuccess : SalesImportStatus.Success,
                        ErrorDetails = errors.Count > 0 ? Truncate(string.Join(" | ", errors), 4000) : null,
                        FileHash = hash
                    };
                    _context.SalesImportBatches.Add(batch);
                    await _context.SaveChangesAsync();

                    foreach (var row in newRows) row.SalesImportBatchId = batch.Id;
                    _context.SalesFleetCapacities.AddRange(newRows);
                    await _context.SaveChangesAsync();

                    await tx.CommitAsync();

                    return new SalesImportResultDto
                    {
                        BatchId = batch.Id,
                        FileType = batch.FileType.ToString(),
                        Status = batch.Status.ToString(),
                        SuccessCount = newRows.Count,
                        RejectedCount = rejectedCount,
                        Errors = errors,
                        Message = $"Imported {newRows.Count} fleet capacity rows ({newRows.Sum(r => r.NumberOfBuses)} buses)."
                    };
                }
                catch
                {
                    await tx.RollbackAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Sales fleet capacity import failed for file {FileName}", fileName);
                return FailedResult(SalesFileType.FleetCapacity, fileName, uploadedByUserId, hash, $"Import failed: {ex.Message}");
            }
        }

        #endregion

        #region Daily Operations upload (يومية التشغيل.xlsx)

        private static readonly Regex RtlMarksRegex = new("[‎‏؜]", RegexOptions.Compiled);

        public async Task<SalesImportResultDto> BulkUploadDailyOperationsAsync(Stream excelStream, byte[] rawBytes, string fileName, string uploadedByUserId)
        {
            var hash = ComputeHash(rawBytes);
            var duplicate = await FindDuplicateBatchAsync(hash, SalesFileType.DailyOperations);
            if (duplicate != null)
            {
                return DuplicateResult(duplicate, SalesFileType.DailyOperations);
            }

            var errors = new List<string>();
            var newRows = new List<SalesDailyOperation>();
            int rejectedCount = 0;
            string sheetName = string.Empty;

            try
            {
                using var workbook = new XLWorkbook(excelStream);
                var ws = workbook.Worksheets.FirstOrDefault(w => FindHeaderRow(w, new[] { "رقم أمر الايجار", "رقم امر الايجار" }, 20) > 0)
                          ?? workbook.Worksheets.FirstOrDefault();

                if (ws == null)
                {
                    return FailedResult(SalesFileType.DailyOperations, fileName, uploadedByUserId, hash, "Excel file has no worksheets.");
                }
                sheetName = ws.Name.Trim();

                var headerRow = FindHeaderRow(ws, new[] { "رقم أمر الايجار", "رقم امر الايجار", "rental order" }, 20);
                if (headerRow == -1)
                {
                    return FailedResult(SalesFileType.DailyOperations, fileName, uploadedByUserId, hash,
                        "Could not locate the header row (expected a 'رقم أمر الايجار' column).");
                }

                var headers = BuildHeaderMap(ws, headerRow);
                int orderCol = FindColumn(headers, "رقم أمر الايجار", "رقم امر الايجار");
                int confirmCol = FindColumn(headers, "رقم التعميد");
                int requestTypeCol = FindColumn(headers, "طلب العميل");
                int nameCol = FindColumn(headers, "أسم العميل", "اسم العميل");
                int pointCol = FindColumn(headers, "المنفذ");
                int directionCol = FindColumn(headers, "الاتجاه");
                int notesCol = FindColumn(headers, "ملاحظات");
                int busTypeCol = FindColumn(headers, "نوع الحافلة");
                int opCountCol = FindColumn(headers, "العدد التشغيلى", "العدد التشغيلي");
                int scheduledCol = FindColumn(headers, "الحافلات المجدولة");
                int dateCol = FindColumn(headers, "تاريخ التنفيذ");
                int timeCol = FindColumn(headers, "وقت التنفيذ");

                if (nameCol == -1)
                {
                    return FailedResult(SalesFileType.DailyOperations, fileName, uploadedByUserId, hash,
                        "Required customer name column ('أسم العميل') was not found.");
                }

                var affectedDates = new HashSet<DateTime>();
                var lastRow = ws.LastRowUsed()?.RowNumber() ?? headerRow;
                for (int r = headerRow + 1; r <= lastRow; r++)
                {
                    try
                    {
                        var customerName = CellText(ws, r, nameCol);
                        var busTypeVal = CellText(ws, r, busTypeCol);
                        var opCountText = CellText(ws, r, opCountCol);

                        if (customerName.Length == 0 && busTypeVal.Length == 0 && opCountText.Length == 0) continue; // blank row

                        // Footer/summary row, e.g. "أجمالى عدد التشغيلات" appearing in the bus-type column.
                        if (busTypeVal.Contains("اجمالى", StringComparison.OrdinalIgnoreCase)
                            || busTypeVal.Contains("أجمالى", StringComparison.OrdinalIgnoreCase)
                            || busTypeVal.Contains("إجمالي", StringComparison.OrdinalIgnoreCase))
                        {
                            continue;
                        }

                        if (customerName.Length == 0)
                        {
                            rejectedCount++;
                            errors.Add($"Row {r}: missing customer name - row skipped.");
                            continue;
                        }

                        var executionDate = ParseCellDate(ws, r, dateCol);
                        if (executionDate == null)
                        {
                            rejectedCount++;
                            errors.Add($"Row {r}: could not parse execution date - row skipped.");
                            continue;
                        }

                        int.TryParse(opCountText, NumberStyles.Integer, CultureInfo.InvariantCulture, out var opCount);
                        int.TryParse(CellText(ws, r, scheduledCol), NumberStyles.Integer, CultureInfo.InvariantCulture, out var scheduled);

                        newRows.Add(new SalesDailyOperation
                        {
                            RentalOrderNumber = orderCol == -1 ? null : CellText(ws, r, orderCol),
                            ConfirmationNumber = confirmCol == -1 ? null : CellText(ws, r, confirmCol),
                            RequestType = requestTypeCol == -1 ? null : CellText(ws, r, requestTypeCol),
                            CustomerName = customerName,
                            ExecutionPoint = pointCol == -1 ? null : CellText(ws, r, pointCol),
                            Direction = directionCol == -1 ? null : CellText(ws, r, directionCol),
                            Notes = notesCol == -1 ? null : CellText(ws, r, notesCol),
                            BusTypeCode = busTypeVal.Length == 0 ? null : busTypeVal,
                            OperationalCount = opCount,
                            ScheduledBuses = scheduled,
                            ExecutionDate = executionDate.Value.Date,
                            ExecutionTime = ParseCellTime(ws, r, timeCol)
                        });
                        affectedDates.Add(executionDate.Value.Date);
                    }
                    catch (Exception rowEx)
                    {
                        rejectedCount++;
                        errors.Add($"Row {r}: {rowEx.Message}");
                    }
                }

                if (newRows.Count == 0)
                {
                    return FailedResult(SalesFileType.DailyOperations, fileName, uploadedByUserId, hash,
                        errors.Count > 0 ? string.Join(" | ", errors) : "No daily-operations rows could be parsed from this file.");
                }

                using var tx = await _context.Database.BeginTransactionAsync();
                try
                {
                    // Re-uploading a day's log replaces that day's rows (so a correction doesn't
                    // double count) while leaving other days' history untouched.
                    var existing = _context.SalesDailyOperations.Where(o => affectedDates.Contains(o.ExecutionDate));
                    _context.SalesDailyOperations.RemoveRange(existing);
                    await _context.SaveChangesAsync();

                    var batch = new SalesImportBatch
                    {
                        FileName = fileName,
                        FileType = SalesFileType.DailyOperations,
                        UploadedByUserId = uploadedByUserId,
                        UploadedAt = DateTime.UtcNow,
                        SourceSheet = sheetName,
                        ReportingPeriod = string.Join(",", affectedDates.OrderBy(d => d).Select(d => d.ToString("yyyy-MM-dd"))),
                        ImportedRowCount = newRows.Count,
                        RejectedRowCount = rejectedCount,
                        Status = rejectedCount > 0 ? SalesImportStatus.PartialSuccess : SalesImportStatus.Success,
                        ErrorDetails = errors.Count > 0 ? Truncate(string.Join(" | ", errors), 4000) : null,
                        FileHash = hash
                    };
                    _context.SalesImportBatches.Add(batch);
                    await _context.SaveChangesAsync();

                    foreach (var row in newRows) row.SalesImportBatchId = batch.Id;
                    _context.SalesDailyOperations.AddRange(newRows);
                    await _context.SaveChangesAsync();

                    await tx.CommitAsync();

                    return new SalesImportResultDto
                    {
                        BatchId = batch.Id,
                        FileType = batch.FileType.ToString(),
                        Status = batch.Status.ToString(),
                        SuccessCount = newRows.Count,
                        RejectedCount = rejectedCount,
                        Errors = errors,
                        Message = $"Imported {newRows.Count} daily operations rows across {affectedDates.Count} day(s)."
                    };
                }
                catch
                {
                    await tx.RollbackAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Sales daily operations import failed for file {FileName}", fileName);
                return FailedResult(SalesFileType.DailyOperations, fileName, uploadedByUserId, hash, $"Import failed: {ex.Message}");
            }
        }

        private static DateTime? ParseCellDate(IXLWorksheet ws, int row, int col)
        {
            if (col == -1) return null;
            var cell = ws.Cell(row, col);
            if (cell.DataType == XLDataType.DateTime)
            {
                return cell.GetDateTime();
            }

            var raw = RtlMarksRegex.Replace(cell.GetString(), string.Empty).Trim();
            if (raw.Length == 0) return null;

            // Accepts Gregorian or Hijri dates, in any of the usual written formats.
            return FlexibleDateParser.Parse(raw);
        }

        private static TimeSpan? ParseCellTime(IXLWorksheet ws, int row, int col)
        {
            if (col == -1) return null;
            var cell = ws.Cell(row, col);
            if (cell.DataType == XLDataType.DateTime)
            {
                return cell.GetDateTime().TimeOfDay;
            }

            var raw = RtlMarksRegex.Replace(cell.GetString(), string.Empty).Trim();
            if (raw.Length == 0) return null;

            return TimeSpan.TryParse(raw, CultureInfo.InvariantCulture, out var ts) ? ts : null;
        }

        #endregion

        #region Combined upload (single template - Sales_Department_Excel_Templates.xlsx)

        // One template, one endpoint: runs the three parsers above against independent copies of the
        // same uploaded bytes. Each parser finds its own sheet by header content regardless of what
        // else is in the workbook (see DetectFileTypeAsync), so no parser changes were needed - a
        // combined workbook missing one of the three sheets just yields a Failed sub-result for that
        // section, which is reflected in the aggregate rather than aborting the other sections.
        public async Task<SalesImportResultDto> BulkUploadCombinedAsync(Stream excelStream, byte[] rawBytes, string fileName, string uploadedByUserId)
        {
            using var customersStream = new MemoryStream(rawBytes);
            using var fleetStream = new MemoryStream(rawBytes);
            using var dailyOpsStream = new MemoryStream(rawBytes);

            var customers = await BulkUploadCustomerRosterAsync(customersStream, rawBytes, fileName, uploadedByUserId);
            var fleet = await BulkUploadFleetCapacityAsync(fleetStream, rawBytes, fileName, uploadedByUserId);
            var dailyOps = await BulkUploadDailyOperationsAsync(dailyOpsStream, rawBytes, fileName, uploadedByUserId);

            var sections = new[]
            {
                ("Customer Roster", customers),
                ("Fleet Capacity", fleet),
                ("Daily Operations", dailyOps)
            };

            var succeededSections = sections.Where(s => s.Item2.Status != SalesImportStatus.Failed.ToString()).ToList();
            var errors = sections.SelectMany(s => s.Item2.Errors.Select(e => $"[{s.Item1}] {e}")).ToList();
            if (customers.Status == SalesImportStatus.Failed.ToString()) errors.Add($"[Customer Roster] {customers.Message}");
            if (fleet.Status == SalesImportStatus.Failed.ToString()) errors.Add($"[Fleet Capacity] {fleet.Message}");
            if (dailyOps.Status == SalesImportStatus.Failed.ToString()) errors.Add($"[Daily Operations] {dailyOps.Message}");

            var status = succeededSections.Count == sections.Length
                ? SalesImportStatus.Success
                : succeededSections.Count > 0
                    ? SalesImportStatus.PartialSuccess
                    : SalesImportStatus.Failed;

            return new SalesImportResultDto
            {
                FileType = "Combined",
                Status = status.ToString(),
                SuccessCount = sections.Sum(s => s.Item2.SuccessCount),
                RejectedCount = sections.Sum(s => s.Item2.RejectedCount),
                Errors = errors,
                Message = string.Join(" | ", sections.Select(s => $"{s.Item1}: {s.Item2.Message}"))
            };
        }

        #endregion

        #region Helpers shared by all three uploaders

        private static string Truncate(string value, int maxLength) =>
            value.Length <= maxLength ? value : value.Substring(0, maxLength);

        private static SalesImportResultDto DuplicateResult(SalesImportBatch duplicate, SalesFileType fileType) => new()
        {
            BatchId = duplicate.Id,
            FileType = fileType.ToString(),
            Status = SalesImportStatus.DuplicateSkipped.ToString(),
            SuccessCount = 0,
            RejectedCount = 0,
            Errors = new List<string>(),
            Message = $"This exact file was already imported on {duplicate.UploadedAt:yyyy-MM-dd HH:mm} UTC (batch #{duplicate.Id}). No changes were made."
        };

        private SalesImportResultDto FailedResult(SalesFileType fileType, string fileName, string uploadedByUserId, string hash, string message)
        {
            // Called from a catch block after a mid-import DbUpdateException, so the change tracker can
            // still hold entities from the rolled-back transaction (e.g. customer records referencing a
            // batch row that no longer exists). Clear it first so this unrelated failure-log insert can't
            // also try to re-save that stale graph and mask the real error behind a second FK violation.
            _context.ChangeTracker.Clear();

            // Record the failed attempt for traceability/logging even though nothing was imported.
            var batch = new SalesImportBatch
            {
                FileName = fileName,
                FileType = fileType,
                UploadedByUserId = uploadedByUserId,
                UploadedAt = DateTime.UtcNow,
                ImportedRowCount = 0,
                RejectedRowCount = 0,
                Status = SalesImportStatus.Failed,
                ErrorDetails = Truncate(message, 4000),
                FileHash = hash
            };
            _context.SalesImportBatches.Add(batch);
            _context.SaveChanges();

            _logger.LogWarning("Sales import failed: {Message} (file: {FileName})", message, fileName);

            return new SalesImportResultDto
            {
                BatchId = batch.Id,
                FileType = fileType.ToString(),
                Status = SalesImportStatus.Failed.ToString(),
                SuccessCount = 0,
                RejectedCount = 0,
                Errors = new List<string> { message },
                Message = message
            };
        }

        #endregion

        #region Read APIs

        public async Task<PagedResultDto<SalesImportBatchDto>> GetImportHistoryAsync(int page, int pageSize)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 200) pageSize = 200;

            var query = _context.SalesImportBatches.AsNoTracking().OrderByDescending(b => b.UploadedAt);
            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(b => new SalesImportBatchDto
                {
                    Id = b.Id,
                    FileName = b.FileName,
                    FileType = b.FileType.ToString(),
                    UploadedByUserId = b.UploadedByUserId,
                    UploadedAt = b.UploadedAt,
                    SourceSheet = b.SourceSheet,
                    ReportingPeriod = b.ReportingPeriod,
                    ImportedRowCount = b.ImportedRowCount,
                    RejectedRowCount = b.RejectedRowCount,
                    Status = b.Status.ToString(),
                    ErrorDetails = b.ErrorDetails
                })
                .ToListAsync();

            return new PagedResultDto<SalesImportBatchDto> { Items = items, TotalCount = totalCount, Page = page, PageSize = pageSize };
        }

        public async Task<SalesCustomersOverviewDto> GetCustomersOverviewAsync(int? year)
        {
            var records = await _context.SalesCustomerRecords.AsNoTracking().ToListAsync();
            if (records.Count == 0) return new SalesCustomersOverviewDto { HasData = false };

            var byYear = records.GroupBy(r => r.FiscalYear).OrderBy(g => g.Key).ToList();
            var years = new List<SalesCustomerYearSummaryDto>();

            HashSet<string>? previousCodes = null;
            foreach (var g in byYear)
            {
                var codes = g.Select(r => r.CustomerCode).ToHashSet(StringComparer.OrdinalIgnoreCase);
                int newCount = previousCodes == null ? codes.Count : codes.Except(previousCodes).Count();
                int retained = previousCodes == null ? 0 : codes.Intersect(previousCodes).Count();
                int churned = previousCodes == null ? 0 : previousCodes.Except(codes).Count();

                double? yoy = null;
                if (previousCodes != null && previousCodes.Count > 0)
                {
                    yoy = Math.Round(((double)(codes.Count - previousCodes.Count) / previousCodes.Count) * 100.0, 1);
                }

                years.Add(new SalesCustomerYearSummaryDto
                {
                    FiscalYear = g.Key,
                    ActiveCustomers = codes.Count,
                    NewCustomers = newCount,
                    RetainedCustomers = retained,
                    ChurnedCustomers = churned,
                    YoYGrowthPercent = yoy,
                    SegmentDistribution = g.GroupBy(r => string.IsNullOrWhiteSpace(r.CustomerGroup) ? "غير مصنف" : r.CustomerGroup!)
                        .ToDictionary(sg => sg.Key, sg => sg.Count())
                });

                previousCodes = codes;
            }

            var targetYear = year ?? years.Last().FiscalYear;
            var targetYearRecords = records.Where(r => r.FiscalYear == targetYear).ToList();
            var totalForTargetYear = Math.Max(targetYearRecords.Count, 1);
            var topSegments = targetYearRecords
                .GroupBy(r => string.IsNullOrWhiteSpace(r.CustomerGroup) ? "غير مصنف" : r.CustomerGroup!)
                .Select(g => new SalesTopSegmentDto
                {
                    CustomerGroup = g.Key,
                    CustomerCount = g.Count(),
                    SharePercent = Math.Round((double)g.Count() / totalForTargetYear * 100.0, 1)
                })
                .OrderByDescending(s => s.CustomerCount)
                .ToList();

            return new SalesCustomersOverviewDto { HasData = true, Years = years, TopSegments = topSegments };
        }

        public async Task<SalesFleetCapacitySummaryDto> GetFleetCapacitySummaryAsync(string? category, string? busType, string? model)
        {
            var query = _context.SalesFleetCapacities.AsNoTracking().AsQueryable();
            if (!string.IsNullOrWhiteSpace(category)) query = query.Where(f => f.Category == category);
            if (!string.IsNullOrWhiteSpace(busType)) query = query.Where(f => f.BusType == busType);
            if (!string.IsNullOrWhiteSpace(model) && int.TryParse(model, out var modelYear)) query = query.Where(f => f.ModelYear == modelYear);

            var rows = await query.ToListAsync();
            if (rows.Count == 0) return new SalesFleetCapacitySummaryDto { HasData = false };

            var totalBuses = rows.Sum(r => r.NumberOfBuses);
            return new SalesFleetCapacitySummaryDto
            {
                HasData = true,
                SnapshotDate = rows.Max(r => r.SnapshotDate),
                TotalBuses = totalBuses,
                TotalSeats = rows.Sum(r => r.TotalSeats),
                AverageSeatsPerBus = totalBuses > 0 ? Math.Round(rows.Sum(r => r.TotalSeats) / (double)totalBuses, 1) : 0,
                Items = rows.Select(r => new SalesFleetCapacityItemDto
                {
                    BusCode = r.BusCode,
                    BusType = r.BusType,
                    Category = r.Category,
                    ModelYear = r.ModelYear,
                    NumberOfBuses = r.NumberOfBuses,
                    SeatsPerBus = r.SeatsPerBus,
                    TotalSeats = r.TotalSeats
                }).OrderByDescending(r => r.NumberOfBuses).ToList(),
                BusesByCategory = rows.GroupBy(r => r.Category).ToDictionary(g => g.Key, g => g.Sum(r => r.NumberOfBuses)),
                BusesByType = rows.GroupBy(r => r.BusType).ToDictionary(g => g.Key, g => g.Sum(r => r.NumberOfBuses))
            };
        }

        public async Task<PagedResultDto<SalesDailyOperationDto>> GetDailyOperationsAsync(int page, int pageSize, DateTime? fromDate, DateTime? toDate)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 200) pageSize = 200;

            var query = _context.SalesDailyOperations.AsNoTracking().AsQueryable();
            if (fromDate.HasValue) query = query.Where(o => o.ExecutionDate >= fromDate.Value.Date);
            if (toDate.HasValue) query = query.Where(o => o.ExecutionDate < toDate.Value.Date.AddDays(1));

            var totalCount = await query.CountAsync();
            var items = await query
                .OrderByDescending(o => o.ExecutionDate).ThenBy(o => o.CustomerName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(o => new SalesDailyOperationDto
                {
                    Id = o.Id,
                    RentalOrderNumber = o.RentalOrderNumber,
                    CustomerName = o.CustomerName,
                    ExecutionPoint = o.ExecutionPoint,
                    Direction = o.Direction,
                    BusTypeCode = o.BusTypeCode,
                    OperationalCount = o.OperationalCount,
                    ScheduledBuses = o.ScheduledBuses,
                    ExecutionDate = o.ExecutionDate
                })
                .ToListAsync();

            return new PagedResultDto<SalesDailyOperationDto> { Items = items, TotalCount = totalCount, Page = page, PageSize = pageSize };
        }

        public async Task<SalesKpisDto> GetSalesKpisAsync()
        {
            var dto = new SalesKpisDto();

            var customers = await GetCustomersOverviewAsync(null);
            dto.HasCustomerData = customers.HasData;
            if (customers.HasData)
            {
                var latest = customers.Years.Last();
                dto.LatestFiscalYear = latest.FiscalYear;
                dto.TotalActiveCustomersActual = latest.ActiveCustomers;
                dto.NewCustomersActual = latest.NewCustomers;
                dto.ChurnedCustomersActual = latest.ChurnedCustomers;
                dto.CustomerGrowthYoYPercent = latest.YoYGrowthPercent;

                var previousYear = customers.Years.Count > 1 ? customers.Years[^2] : null;
                dto.TotalActiveCustomersTarget = previousYear?.ActiveCustomers ?? latest.ActiveCustomers;
                dto.CustomerRetentionRateActual = previousYear != null && previousYear.ActiveCustomers > 0
                    ? Math.Round((double)latest.RetainedCustomers / previousYear.ActiveCustomers * 100.0, 1)
                    : 0;

                var topSegment = customers.TopSegments.FirstOrDefault();
                if (topSegment != null)
                {
                    dto.TopCustomerSegment = topSegment.CustomerGroup;
                    dto.TopCustomerSegmentSharePercent = topSegment.SharePercent;
                }
            }

            var fleet = await GetFleetCapacitySummaryAsync(null, null, null);
            dto.HasFleetData = fleet.HasData;
            if (fleet.HasData)
            {
                dto.TotalFleetBuses = fleet.TotalBuses;
                dto.TotalFleetSeats = fleet.TotalSeats;
                dto.AverageSeatsPerBus = fleet.AverageSeatsPerBus;
            }

            var latestOpsDate = await _context.SalesDailyOperations.AsNoTracking()
                .OrderByDescending(o => o.ExecutionDate)
                .Select(o => (DateTime?)o.ExecutionDate)
                .FirstOrDefaultAsync();
            dto.HasDailyOperationsData = latestOpsDate != null;
            if (latestOpsDate != null)
            {
                dto.LatestDailyOperationsDate = latestOpsDate;
                dto.LatestDailyOperationsCount = await _context.SalesDailyOperations.AsNoTracking()
                    .Where(o => o.ExecutionDate == latestOpsDate.Value)
                    .SumAsync(o => o.OperationalCount);
            }

            return dto;
        }

        #endregion
    }
}
