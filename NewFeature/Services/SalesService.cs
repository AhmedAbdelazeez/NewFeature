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
using NewFeature.Services.ExcelImport;
using NewFeature.Services.Repositories;

namespace NewFeature.Services
{
    // Sales department: three uploads (customer roster, fleet capacity, daily operations), each with
    // its own page, its records index + add/edit/delete, and the KPIs the executive dashboard shows.
    //
    // Parsing stays tolerant of the files Sales really has: AX exports whose header row starts a few
    // rows down, Arabic or English headers, "Grand Total" footers, RTL marks inside dates. A row is
    // judged by the same Validate* rules the page's add/edit form uses, so a record can never be
    // accepted by one path and rejected by the other.
    public class SalesService : ISalesService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<SalesService> _logger;

        // Rows typed in on a page still need an owning import batch; they all share one per template.
        private const string ManualEntryHash = "MANUAL-ENTRY";

        private const int HeaderSearchRows = 20;

        private static readonly Regex RtlMarks = new("[‎‏؜‪-‮]", RegexOptions.Compiled);
        private static readonly Regex Spaces = new(@"\s+", RegexOptions.Compiled);

        public SalesService(ApplicationDbContext context, ILogger<SalesService> logger)
        {
            _context = context;
            _logger = logger;
        }

        #region Shared validation (upload rows and the page's add/edit form)

        internal static List<FieldErrorDto> ValidateCustomer(SalesCustomerDto dto)
        {
            var errors = new List<FieldErrorDto>();
            if (string.IsNullOrWhiteSpace(dto.CustomerCode)) errors.Add("customerCode", "رقم حساب العميل (Order Account) مطلوب.");
            else if (dto.CustomerCode.Trim().Length > 50) errors.Add("customerCode", "رقم حساب العميل أطول من 50 حرفاً.");

            if (string.IsNullOrWhiteSpace(dto.CustomerName)) errors.Add("customerName", "اسم العميل (Name) مطلوب.");
            else if (dto.CustomerName.Trim().Length > 300) errors.Add("customerName", "اسم العميل أطول من 300 حرف.");

            if ((dto.CustomerGroup?.Trim().Length ?? 0) > 50) errors.Add("customerGroup", "فئة العميل أطول من 50 حرفاً.");
            if ((dto.Currency?.Trim().Length ?? 0) > 10) errors.Add("currency", "العملة أطول من 10 أحرف.");

            if (dto.FiscalYear == null) errors.Add("fiscalYear", "السنة المالية مطلوبة.");
            else if (dto.FiscalYear is < 1990 or > 2100) errors.Add("fiscalYear", "السنة المالية غير صحيحة.");
            return errors;
        }

        internal static List<FieldErrorDto> ValidateFleet(SalesFleetCapacityDto dto)
        {
            var errors = new List<FieldErrorDto>();
            if (string.IsNullOrWhiteSpace(dto.BusType)) errors.Add("busType", "نوع الحافلة (Bus Type) مطلوب.");
            else if (dto.BusType.Trim().Length > 100) errors.Add("busType", "نوع الحافلة أطول من 100 حرف.");
            if ((dto.BusCode?.Trim().Length ?? 0) > 100) errors.Add("busCode", "كود الحافلة أطول من 100 حرف.");
            if ((dto.Category?.Trim().Length ?? 0) > 100) errors.Add("category", "الفئة أطول من 100 حرف.");

            if (dto.NumberOfBuses == null) errors.Add("numberOfBuses", "عدد الحافلات (Number of Buses) مطلوب.");
            else if (dto.NumberOfBuses <= 0) errors.Add("numberOfBuses", "عدد الحافلات يجب أن يكون أكبر من صفر.");

            if (dto.SeatsPerBus == null) errors.Add("seatsPerBus", "عدد المقاعد للحافلة (Seats per Bus) مطلوب.");
            else if (dto.SeatsPerBus <= 0) errors.Add("seatsPerBus", "عدد المقاعد يجب أن يكون أكبر من صفر.");

            if (dto.TotalSeats is < 0) errors.Add("totalSeats", "إجمالي المقاعد لا يمكن أن يكون سالباً.");
            if (dto.ModelYear is < 1980 or > 2100) errors.Add("modelYear", "سنة الموديل غير صحيحة.");
            return errors;
        }

        internal static List<FieldErrorDto> ValidateOperation(SalesDailyOperationDto dto)
        {
            var errors = new List<FieldErrorDto>();
            if (string.IsNullOrWhiteSpace(dto.CustomerName)) errors.Add("customerName", "اسم العميل (Customer Name) مطلوب.");
            else if (dto.CustomerName.Trim().Length > 300) errors.Add("customerName", "اسم العميل أطول من 300 حرف.");

            if (dto.ExecutionDate == null) errors.Add("executionDate", "تاريخ التنفيذ (Execution Date) مطلوب أو غير مفهوم.");

            if (dto.OperationalCount == null) errors.Add("operationalCount", "عدد الحافلات المطلوبة (Operational Count) مطلوب.");
            else if (dto.OperationalCount < 0) errors.Add("operationalCount", "عدد الحافلات المطلوبة لا يمكن أن يكون سالباً.");
            if (dto.ScheduledBuses is < 0) errors.Add("scheduledBuses", "الحافلات المجدولة لا يمكن أن تكون سالبة.");

            if (!string.IsNullOrWhiteSpace(dto.ExecutionTime) && ParseTime(dto.ExecutionTime) == null)
                errors.Add("executionTime", "وقت التنفيذ غير مفهوم، اكتبه مثل 06:00.");

            CheckLength(errors, "rentalOrderNumber", dto.RentalOrderNumber, 50, "رقم أمر الإيجار");
            CheckLength(errors, "confirmationNumber", dto.ConfirmationNumber, 50, "رقم التعميد");
            CheckLength(errors, "requestType", dto.RequestType, 100, "نوع الطلب");
            CheckLength(errors, "executionPoint", dto.ExecutionPoint, 200, "المنفذ");
            CheckLength(errors, "direction", dto.Direction, 300, "الاتجاه");
            CheckLength(errors, "busTypeCode", dto.BusTypeCode, 50, "نوع الحافلة");
            CheckLength(errors, "notes", dto.Notes, 500, "الملاحظات");
            return errors;
        }

        // A cell that holds text where a number belongs reports that, not also "مطلوب".
        private static void ReplaceError(List<FieldErrorDto> errors, string field, string message)
        {
            errors.RemoveAll(e => e.Field == field);
            errors.Add(field, message);
        }

        private static void CheckLength(List<FieldErrorDto> errors, string field, string? value, int max, string label)
        {
            if ((value?.Trim().Length ?? 0) > max) errors.Add(field, $"{label} أطول من {max} حرفاً.");
        }

        private static string? Clean(string? value)
        {
            var trimmed = value?.Trim();
            return string.IsNullOrEmpty(trimmed) ? null : trimmed;
        }

        private static void Apply(SalesCustomerDto dto, SalesCustomerRecord e)
        {
            e.CustomerCode = dto.CustomerCode!.Trim();
            e.CustomerName = dto.CustomerName!.Trim();
            e.CustomerGroup = Clean(dto.CustomerGroup);
            e.Currency = Clean(dto.Currency);
            e.FiscalYear = dto.FiscalYear!.Value;
        }

        private static void Apply(SalesFleetCapacityDto dto, SalesFleetCapacity e)
        {
            e.BusCode = Clean(dto.BusCode);
            e.BusType = dto.BusType!.Trim();
            e.Category = Clean(dto.Category) ?? "غير محدد";
            e.ModelYear = dto.ModelYear;
            e.NumberOfBuses = dto.NumberOfBuses!.Value;
            e.SeatsPerBus = dto.SeatsPerBus!.Value;
            e.TotalSeats = dto.TotalSeats ?? dto.NumberOfBuses!.Value * dto.SeatsPerBus!.Value;
        }

        private static void Apply(SalesDailyOperationDto dto, SalesDailyOperation e)
        {
            e.RentalOrderNumber = Clean(dto.RentalOrderNumber);
            e.ConfirmationNumber = Clean(dto.ConfirmationNumber);
            e.RequestType = Clean(dto.RequestType);
            e.CustomerName = dto.CustomerName!.Trim();
            e.ExecutionPoint = Clean(dto.ExecutionPoint);
            e.Direction = Clean(dto.Direction);
            e.BusTypeCode = Clean(dto.BusTypeCode);
            e.OperationalCount = dto.OperationalCount!.Value;
            e.ScheduledBuses = dto.ScheduledBuses ?? 0;
            e.ExecutionDate = dto.ExecutionDate!.Value.Date;
            e.ExecutionTime = ParseTime(dto.ExecutionTime);
            e.Notes = Clean(dto.Notes);
        }

        private static SalesCustomerDto ToDto(SalesCustomerRecord e) => new()
        {
            Id = e.Id, CustomerCode = e.CustomerCode, CustomerName = e.CustomerName,
            CustomerGroup = e.CustomerGroup, Currency = e.Currency, FiscalYear = e.FiscalYear
        };

        private static SalesFleetCapacityDto ToDto(SalesFleetCapacity e) => new()
        {
            Id = e.Id, BusCode = e.BusCode, BusType = e.BusType, Category = e.Category, ModelYear = e.ModelYear,
            NumberOfBuses = e.NumberOfBuses, SeatsPerBus = e.SeatsPerBus, TotalSeats = e.TotalSeats
        };

        private static SalesDailyOperationDto ToDto(SalesDailyOperation e) => new()
        {
            Id = e.Id, RentalOrderNumber = e.RentalOrderNumber, ConfirmationNumber = e.ConfirmationNumber,
            RequestType = e.RequestType, CustomerName = e.CustomerName, ExecutionPoint = e.ExecutionPoint,
            Direction = e.Direction, BusTypeCode = e.BusTypeCode, OperationalCount = e.OperationalCount,
            ScheduledBuses = e.ScheduledBuses, ExecutionDate = e.ExecutionDate,
            ExecutionTime = e.ExecutionTime?.ToString(@"hh\:mm"), Notes = e.Notes
        };

        #endregion

        #region Reading the workbook

        // Header text compared the way people actually vary it: case, spacing, RTL marks and the
        // Arabic letters that are routinely swapped (أ/إ/آ -> ا, ة -> ه, ى -> ي).
        private static string Normalize(string? text)
        {
            if (string.IsNullOrEmpty(text)) return string.Empty;
            var s = Spaces.Replace(RtlMarks.Replace(text, string.Empty), " ").Trim().ToLowerInvariant();
            return s.Replace('أ', 'ا').Replace('إ', 'ا').Replace('آ', 'ا').Replace('ة', 'ه').Replace('ى', 'ي');
        }

        private sealed class SheetHeader
        {
            public int Row { get; init; }
            public Dictionary<string, int> Columns { get; init; } = new();
            public Dictionary<string, string> Labels { get; init; } = new();
        }

        // Maps each template column to a column of `row`: exact header names first, then keywords,
        // and a column claimed once is never matched again (so "Bus Type" can't also be read as
        // "Bus Type Code"). Returns null when any required column is missing.
        private static SheetHeader? ResolveHeader(IXLWorksheet ws, int row, ExcelTemplateDefinition def)
        {
            var lastCell = ws.Row(row).LastCellUsed();
            if (lastCell == null) return null;

            var headers = new List<(string Text, string Raw, int Col)>();
            for (int c = 1; c <= lastCell.Address.ColumnNumber; c++)
            {
                var raw = ws.Cell(row, c).GetString().Trim();
                if (raw.Length > 0) headers.Add((Normalize(raw), raw, c));
            }

            var columns = new Dictionary<string, int>();
            var labels = new Dictionary<string, string>();
            var claimed = new HashSet<int>();

            foreach (var exact in new[] { true, false })
            {
                foreach (var column in def.Columns)
                {
                    if (columns.ContainsKey(column.Key)) continue;
                    foreach (var alias in column.HeaderAliases.Select(Normalize))
                    {
                        var match = headers.FirstOrDefault(h => !claimed.Contains(h.Col)
                            && (exact ? h.Text == alias : h.Text.Contains(alias, StringComparison.Ordinal)));
                        if (match.Col == 0) continue;
                        columns[column.Key] = match.Col;
                        labels[column.Key] = match.Raw;
                        claimed.Add(match.Col);
                        break;
                    }
                }
            }

            if (def.Columns.Where(c => c.Required).Any(c => !columns.ContainsKey(c.Key))) return null;
            return new SheetHeader { Row = row, Columns = columns, Labels = labels };
        }

        // The header row can start a few rows down (report titles, dates, blank rows above it).
        private static SheetHeader? FindHeader(IXLWorksheet ws, ExcelTemplateDefinition def)
        {
            var last = Math.Min(ws.LastRowUsed()?.RowNumber() ?? 0, HeaderSearchRows);
            for (int r = 1; r <= last; r++)
            {
                var header = ResolveHeader(ws, r, def);
                if (header != null) return header;
            }
            return null;
        }

        private static string RequiredHeadersText(ExcelTemplateDefinition def) =>
            string.Join("، ", def.Columns.Where(c => c.Required).Select(c => c.HeaderAliases[0]));

        private static string Text(IXLWorksheet ws, int row, SheetHeader h, string key) =>
            h.Columns.TryGetValue(key, out var col)
                ? Spaces.Replace(RtlMarks.Replace(ws.Cell(row, col).GetString(), string.Empty), " ").Trim()
                : string.Empty;

        // A whole number from a numeric or text cell. `invalid` is set when the cell holds something
        // that is not one (so the row is rejected with a reason rather than silently read as 0).
        private static int? Int(IXLWorksheet ws, int row, SheetHeader h, string key, ref bool invalid)
        {
            if (!h.Columns.TryGetValue(key, out var col)) return null;
            var cell = ws.Cell(row, col);
            if (cell.DataType == XLDataType.Number)
            {
                var d = cell.GetDouble();
                if (Math.Abs(d - Math.Round(d)) < 0.0001) return (int)Math.Round(d);
                invalid = true;
                return null;
            }
            var text = Text(ws, row, h, key).Replace(",", string.Empty);
            if (text.Length == 0) return null;
            if (decimal.TryParse(text, NumberStyles.Number, CultureInfo.InvariantCulture, out var value) && value == Math.Round(value))
                return (int)value;
            invalid = true;
            return null;
        }

        private static DateTime? Date(IXLWorksheet ws, int row, SheetHeader h, string key)
        {
            if (!h.Columns.TryGetValue(key, out var col)) return null;
            var cell = ws.Cell(row, col);
            if (cell.DataType == XLDataType.DateTime) return cell.GetDateTime();
            if (cell.DataType == XLDataType.Number)
            {
                var serial = cell.GetDouble();
                if (serial > 20000 && serial < 80000) return DateTime.FromOADate(serial);
            }
            var raw = Text(ws, row, h, key);
            return raw.Length == 0 ? null : FlexibleDateParser.Parse(raw);
        }

        // Times come as Excel time values (a fraction of a day), DateTimes, or text like "6:00".
        private static string? TimeText(IXLWorksheet ws, int row, SheetHeader h, string key)
        {
            if (!h.Columns.TryGetValue(key, out var col)) return null;
            var cell = ws.Cell(row, col);
            if (cell.DataType == XLDataType.DateTime) return cell.GetDateTime().ToString("HH:mm");
            if (cell.DataType == XLDataType.TimeSpan) return cell.GetTimeSpan().ToString(@"hh\:mm");
            if (cell.DataType == XLDataType.Number)
            {
                var fraction = cell.GetDouble() % 1;
                return TimeSpan.FromDays(fraction).ToString(@"hh\:mm");
            }
            var text = Text(ws, row, h, key);
            return text.Length == 0 ? null : text;
        }

        private static TimeSpan? ParseTime(string? text)
        {
            if (string.IsNullOrWhiteSpace(text)) return null;
            var t = text.Trim();
            if (TimeSpan.TryParse(t, CultureInfo.InvariantCulture, out var ts) && ts >= TimeSpan.Zero && ts < TimeSpan.FromDays(1)) return ts;
            if (DateTime.TryParse(t, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt)) return dt.TimeOfDay;
            return null;
        }

        private static bool IsTotalRow(params string[] values) =>
            values.Select(Normalize).Any(v => v.Contains("اجمال") || v == "total" || v.StartsWith("grand total"));

        private static bool TryParseYearSheetName(string sheetName, out int year)
        {
            var trimmed = sheetName.Trim();
            if (trimmed.Length == 4 && int.TryParse(trimmed, NumberStyles.None, CultureInfo.InvariantCulture, out year)
                && year is >= 1990 and <= 2100)
                return true;
            year = 0;
            return false;
        }

        private static XLWorkbook? OpenWorkbook(Stream stream, ExcelImportResultDto result)
        {
            try
            {
                return new XLWorkbook(stream);
            }
            catch
            {
                result.Success = false;
                result.Message = "تعذّرت قراءة الملف المرفوع كملف إكسل. تأكد من أنه ملف .xlsx أو .xls صالح وغير تالف.";
                return null;
            }
        }

        private static string HashOf(Stream stream)
        {
            if (!stream.CanSeek) return string.Empty;
            var position = stream.Position;
            stream.Position = 0;
            var hash = Convert.ToHexString(SHA256.HashData(stream));
            stream.Position = position;
            return hash;
        }

        // Every field error of a row becomes one row error, under the header the sheet really uses.
        private static void AddRowErrors(ExcelImportResultDto result, int row, List<FieldErrorDto> errors,
            SheetHeader header, string? sheetPrefix = null)
        {
            foreach (var e in errors)
            {
                header.Labels.TryGetValue(e.Field, out var label);
                result.Errors.Add(new ExcelRowErrorDto
                {
                    RowNumber = row,
                    Column = sheetPrefix == null ? label : $"{sheetPrefix} - {label}",
                    Message = e.Message
                });
            }
        }

        #endregion

        #region Upload 1 - customer roster (sheet name = fiscal year)

        private static readonly string[] CustomerFooterMarkers = { "grand total", "total customer turnover", "customer discount" };

        public async Task<ExcelImportResultDto> BulkUploadCustomersAsync(Stream excelStream, string fileName, string uploadedByUserId)
        {
            var result = new ExcelImportResultDto();
            var hash = HashOf(excelStream);
            var def = DepartmentTemplates.SalesCustomers;

            var workbook = OpenWorkbook(excelStream, result);
            if (workbook == null) return result;

            var records = new List<SalesCustomerRecord>();
            var years = new List<int>();

            using (workbook)
            {
                var yearSheets = workbook.Worksheets
                    .Select(ws => (Sheet: ws, Ok: TryParseYearSheetName(ws.Name, out var y), Year: y))
                    .Where(x => x.Ok)
                    .OrderBy(x => x.Year)
                    .ToList();

                if (yearSheets.Count == 0)
                {
                    result.Message = "لم يتم العثور على ورقة باسم السنة المالية. اسم ورقة العملاء يجب أن يكون السنة المالية، مثل \"2026\" (ورقة لكل سنة).";
                    await LogBatchAsync(SalesFileType.CustomerRoster, fileName, uploadedByUserId, hash, result, null, null);
                    return result;
                }

                bool multipleSheets = yearSheets.Count > 1;
                foreach (var (ws, _, year) in yearSheets)
                {
                    var prefix = multipleSheets ? $"ورقة {ws.Name.Trim()}" : null;
                    var header = FindHeader(ws, def);
                    if (header == null)
                    {
                        result.Errors.Add(new ExcelRowErrorDto
                        {
                            RowNumber = 1,
                            Column = prefix,
                            Message = $"لم يتم العثور على صف العناوين في ورقة {ws.Name.Trim()} (المطلوب: {RequiredHeadersText(def)}). تم تجاهل الورقة."
                        });
                        continue;
                    }

                    years.Add(year);
                    var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    var lastRow = ws.LastRowUsed()?.RowNumber() ?? header.Row;

                    for (int r = header.Row + 1; r <= lastRow; r++)
                    {
                        var code = Text(ws, r, header, DepartmentTemplates.SalesCustomerCode);
                        var name = Text(ws, r, header, DepartmentTemplates.SalesCustomerName);
                        if (code.Length == 0 && name.Length == 0) continue;
                        if (CustomerFooterMarkers.Any(m => code.Equals(m, StringComparison.OrdinalIgnoreCase))) continue;
                        // AX repeats the company banner mid-sheet with every other column blank.
                        if (name.Length == 0 && code.Length > 50) continue;

                        result.DataRows++;
                        var dto = new SalesCustomerDto
                        {
                            CustomerCode = code,
                            CustomerName = name,
                            CustomerGroup = Text(ws, r, header, DepartmentTemplates.SalesCustomerGroup),
                            Currency = Text(ws, r, header, DepartmentTemplates.SalesCurrency),
                            FiscalYear = year
                        };

                        var errors = ValidateCustomer(dto);
                        if (errors.Count == 0 && !seen.Add(code))
                            errors.Add("customerCode", $"رقم الحساب {code} مكرر في نفس السنة، تم الاحتفاظ بأول صف.");
                        if (errors.Count > 0)
                        {
                            result.SkippedRows++;
                            AddRowErrors(result, r, errors, header, prefix);
                            continue;
                        }

                        var entity = new SalesCustomerRecord();
                        Apply(dto, entity);
                        records.Add(entity);
                    }
                }
            }

            result.TotalRows = result.DataRows;
            if (records.Count == 0)
            {
                result.Message = result.Errors.Count > 0
                    ? "لم يتم حفظ أي عميل. راجع أسباب الرفض أدناه."
                    : "الملف لا يحتوي على أي صف عميل.";
                await LogBatchAsync(SalesFileType.CustomerRoster, fileName, uploadedByUserId, hash, result, null, null);
                return result;
            }

            var distinctYears = years.Distinct().OrderBy(y => y).ToList();
            await ReplaceAsync(SalesFileType.CustomerRoster, fileName, uploadedByUserId, hash, result,
                string.Join(",", distinctYears), string.Join(",", distinctYears),
                async () =>
                {
                    var old = await _context.SalesCustomerRecords.Where(c => distinctYears.Contains(c.FiscalYear)).ToListAsync();
                    _context.SalesCustomerRecords.RemoveRange(old);
                    return old.Count;
                },
                records,
                (e, batchId) => e.SalesImportBatchId = batchId,
                list => _context.SalesCustomerRecords.AddRange(list));

            result.Message = $"تم رفع قائمة العملاء لسنة {string.Join(" و ", distinctYears)}: {records.Count:N0} عميل. "
                             + "بيانات نفس السنة السابقة تم استبدالها بهذا الملف.";
            return result;
        }

        #endregion

        #region Upload 2 - fleet capacity (a snapshot)

        public async Task<ExcelImportResultDto> BulkUploadFleetCapacityAsync(Stream excelStream, string fileName, string uploadedByUserId)
        {
            var result = new ExcelImportResultDto();
            var hash = HashOf(excelStream);
            var def = DepartmentTemplates.SalesFleetCapacity;

            var workbook = OpenWorkbook(excelStream, result);
            if (workbook == null) return result;

            var rows = new List<SalesFleetCapacity>();
            string sheetName;

            using (workbook)
            {
                var found = workbook.Worksheets
                    .Select(ws => (Sheet: ws, Header: FindHeader(ws, def)))
                    .FirstOrDefault(x => x.Header != null);

                if (found.Header == null)
                {
                    result.Message = $"لم يتم العثور على ورقة الطاقة الاستيعابية. يجب أن يحتوي صف العناوين على: {RequiredHeadersText(def)}.";
                    await LogBatchAsync(SalesFileType.FleetCapacity, fileName, uploadedByUserId, hash, result, null, null);
                    return result;
                }

                var (ws, header) = (found.Sheet, found.Header);
                sheetName = ws.Name.Trim();
                var lastRow = ws.LastRowUsed()?.RowNumber() ?? header.Row;

                for (int r = header.Row + 1; r <= lastRow; r++)
                {
                    var busType = Text(ws, r, header, DepartmentTemplates.SalesBusType);
                    var category = Text(ws, r, header, DepartmentTemplates.SalesCategory);
                    var busCode = Text(ws, r, header, DepartmentTemplates.SalesBusCode);
                    var busesText = Text(ws, r, header, DepartmentTemplates.SalesNumberOfBuses);
                    if (busType.Length == 0 && category.Length == 0 && busCode.Length == 0 && busesText.Length == 0) continue;
                    if (IsTotalRow(busType, category, busCode)) continue;

                    result.DataRows++;
                    bool badBuses = false, badSeats = false, badTotal = false, badModel = false;
                    var dto = new SalesFleetCapacityDto
                    {
                        BusCode = busCode,
                        BusType = busType,
                        Category = category,
                        ModelYear = Int(ws, r, header, DepartmentTemplates.SalesModelYear, ref badModel),
                        NumberOfBuses = Int(ws, r, header, DepartmentTemplates.SalesNumberOfBuses, ref badBuses),
                        SeatsPerBus = Int(ws, r, header, DepartmentTemplates.SalesSeatsPerBus, ref badSeats),
                        TotalSeats = Int(ws, r, header, DepartmentTemplates.SalesTotalSeats, ref badTotal)
                    };

                    var errors = ValidateFleet(dto);
                    if (badBuses) ReplaceError(errors, "numberOfBuses", "عدد الحافلات يجب أن يكون رقماً صحيحاً.");
                    if (badSeats) ReplaceError(errors, "seatsPerBus", "عدد المقاعد يجب أن يكون رقماً صحيحاً.");
                    if (badTotal) ReplaceError(errors, "totalSeats", "إجمالي المقاعد يجب أن يكون رقماً صحيحاً.");
                    if (badModel) ReplaceError(errors, "modelYear", "سنة الموديل يجب أن تكون رقماً مثل 2026.");
                    if (errors.Count > 0)
                    {
                        result.SkippedRows++;
                        AddRowErrors(result, r, errors, header);
                        continue;
                    }

                    var entity = new SalesFleetCapacity { SnapshotDate = DateTime.UtcNow };
                    Apply(dto, entity);
                    rows.Add(entity);
                }
            }

            result.TotalRows = result.DataRows;
            if (rows.Count == 0)
            {
                result.Message = result.Errors.Count > 0
                    ? "لم يتم حفظ أي صف. راجع أسباب الرفض أدناه."
                    : "الملف لا يحتوي على أي صف حافلات.";
                await LogBatchAsync(SalesFileType.FleetCapacity, fileName, uploadedByUserId, hash, result, null, null);
                return result;
            }

            await ReplaceAsync(SalesFileType.FleetCapacity, fileName, uploadedByUserId, hash, result,
                sheetName, DateTime.UtcNow.ToString("yyyy-MM-dd"),
                async () =>
                {
                    var old = await _context.SalesFleetCapacities.ToListAsync();
                    _context.SalesFleetCapacities.RemoveRange(old);
                    return old.Count;
                },
                rows,
                (e, batchId) => e.SalesImportBatchId = batchId,
                list => _context.SalesFleetCapacities.AddRange(list));

            result.Message = $"تم تحديث الطاقة الاستيعابية: {rows.Sum(r => r.NumberOfBuses):N0} حافلة و {rows.Sum(r => r.TotalSeats):N0} مقعد. "
                             + "الملف يستبدل بيانات الأسطول السابقة بالكامل.";
            return result;
        }

        #endregion

        #region Upload 3 - daily operations (replaces the dates it covers)

        public async Task<ExcelImportResultDto> BulkUploadDailyOperationsAsync(Stream excelStream, string fileName, string uploadedByUserId)
        {
            var result = new ExcelImportResultDto();
            var hash = HashOf(excelStream);
            var def = DepartmentTemplates.SalesDailyOperations;

            var workbook = OpenWorkbook(excelStream, result);
            if (workbook == null) return result;

            var rows = new List<SalesDailyOperation>();
            string sheetName;

            using (workbook)
            {
                var found = workbook.Worksheets
                    .Select(ws => (Sheet: ws, Header: FindHeader(ws, def)))
                    .FirstOrDefault(x => x.Header != null);

                if (found.Header == null)
                {
                    result.Message = $"لم يتم العثور على ورقة التشغيل اليومي. يجب أن يحتوي صف العناوين على: {RequiredHeadersText(def)}.";
                    await LogBatchAsync(SalesFileType.DailyOperations, fileName, uploadedByUserId, hash, result, null, null);
                    return result;
                }

                var (ws, header) = (found.Sheet, found.Header);
                sheetName = ws.Name.Trim();
                var lastRow = ws.LastRowUsed()?.RowNumber() ?? header.Row;

                for (int r = header.Row + 1; r <= lastRow; r++)
                {
                    var customer = Text(ws, r, header, DepartmentTemplates.SalesOpsCustomerName);
                    var busType = Text(ws, r, header, DepartmentTemplates.SalesBusTypeCode);
                    var order = Text(ws, r, header, DepartmentTemplates.SalesRentalOrderNumber);
                    var countText = Text(ws, r, header, DepartmentTemplates.SalesOperationalCount);
                    if (customer.Length == 0 && busType.Length == 0 && order.Length == 0 && countText.Length == 0) continue;
                    if (IsTotalRow(customer, busType, order)) continue;

                    result.DataRows++;
                    bool badCount = false, badScheduled = false;
                    var dto = new SalesDailyOperationDto
                    {
                        RentalOrderNumber = order,
                        ConfirmationNumber = Text(ws, r, header, DepartmentTemplates.SalesConfirmationNumber),
                        RequestType = Text(ws, r, header, DepartmentTemplates.SalesRequestType),
                        CustomerName = customer,
                        ExecutionPoint = Text(ws, r, header, DepartmentTemplates.SalesExecutionPoint),
                        Direction = Text(ws, r, header, DepartmentTemplates.SalesDirection),
                        BusTypeCode = busType,
                        OperationalCount = Int(ws, r, header, DepartmentTemplates.SalesOperationalCount, ref badCount),
                        ScheduledBuses = Int(ws, r, header, DepartmentTemplates.SalesScheduledBuses, ref badScheduled),
                        ExecutionDate = Date(ws, r, header, DepartmentTemplates.SalesExecutionDate),
                        ExecutionTime = TimeText(ws, r, header, DepartmentTemplates.SalesExecutionTime),
                        Notes = Text(ws, r, header, DepartmentTemplates.SalesNotes)
                    };

                    var errors = ValidateOperation(dto);
                    if (badCount) ReplaceError(errors, "operationalCount", "عدد الحافلات المطلوبة يجب أن يكون رقماً صحيحاً.");
                    if (badScheduled) ReplaceError(errors, "scheduledBuses", "الحافلات المجدولة يجب أن تكون رقماً صحيحاً (أو تُترك فارغة).");
                    if (errors.Count > 0)
                    {
                        result.SkippedRows++;
                        AddRowErrors(result, r, errors, header);
                        continue;
                    }

                    var entity = new SalesDailyOperation();
                    Apply(dto, entity);
                    rows.Add(entity);
                }
            }

            result.TotalRows = result.DataRows;
            if (rows.Count == 0)
            {
                result.Message = result.Errors.Count > 0
                    ? "لم يتم حفظ أي صف. راجع أسباب الرفض أدناه."
                    : "الملف لا يحتوي على أي صف تشغيل.";
                await LogBatchAsync(SalesFileType.DailyOperations, fileName, uploadedByUserId, hash, result, null, null);
                return result;
            }

            var dates = rows.Select(o => o.ExecutionDate).Distinct().OrderBy(d => d).ToList();
            var period = dates.Count == 1
                ? dates[0].ToString("yyyy-MM-dd")
                : $"{dates[0]:yyyy-MM-dd} → {dates[^1]:yyyy-MM-dd}";

            await ReplaceAsync(SalesFileType.DailyOperations, fileName, uploadedByUserId, hash, result,
                sheetName, period,
                async () =>
                {
                    var old = await _context.SalesDailyOperations.Where(o => dates.Contains(o.ExecutionDate)).ToListAsync();
                    _context.SalesDailyOperations.RemoveRange(old);
                    return old.Count;
                },
                rows,
                (e, batchId) => e.SalesImportBatchId = batchId,
                list => _context.SalesDailyOperations.AddRange(list));

            result.Message = $"تم رفع {rows.Count:N0} أمر تشغيل لعدد {dates.Count} يوم ({period}). "
                             + "بيانات نفس الأيام السابقة تم استبدالها بهذا الملف.";
            return result;
        }

        #endregion

        #region Combined workbook

        public async Task<ExcelImportResultDto> BulkUploadCombinedAsync(byte[] xlsxBytes, string fileName, string uploadedByUserId)
        {
            var sections = new List<(string Name, ExcelImportResultDto Result)>();
            using (var s = new MemoryStream(xlsxBytes)) sections.Add(("العملاء", await BulkUploadCustomersAsync(s, fileName, uploadedByUserId)));
            using (var s = new MemoryStream(xlsxBytes)) sections.Add(("الطاقة الاستيعابية", await BulkUploadFleetCapacityAsync(s, fileName, uploadedByUserId)));
            using (var s = new MemoryStream(xlsxBytes)) sections.Add(("التشغيل اليومي", await BulkUploadDailyOperationsAsync(s, fileName, uploadedByUserId)));

            return new ExcelImportResultDto
            {
                Success = sections.Any(x => x.Result.Success),
                Message = string.Join(" | ", sections.Select(x => $"{x.Name}: {x.Result.Message}")),
                TotalRows = sections.Sum(x => x.Result.TotalRows),
                DataRows = sections.Sum(x => x.Result.DataRows),
                InsertedRows = sections.Sum(x => x.Result.InsertedRows),
                UpdatedRows = sections.Sum(x => x.Result.UpdatedRows),
                SkippedRows = sections.Sum(x => x.Result.SkippedRows),
                Errors = sections.SelectMany(x => x.Result.Errors.Select(e => new ExcelRowErrorDto
                {
                    RowNumber = e.RowNumber,
                    Column = string.IsNullOrEmpty(e.Column) ? x.Name : $"{x.Name} - {e.Column}",
                    Message = e.Message
                })).ToList()
            };
        }

        #endregion

        #region Saving an upload

        // Deletes what the file replaces, records the batch and inserts the new rows - all or nothing.
        private async System.Threading.Tasks.Task ReplaceAsync<T>(
            SalesFileType type, string fileName, string user, string hash, ExcelImportResultDto result,
            string? sheets, string? period,
            Func<Task<int>> removeOld,
            List<T> rows,
            Action<T, int> setBatch,
            Action<IEnumerable<T>> addRange)
        {
            using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
                var replaced = await removeOld();
                await _context.SaveChangesAsync();

                result.Success = true;
                result.InsertedRows = rows.Count;
                var batch = NewBatch(type, fileName, user, hash, result, sheets, period);
                _context.SalesImportBatches.Add(batch);
                await _context.SaveChangesAsync();

                foreach (var row in rows) setBatch(row, batch.Id);
                // Small chunks: one huge multi-row insert under this connection's MARS setting
                // spuriously reports an FK violation against the batch row inserted just above.
                const int chunk = 200;
                for (int i = 0; i < rows.Count; i += chunk)
                {
                    addRange(rows.Skip(i).Take(chunk));
                    await _context.SaveChangesAsync();
                }

                await tx.CommitAsync();
                _logger.LogInformation("Sales {Type} upload {File}: {Inserted} rows in, {Replaced} replaced, {Skipped} skipped.",
                    type, fileName, rows.Count, replaced, result.SkippedRows);
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                _context.ChangeTracker.Clear();
                _logger.LogError(ex, "Sales {Type} upload failed for {File}.", type, fileName);
                result.Success = false;
                result.InsertedRows = 0;
                result.Message = "حدث خطأ أثناء حفظ البيانات، ولم يتم تغيير أي شيء. يرجى إعادة المحاولة.";
                await LogBatchAsync(type, fileName, user, hash, result, sheets, period);
            }
        }

        private static SalesImportBatch NewBatch(SalesFileType type, string fileName, string user, string hash,
            ExcelImportResultDto result, string? sheets, string? period) => new()
        {
            FileName = Truncate(fileName, 255),
            FileType = type,
            UploadedByUserId = Truncate(user, 64),
            UploadedAt = DateTime.UtcNow,
            SourceSheet = sheets == null ? null : Truncate(sheets, 300),
            ReportingPeriod = period == null ? null : Truncate(period, 100),
            ImportedRowCount = result.Success ? result.InsertedRows : 0,
            RejectedRowCount = result.SkippedRows,
            Status = !result.Success ? SalesImportStatus.Failed
                : result.SkippedRows > 0 ? SalesImportStatus.PartialSuccess : SalesImportStatus.Success,
            ErrorDetails = result.Errors.Count == 0 && result.Success ? null
                : Truncate(string.Join(" | ", new[] { result.Success ? null : result.Message }
                    .Concat(result.Errors.Select(e => $"صف {e.RowNumber}: {e.Message}"))
                    .Where(x => !string.IsNullOrEmpty(x))), 4000),
            FileHash = Truncate(hash, 64)
        };

        // A failed upload is still recorded in the import history.
        private async System.Threading.Tasks.Task LogBatchAsync(SalesFileType type, string fileName, string user, string hash,
            ExcelImportResultDto result, string? sheets, string? period)
        {
            try
            {
                _context.SalesImportBatches.Add(NewBatch(type, fileName, user, hash, result, sheets, period));
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _context.ChangeTracker.Clear();
                _logger.LogError(ex, "Could not record the failed Sales {Type} upload.", type);
            }
        }

        private static string Truncate(string value, int max) => value.Length <= max ? value : value.Substring(0, max);

        private async Task<int> ManualBatchIdAsync(SalesFileType type)
        {
            var batch = await _context.SalesImportBatches
                .FirstOrDefaultAsync(b => b.FileType == type && b.FileHash == ManualEntryHash);
            if (batch == null)
            {
                batch = new SalesImportBatch
                {
                    FileName = "إدخال يدوي من الصفحة",
                    FileType = type,
                    UploadedByUserId = "manual",
                    UploadedAt = DateTime.UtcNow,
                    Status = SalesImportStatus.Success,
                    FileHash = ManualEntryHash
                };
                _context.SalesImportBatches.Add(batch);
                await _context.SaveChangesAsync();
            }
            return batch.Id;
        }

        public async Task<PagedResultDto<SalesImportBatchDto>> GetImportHistoryAsync(int page, int pageSize)
        {
            (page, pageSize) = Paging(page, pageSize);
            var query = _context.SalesImportBatches.AsNoTracking()
                .Where(b => b.FileHash != ManualEntryHash)
                .OrderByDescending(b => b.UploadedAt);
            var total = await query.CountAsync();
            var items = await query.Skip((page - 1) * pageSize).Take(pageSize)
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
            return new PagedResultDto<SalesImportBatchDto> { Items = items, TotalCount = total, Page = page, PageSize = pageSize };
        }

        private static (int Page, int PageSize) Paging(int page, int pageSize) =>
            (Math.Max(page, 1), pageSize < 1 ? 20 : Math.Min(pageSize, 200));

        #endregion

        #region Customers - index + CRUD

        public async Task<PagedResultDto<SalesCustomerDto>> GetCustomersPagedAsync(int page, int pageSize, string? search, int? fiscalYear, string? customerGroup)
        {
            (page, pageSize) = Paging(page, pageSize);
            var q = _context.SalesCustomerRecords.AsNoTracking().AsQueryable();
            if (fiscalYear.HasValue) q = q.Where(c => c.FiscalYear == fiscalYear.Value);
            if (!string.IsNullOrWhiteSpace(customerGroup)) q = q.Where(c => c.CustomerGroup == customerGroup);
            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim();
                q = q.Where(c => c.CustomerCode.Contains(s) || c.CustomerName.Contains(s) || (c.CustomerGroup != null && c.CustomerGroup.Contains(s)));
            }

            var total = await q.CountAsync();
            var items = await q.OrderByDescending(c => c.FiscalYear).ThenBy(c => c.CustomerCode)
                .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
            return new PagedResultDto<SalesCustomerDto> { Items = items.Select(ToDto).ToList(), TotalCount = total, Page = page, PageSize = pageSize };
        }

        public async Task<SalesCustomerFilterOptionsDto> GetCustomerFilterOptionsAsync() => new()
        {
            Years = await _context.SalesCustomerRecords.AsNoTracking().Select(c => c.FiscalYear).Distinct().OrderByDescending(y => y).ToListAsync(),
            Groups = await _context.SalesCustomerRecords.AsNoTracking().Where(c => c.CustomerGroup != null && c.CustomerGroup != "")
                .Select(c => c.CustomerGroup!).Distinct().OrderBy(g => g).ToListAsync()
        };

        public async Task<List<SalesCustomerDto>> GetLatestYearCustomersAsync()
        {
            var latest = await _context.SalesCustomerRecords.AsNoTracking().MaxAsync(c => (int?)c.FiscalYear);
            if (latest == null) return new List<SalesCustomerDto>();
            var rows = await _context.SalesCustomerRecords.AsNoTracking().Where(c => c.FiscalYear == latest)
                .OrderBy(c => c.CustomerCode).ToListAsync();
            return rows.Select(ToDto).ToList();
        }

        public async Task<SalesCustomerDto?> GetCustomerAsync(int id)
        {
            var e = await _context.SalesCustomerRecords.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
            return e == null ? null : ToDto(e);
        }

        private async Task<List<FieldErrorDto>> ValidateCustomerAsync(SalesCustomerDto dto, int? excludeId)
        {
            var errors = ValidateCustomer(dto);
            if (errors.Count == 0)
            {
                var code = dto.CustomerCode!.Trim();
                var exists = await _context.SalesCustomerRecords.AnyAsync(c =>
                    c.FiscalYear == dto.FiscalYear && c.CustomerCode == code && (excludeId == null || c.Id != excludeId));
                if (exists) errors.Add("customerCode", $"رقم الحساب {code} موجود مسبقاً في سنة {dto.FiscalYear}.");
            }
            return errors;
        }

        public async Task<CrudResult<SalesCustomerDto>> CreateCustomerAsync(SalesCustomerDto dto)
        {
            var errors = await ValidateCustomerAsync(dto, null);
            if (errors.Count > 0) return CrudResult<SalesCustomerDto>.Invalid(errors);
            var e = new SalesCustomerRecord { SalesImportBatchId = await ManualBatchIdAsync(SalesFileType.CustomerRoster) };
            Apply(dto, e);
            _context.SalesCustomerRecords.Add(e);
            await _context.SaveChangesAsync();
            return CrudResult<SalesCustomerDto>.Ok(ToDto(e));
        }

        public async Task<CrudResult<SalesCustomerDto>> UpdateCustomerAsync(int id, SalesCustomerDto dto)
        {
            var e = await _context.SalesCustomerRecords.FindAsync(id);
            if (e == null) return CrudResult<SalesCustomerDto>.Missing();
            var errors = await ValidateCustomerAsync(dto, id);
            if (errors.Count > 0) return CrudResult<SalesCustomerDto>.Invalid(errors);
            Apply(dto, e);
            await _context.SaveChangesAsync();
            return CrudResult<SalesCustomerDto>.Ok(ToDto(e));
        }

        public async Task<bool> DeleteCustomerAsync(int id)
        {
            var e = await _context.SalesCustomerRecords.FindAsync(id);
            if (e == null) return false;
            _context.SalesCustomerRecords.Remove(e);
            await _context.SaveChangesAsync();
            return true;
        }

        #endregion

        #region Fleet capacity - index + CRUD

        public async Task<PagedResultDto<SalesFleetCapacityDto>> GetFleetPagedAsync(int page, int pageSize, string? search, string? category, string? busType)
        {
            (page, pageSize) = Paging(page, pageSize);
            var q = _context.SalesFleetCapacities.AsNoTracking().AsQueryable();
            if (!string.IsNullOrWhiteSpace(category)) q = q.Where(f => f.Category == category);
            if (!string.IsNullOrWhiteSpace(busType)) q = q.Where(f => f.BusType == busType);
            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim();
                q = q.Where(f => f.BusType.Contains(s) || f.Category.Contains(s) || (f.BusCode != null && f.BusCode.Contains(s)));
            }

            var total = await q.CountAsync();
            var items = await q.OrderByDescending(f => f.NumberOfBuses).ThenBy(f => f.BusType)
                .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
            return new PagedResultDto<SalesFleetCapacityDto> { Items = items.Select(ToDto).ToList(), TotalCount = total, Page = page, PageSize = pageSize };
        }

        public async Task<SalesFleetFilterOptionsDto> GetFleetFilterOptionsAsync() => new()
        {
            Categories = await _context.SalesFleetCapacities.AsNoTracking().Select(f => f.Category).Distinct().OrderBy(x => x).ToListAsync(),
            BusTypes = await _context.SalesFleetCapacities.AsNoTracking().Select(f => f.BusType).Distinct().OrderBy(x => x).ToListAsync()
        };

        public async Task<List<SalesFleetCapacityDto>> GetAllFleetAsync() =>
            (await _context.SalesFleetCapacities.AsNoTracking().OrderBy(f => f.Category).ThenBy(f => f.BusType).ToListAsync())
            .Select(ToDto).ToList();

        public async Task<SalesFleetCapacityDto?> GetFleetItemAsync(int id)
        {
            var e = await _context.SalesFleetCapacities.AsNoTracking().FirstOrDefaultAsync(f => f.Id == id);
            return e == null ? null : ToDto(e);
        }

        public async Task<CrudResult<SalesFleetCapacityDto>> CreateFleetItemAsync(SalesFleetCapacityDto dto)
        {
            var errors = ValidateFleet(dto);
            if (errors.Count > 0) return CrudResult<SalesFleetCapacityDto>.Invalid(errors);
            var e = new SalesFleetCapacity
            {
                SalesImportBatchId = await ManualBatchIdAsync(SalesFileType.FleetCapacity),
                SnapshotDate = DateTime.UtcNow
            };
            Apply(dto, e);
            _context.SalesFleetCapacities.Add(e);
            await _context.SaveChangesAsync();
            return CrudResult<SalesFleetCapacityDto>.Ok(ToDto(e));
        }

        public async Task<CrudResult<SalesFleetCapacityDto>> UpdateFleetItemAsync(int id, SalesFleetCapacityDto dto)
        {
            var e = await _context.SalesFleetCapacities.FindAsync(id);
            if (e == null) return CrudResult<SalesFleetCapacityDto>.Missing();
            var errors = ValidateFleet(dto);
            if (errors.Count > 0) return CrudResult<SalesFleetCapacityDto>.Invalid(errors);
            Apply(dto, e);
            await _context.SaveChangesAsync();
            return CrudResult<SalesFleetCapacityDto>.Ok(ToDto(e));
        }

        public async Task<bool> DeleteFleetItemAsync(int id)
        {
            var e = await _context.SalesFleetCapacities.FindAsync(id);
            if (e == null) return false;
            _context.SalesFleetCapacities.Remove(e);
            await _context.SaveChangesAsync();
            return true;
        }

        #endregion

        #region Daily operations - index + CRUD

        public async Task<PagedResultDto<SalesDailyOperationDto>> GetDailyOperationsPagedAsync(int page, int pageSize, string? search,
            DateTime? fromDate, DateTime? toDate, string? requestType, string? executionPoint)
        {
            (page, pageSize) = Paging(page, pageSize);
            var q = _context.SalesDailyOperations.AsNoTracking().AsQueryable();
            if (fromDate.HasValue) q = q.Where(o => o.ExecutionDate >= fromDate.Value.Date);
            if (toDate.HasValue) q = q.Where(o => o.ExecutionDate < toDate.Value.Date.AddDays(1));
            if (!string.IsNullOrWhiteSpace(requestType)) q = q.Where(o => o.RequestType == requestType);
            if (!string.IsNullOrWhiteSpace(executionPoint)) q = q.Where(o => o.ExecutionPoint == executionPoint);
            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim();
                q = q.Where(o => o.CustomerName.Contains(s)
                                 || (o.RentalOrderNumber != null && o.RentalOrderNumber.Contains(s))
                                 || (o.ConfirmationNumber != null && o.ConfirmationNumber.Contains(s))
                                 || (o.Direction != null && o.Direction.Contains(s)));
            }

            var total = await q.CountAsync();
            var items = await q.OrderByDescending(o => o.ExecutionDate).ThenBy(o => o.ExecutionTime).ThenBy(o => o.CustomerName)
                .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
            return new PagedResultDto<SalesDailyOperationDto> { Items = items.Select(ToDto).ToList(), TotalCount = total, Page = page, PageSize = pageSize };
        }

        public async Task<SalesOperationFilterOptionsDto> GetOperationFilterOptionsAsync() => new()
        {
            RequestTypes = await _context.SalesDailyOperations.AsNoTracking().Where(o => o.RequestType != null && o.RequestType != "")
                .Select(o => o.RequestType!).Distinct().OrderBy(x => x).ToListAsync(),
            ExecutionPoints = await _context.SalesDailyOperations.AsNoTracking().Where(o => o.ExecutionPoint != null && o.ExecutionPoint != "")
                .Select(o => o.ExecutionPoint!).Distinct().OrderBy(x => x).ToListAsync()
        };

        public async Task<SalesDailyOperationDto?> GetDailyOperationAsync(int id)
        {
            var e = await _context.SalesDailyOperations.AsNoTracking().FirstOrDefaultAsync(o => o.Id == id);
            return e == null ? null : ToDto(e);
        }

        public async Task<CrudResult<SalesDailyOperationDto>> CreateDailyOperationAsync(SalesDailyOperationDto dto)
        {
            var errors = ValidateOperation(dto);
            if (errors.Count > 0) return CrudResult<SalesDailyOperationDto>.Invalid(errors);
            var e = new SalesDailyOperation { SalesImportBatchId = await ManualBatchIdAsync(SalesFileType.DailyOperations) };
            Apply(dto, e);
            _context.SalesDailyOperations.Add(e);
            await _context.SaveChangesAsync();
            return CrudResult<SalesDailyOperationDto>.Ok(ToDto(e));
        }

        public async Task<CrudResult<SalesDailyOperationDto>> UpdateDailyOperationAsync(int id, SalesDailyOperationDto dto)
        {
            var e = await _context.SalesDailyOperations.FindAsync(id);
            if (e == null) return CrudResult<SalesDailyOperationDto>.Missing();
            var errors = ValidateOperation(dto);
            if (errors.Count > 0) return CrudResult<SalesDailyOperationDto>.Invalid(errors);
            Apply(dto, e);
            await _context.SaveChangesAsync();
            return CrudResult<SalesDailyOperationDto>.Ok(ToDto(e));
        }

        public async Task<bool> DeleteDailyOperationAsync(int id)
        {
            var e = await _context.SalesDailyOperations.FindAsync(id);
            if (e == null) return false;
            _context.SalesDailyOperations.Remove(e);
            await _context.SaveChangesAsync();
            return true;
        }

        #endregion

        #region KPIs for the executive dashboard

        public async Task<SalesKpisDto> GetSalesKpisAsync()
        {
            var dto = new SalesKpisDto();

            // Customers: the latest fiscal year on file, compared with the year uploaded before it.
            var years = await _context.SalesCustomerRecords.AsNoTracking()
                .Select(c => c.FiscalYear).Distinct().OrderByDescending(y => y).Take(2).ToListAsync();
            if (years.Count > 0)
            {
                dto.HasCustomerData = true;
                dto.LatestFiscalYear = years[0];
                var latest = await _context.SalesCustomerRecords.AsNoTracking()
                    .Where(c => c.FiscalYear == years[0])
                    .Select(c => new { c.CustomerCode, c.CustomerGroup }).ToListAsync();
                var codes = latest.Select(c => c.CustomerCode).ToHashSet(StringComparer.OrdinalIgnoreCase);
                dto.ActiveCustomers = codes.Count;

                if (years.Count > 1)
                {
                    var previous = (await _context.SalesCustomerRecords.AsNoTracking()
                            .Where(c => c.FiscalYear == years[1]).Select(c => c.CustomerCode).ToListAsync())
                        .ToHashSet(StringComparer.OrdinalIgnoreCase);
                    dto.PreviousYearCustomers = previous.Count;
                    dto.NewCustomers = codes.Count(c => !previous.Contains(c));
                }

                var top = latest
                    .GroupBy(c => string.IsNullOrWhiteSpace(c.CustomerGroup) ? "غير مصنف" : c.CustomerGroup!)
                    .OrderByDescending(g => g.Count())
                    .FirstOrDefault();
                if (top != null && latest.Count > 0)
                {
                    dto.TopCustomerGroup = top.Key;
                    dto.TopCustomerGroupSharePercent = Math.Round(top.Count() * 100.0 / latest.Count, 1);
                }
            }

            // Fleet: the current snapshot.
            var fleet = await _context.SalesFleetCapacities.AsNoTracking()
                .Select(f => new { f.NumberOfBuses, f.TotalSeats }).ToListAsync();
            if (fleet.Count > 0)
            {
                dto.HasFleetData = true;
                dto.TotalFleetBuses = fleet.Sum(f => f.NumberOfBuses);
                dto.TotalFleetSeats = fleet.Sum(f => f.TotalSeats);
            }

            // Daily operations: the latest execution date uploaded.
            var latestDate = await _context.SalesDailyOperations.AsNoTracking().MaxAsync(o => (DateTime?)o.ExecutionDate);
            if (latestDate != null)
            {
                dto.HasDailyOperationsData = true;
                dto.LatestOperationsDate = latestDate;
                var day = await _context.SalesDailyOperations.AsNoTracking()
                    .Where(o => o.ExecutionDate == latestDate.Value)
                    .Select(o => new { o.OperationalCount, o.ScheduledBuses }).ToListAsync();
                dto.LatestOrdersCount = day.Count;
                dto.LatestRequestedBuses = day.Sum(o => o.OperationalCount);
                dto.LatestScheduledBuses = day.Sum(o => o.ScheduledBuses);
                if (dto.LatestRequestedBuses > 0)
                    dto.SchedulingCoveragePercent = Math.Round(dto.LatestScheduledBuses * 100.0 / dto.LatestRequestedBuses, 1);
                if (dto.TotalFleetBuses > 0)
                    dto.FleetUtilizationPercent = Math.Round(dto.LatestScheduledBuses * 100.0 / dto.TotalFleetBuses, 1);
            }

            return dto;
        }

        #endregion
    }
}
