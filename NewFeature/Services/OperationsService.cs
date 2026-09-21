using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NewFeature.Services.ExcelImport;
using NewFeature.Models;
using NewFeature.Services.Repositories;

namespace NewFeature.Services
{
    public class OperationsService : IOperationsService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<OperationsService> _logger;

        public OperationsService(ApplicationDbContext context, ILogger<OperationsService> logger)
        {
            _context = context;
            _logger = logger;
        }

        #region KPIs Calculation
        // Ten indicators, every one of them a count, a distinct-count or a sum over the approved
        // Operations dispatch log (one row per bus assigned to a rental order on a day) - the
        // department's one template.
        public async Task<OperationsKpisDto> GetOperationsKpisAsync()
        {
            var records = await _context.OperationsDispatchRecords.AsNoTracking().ToListAsync();

            int totalOrders = records.Count;
            int completed = records.Count(r => r.IsCompleted);
            double completionRate = totalOrders > 0 ? (double)completed / totalOrders * 100.0 : 0;

            int rentalOrders = records.Select(r => r.RentOrder).Distinct(StringComparer.OrdinalIgnoreCase).Count();

            // The customer account code identifies a client far more reliably than its written
            // name, which arrives with inconsistent spacing across months; the name is only used
            // for rows where no account code was recorded.
            int clientsServed = records
                .Select(r => !string.IsNullOrWhiteSpace(r.CustomerAccount) ? r.CustomerAccount.Trim() : r.CustomerName.Trim())
                .Where(k => !string.IsNullOrWhiteSpace(k))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Count();

            int busesDeployed = records
                .Select(r => r.BusNumber.Trim())
                .Where(b => !string.IsNullOrWhiteSpace(b))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Count();

            // A dispatch line can carry a second (relief) driver; both count as drivers who worked.
            var drivers = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var r in records)
            {
                AddDriver(drivers, r.DriverNumber, r.DriverName);
                AddDriver(drivers, r.AdditionalDriverNumber, r.AdditionalDriverName);
            }

            double plannedKm = records.Sum(r => r.PlannedKm ?? 0);
            double actualKm = records.Sum(r => r.ActualKm ?? 0);
            double diesel = records.Sum(r => r.DieselLiters ?? 0);

            int distinctDays = records.Select(r => r.DeliveryDate.Date).Distinct().Count();
            double averageOrdersPerDay = distinctDays > 0 ? (double)totalOrders / distinctDays : 0;

            var topDirections = records
                .Where(r => !string.IsNullOrWhiteSpace(r.Direction))
                .GroupBy(r => r.Direction.Trim(), StringComparer.OrdinalIgnoreCase)
                .Select(g => new OperationsDirectionUsageDto
                {
                    Direction = g.Key,
                    DirectionName = g.Select(x => x.DirectionName).FirstOrDefault(n => !string.IsNullOrWhiteSpace(n)),
                    OrdersCount = g.Count(),
                    SharePercentage = totalOrders > 0 ? Math.Round((double)g.Count() / totalOrders * 100.0, 1) : 0
                })
                .OrderByDescending(d => d.OrdersCount)
                .Take(10)
                .ToList();

            return new OperationsKpisDto
            {
                TotalDispatchOrders = totalOrders,
                RentalOrdersCount = rentalOrders,
                ClientsServedCount = clientsServed,
                BusesDeployedCount = busesDeployed,
                DriversAssignedCount = drivers.Count,
                CompletedOrdersCount = completed,
                CompletionRatePercent = Math.Round(completionRate, 1),
                TotalPlannedKm = Math.Round(plannedKm, 1),
                TotalActualKm = Math.Round(actualKm, 1),
                TotalDieselLiters = Math.Round(diesel, 1),
                AverageOrdersPerDay = Math.Round(averageOrdersPerDay, 1),
                TopDirections = topDirections
            };
        }

        // A driver is identified by payroll number where the sheet gives one, and by name where it
        // doesn't - counting both raw columns separately would double-count anyone whose number was
        // left blank on some rows.
        private static void AddDriver(HashSet<string> set, string? number, string? name)
        {
            if (!string.IsNullOrWhiteSpace(number)) { set.Add(number.Trim()); return; }
            if (!string.IsNullOrWhiteSpace(name)) set.Add(name.Trim());
        }

        #endregion

        #region Dispatch records: index + CRUD
        public async Task<PagedResultDto<OperationsDispatchRecordDto>> GetDispatchRecordsPagedAsync(
            int page, int pageSize, string? search, DateTime? fromDate, DateTime? toDate)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 20;
            if (pageSize > 200) pageSize = 200;

            var query = _context.OperationsDispatchRecords.AsNoTracking().AsQueryable();
            if (fromDate.HasValue) query = query.Where(r => r.DeliveryDate >= fromDate.Value.Date);
            if (toDate.HasValue) query = query.Where(r => r.DeliveryDate < toDate.Value.Date.AddDays(1));
            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                query = query.Where(r =>
                    r.RentOrder.Contains(term) ||
                    r.CustomerName.Contains(term) ||
                    r.BusNumber.Contains(term) ||
                    r.DriverName.Contains(term) ||
                    r.Direction.Contains(term) ||
                    (r.DirectionName != null && r.DirectionName.Contains(term)));
            }

            var totalCount = await query.CountAsync();
            var items = await query
                .OrderByDescending(r => r.DeliveryDate).ThenBy(r => r.RentOrder).ThenBy(r => r.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResultDto<OperationsDispatchRecordDto>
            {
                Items = items.Select(ToDto).ToList(),
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<OperationsDispatchRecordDto?> GetDispatchRecordAsync(int id)
        {
            var record = await _context.OperationsDispatchRecords.AsNoTracking().FirstOrDefaultAsync(r => r.Id == id);
            return record == null ? null : ToDto(record);
        }

        public async Task<CrudResult<OperationsDispatchRecordDto>> CreateDispatchRecordAsync(OperationsDispatchRecordDto dto)
        {
            var errors = Validate(dto);
            if (errors.Count == 0 && await KeyTakenAsync(dto, excludeId: null))
                errors.Add("rentOrder", DuplicateKeyMessage);
            if (errors.Count > 0) return CrudResult<OperationsDispatchRecordDto>.Invalid(errors);

            var record = new OperationsDispatchRecord();
            Apply(dto, record);
            _context.OperationsDispatchRecords.Add(record);
            await _context.SaveChangesAsync();
            return CrudResult<OperationsDispatchRecordDto>.Ok(ToDto(record));
        }

        public async Task<CrudResult<OperationsDispatchRecordDto>> UpdateDispatchRecordAsync(int id, OperationsDispatchRecordDto dto)
        {
            var record = await _context.OperationsDispatchRecords.FirstOrDefaultAsync(r => r.Id == id);
            if (record == null) return CrudResult<OperationsDispatchRecordDto>.Missing();

            var errors = Validate(dto);
            if (errors.Count == 0 && await KeyTakenAsync(dto, excludeId: id))
                errors.Add("rentOrder", DuplicateKeyMessage);
            if (errors.Count > 0) return CrudResult<OperationsDispatchRecordDto>.Invalid(errors);

            Apply(dto, record);
            await _context.SaveChangesAsync();
            return CrudResult<OperationsDispatchRecordDto>.Ok(ToDto(record));
        }

        public async Task<bool> DeleteDispatchRecordAsync(int id)
        {
            var record = await _context.OperationsDispatchRecords.FirstOrDefaultAsync(r => r.Id == id);
            if (record == null) return false;
            _context.OperationsDispatchRecords.Remove(record);
            await _context.SaveChangesAsync();
            return true;
        }

        private const string DuplicateKeyMessage =
            "يوجد أمر تشغيل مسجل بنفس الخط وأمر الإيجار ورقم الحافلة وتاريخ التنفيذ.";

        private Task<bool> KeyTakenAsync(OperationsDispatchRecordDto dto, int? excludeId)
        {
            var day = dto.DeliveryDate.Date;
            var direction = dto.Direction.Trim();
            var rentOrder = dto.RentOrder.Trim();
            var bus = dto.BusNumber.Trim();
            return _context.OperationsDispatchRecords.AnyAsync(r =>
                r.DeliveryDate == day && r.Direction == direction && r.RentOrder == rentOrder && r.BusNumber == bus &&
                (excludeId == null || r.Id != excludeId));
        }

        // The single rule set for a dispatch line. The Excel import runs every row through it and
        // the page's add/edit form does the same, so both entry paths accept exactly the same data.
        // Field names are the DTO's camelCase property names, which is what the page's form uses.
        private static List<FieldErrorDto> Validate(OperationsDispatchRecordDto dto)
        {
            var errors = new List<FieldErrorDto>();

            if (string.IsNullOrWhiteSpace(dto.Direction)) errors.Add("direction", "كود الخط (Direction) مطلوب.");
            if (string.IsNullOrWhiteSpace(dto.RentOrder)) errors.Add("rentOrder", "أمر الإيجار (Rent Order) مطلوب.");
            if (string.IsNullOrWhiteSpace(dto.CustomerName)) errors.Add("customerName", "اسم العميل مطلوب.");
            if (string.IsNullOrWhiteSpace(dto.BusNumber)) errors.Add("busNumber", "رقم الحافلة مطلوب.");
            if (dto.DeliveryDate == default)
                errors.Add("deliveryDate", "تاريخ التنفيذ (DELV. Date) غير مقروء. استخدم الصيغة يوم/شهر/سنة.");
            if (string.IsNullOrWhiteSpace(dto.DriverName)) errors.Add("driverName", "اسم السائق مطلوب.");
            if (string.IsNullOrWhiteSpace(dto.Completion)) errors.Add("completion", "حالة التنفيذ (Completeion) مطلوبة.");

            if (dto.PlannedKm is < 0) errors.Add("plannedKm", "الكيلومترات المخططة لا يمكن أن تكون بالسالب.");
            if (dto.ActualKm is < 0) errors.Add("actualKm", "الكيلومترات الفعلية لا يمكن أن تكون بالسالب.");
            if (dto.DieselLiters is < 0) errors.Add("dieselLiters", "الديزل لا يمكن أن يكون بالسالب.");

            if (dto.PlannedStart.HasValue && dto.PlannedEnd.HasValue && dto.PlannedEnd < dto.PlannedStart)
                errors.Add("plannedEnd", "النهاية المخططة تسبق البداية المخططة.");

            return errors;
        }

        private static void Apply(OperationsDispatchRecordDto dto, OperationsDispatchRecord record)
        {
            record.Direction = Truncate(dto.Direction, 50);
            record.DirectionName = TruncateOrNull(dto.DirectionName, 200);
            record.RentOrder = Truncate(dto.RentOrder, 50);
            record.CustomerAccount = TruncateOrNull(dto.CustomerAccount, 50);
            record.CustomerName = Truncate(dto.CustomerName, 250);
            record.BusType = TruncateOrNull(dto.BusType, 100);
            record.BusNumber = Truncate(dto.BusNumber, 50);
            record.DeliveryDate = dto.DeliveryDate.Date;
            record.PlannedStart = dto.PlannedStart;
            record.PlannedEnd = dto.PlannedEnd;
            record.DriverNumber = TruncateOrNull(dto.DriverNumber, 50);
            record.DriverName = Truncate(dto.DriverName, 250);
            record.AdditionalDriverNumber = TruncateOrNull(dto.AdditionalDriverNumber, 50);
            record.AdditionalDriverName = TruncateOrNull(dto.AdditionalDriverName, 250);
            record.Location = TruncateOrNull(dto.Location, 200);
            record.FromLocation = TruncateOrNull(dto.FromLocation, 200);
            record.ToLocation = TruncateOrNull(dto.ToLocation, 200);
            record.ActualKm = dto.ActualKm;
            record.PlannedKm = dto.PlannedKm;
            record.DieselLiters = dto.DieselLiters;
            record.Completion = Truncate(dto.Completion, 50);
            record.IsCompleted = IsCompletedStatus(dto.Completion);
        }

        private static OperationsDispatchRecordDto ToDto(OperationsDispatchRecord r) => new()
        {
            Id = r.Id,
            Direction = r.Direction,
            DirectionName = r.DirectionName,
            RentOrder = r.RentOrder,
            CustomerAccount = r.CustomerAccount,
            CustomerName = r.CustomerName,
            BusType = r.BusType,
            BusNumber = r.BusNumber,
            DeliveryDate = r.DeliveryDate,
            PlannedStart = r.PlannedStart,
            PlannedEnd = r.PlannedEnd,
            DriverNumber = r.DriverNumber,
            DriverName = r.DriverName,
            AdditionalDriverNumber = r.AdditionalDriverNumber,
            AdditionalDriverName = r.AdditionalDriverName,
            Location = r.Location,
            FromLocation = r.FromLocation,
            ToLocation = r.ToLocation,
            ActualKm = r.ActualKm,
            PlannedKm = r.PlannedKm,
            DieselLiters = r.DieselLiters,
            Completion = r.Completion
        };
        #endregion

        #region Bulk Upload (the approved Operations dispatch template)
        // Excel column each DTO field is reported against when an import row is rejected.
        private static readonly Dictionary<string, string> ImportColumnFor = new()
        {
            ["direction"] = "Direction", ["rentOrder"] = "Rent Order", ["customerName"] = "Name",
            ["busNumber"] = "Bus number", ["deliveryDate"] = "DELV. Date", ["driverName"] = "Driver Name",
            ["completion"] = "Completeion", ["plannedKm"] = "PlannedKM", ["actualKm"] = "ActualKM",
            ["dieselLiters"] = "Desiel", ["plannedEnd"] = "PlannedEnd"
        };

        // One row per bus assigned to a rental order on a day. Direction + rental order + bus +
        // execution date is what identifies a line, so re-uploading a corrected month updates the
        // same rows instead of appending a second copy of them.
        public async Task<ExcelImportResultDto> BulkUploadDispatchLogAsync(System.IO.Stream excelStream)
        {
            var existing = await _context.OperationsDispatchRecords.ToListAsync();
            var seenInThisFile = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            var result = await ExcelImportEngine.RunAsync(
                excelStream,
                DepartmentTemplates.Operations,
                async row =>
                {
                    var dto = new OperationsDispatchRecordDto
                    {
                        Direction = row.GetString(DepartmentTemplates.OperationsDirection),
                        DirectionName = row.GetString(DepartmentTemplates.OperationsDirectionName),
                        RentOrder = row.GetString(DepartmentTemplates.OperationsRentOrder),
                        CustomerAccount = row.GetString(DepartmentTemplates.OperationsCustomerAccount),
                        CustomerName = row.GetString(DepartmentTemplates.OperationsCustomerName),
                        BusType = row.GetString(DepartmentTemplates.OperationsBusType),
                        BusNumber = row.GetString(DepartmentTemplates.OperationsBusNumber),
                        DeliveryDate = row.GetDate(DepartmentTemplates.OperationsDeliveryDate)?.Date ?? default,
                        PlannedStart = row.GetDate(DepartmentTemplates.OperationsPlannedStart),
                        PlannedEnd = row.GetDate(DepartmentTemplates.OperationsPlannedEnd),
                        DriverNumber = row.GetString(DepartmentTemplates.OperationsDriverNo),
                        DriverName = row.GetString(DepartmentTemplates.OperationsDriverName),
                        AdditionalDriverNumber = row.GetString(DepartmentTemplates.OperationsAddDriverNo),
                        AdditionalDriverName = row.GetString(DepartmentTemplates.OperationsAddDriverName),
                        Location = row.GetString(DepartmentTemplates.OperationsLocation),
                        FromLocation = row.GetString(DepartmentTemplates.OperationsFromLocation),
                        ToLocation = row.GetString(DepartmentTemplates.OperationsToLocation),
                        ActualKm = (double?)row.GetDecimal(DepartmentTemplates.OperationsActualKm),
                        PlannedKm = (double?)row.GetDecimal(DepartmentTemplates.OperationsPlannedKm),
                        DieselLiters = (double?)row.GetDecimal(DepartmentTemplates.OperationsDiesel),
                        Completion = row.GetString(DepartmentTemplates.OperationsCompletion)
                    };

                    var errors = Validate(dto);
                    if (errors.Count > 0)
                        return ExcelRowOutcomeResult.Skipped(errors.Joined(), ImportColumnFor.GetValueOrDefault(errors[0].Field));

                    var direction = Truncate(dto.Direction, 50);
                    var rentOrder = Truncate(dto.RentOrder, 50);
                    var busNumber = Truncate(dto.BusNumber, 50);
                    var day = dto.DeliveryDate.Date;

                    // The same dispatch line appearing twice inside one uploaded file is a data
                    // entry mistake, and is reported rather than silently collapsed - otherwise a
                    // duplicated row would look imported while quietly overwriting its twin.
                    if (!seenInThisFile.Add($"{day:yyyy-MM-dd}|{rentOrder}|{busNumber}|{direction}"))
                        return ExcelRowOutcomeResult.Skipped(
                            "هذا الصف مكرر داخل نفس الملف (نفس الخط وأمر الإيجار والحافلة والتاريخ).", "Rent Order");

                    var record = existing.FirstOrDefault(r =>
                        r.DeliveryDate.Date == day &&
                        string.Equals(r.RentOrder, rentOrder, StringComparison.OrdinalIgnoreCase) &&
                        string.Equals(r.BusNumber, busNumber, StringComparison.OrdinalIgnoreCase) &&
                        string.Equals(r.Direction, direction, StringComparison.OrdinalIgnoreCase));

                    bool isNew = record == null;
                    record ??= new OperationsDispatchRecord();
                    Apply(dto, record);

                    if (isNew)
                    {
                        _context.OperationsDispatchRecords.Add(record);
                        existing.Add(record);
                        await System.Threading.Tasks.Task.CompletedTask;
                        return ExcelRowOutcomeResult.Inserted();
                    }

                    return ExcelRowOutcomeResult.Updated();
                },
                () => _context.SaveChangesAsync(),
                _logger);

            return result;
        }

        // The Completeion column is free text in practice ("Completed", "مكتمل", "Cancelled",
        // "ملغي"), so it is stored verbatim and only the completed/not-completed decision is
        // normalised here, where the completion-rate KPI depends on it.
        private static bool IsCompletedStatus(string raw)
        {
            var value = raw.Trim();
            if (value.IndexOf("complete", StringComparison.OrdinalIgnoreCase) >= 0) return true;
            if (value.IndexOf("closed", StringComparison.OrdinalIgnoreCase) >= 0) return true;
            if (value.IndexOf("done", StringComparison.OrdinalIgnoreCase) >= 0) return true;
            if (value.Contains("مكتمل") || value.Contains("منفذ") || value.Contains("تم")) return true;
            return false;
        }

        private static string Truncate(string value, int maxLength)
        {
            var trimmed = value.Trim();
            return trimmed.Length <= maxLength ? trimmed : trimmed.Substring(0, maxLength);
        }

        private static string? TruncateOrNull(string? value, int maxLength) =>
            string.IsNullOrWhiteSpace(value) ? null : Truncate(value, maxLength);
        #endregion
    }
}
