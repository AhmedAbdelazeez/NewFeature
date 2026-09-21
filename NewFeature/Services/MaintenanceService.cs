using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NewFeature.Models;
using NewFeature.Services.Repositories;
using NewFeature.Services.ExcelImport;

namespace NewFeature.Services
{
    public class MaintenanceService : IMaintenanceService
    {
        private readonly IRepository<MaintenanceWorkOrder> _workOrderRepository;
        private readonly IRepository<SparePartConsumption> _partRepository;
        private readonly IRepository<Vehicle> _vehicleRepository;
        private readonly IRepository<InventoryItem> _inventoryRepository;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<MaintenanceService> _logger;

        public MaintenanceService(
            IRepository<MaintenanceWorkOrder> workOrderRepository,
            IRepository<SparePartConsumption> partRepository,
            IRepository<Vehicle> vehicleRepository,
            IRepository<InventoryItem> inventoryRepository,
            ApplicationDbContext context,
            ILogger<MaintenanceService> logger)
        {
            _workOrderRepository = workOrderRepository;
            _partRepository = partRepository;
            _vehicleRepository = vehicleRepository;
            _inventoryRepository = inventoryRepository;
            _context = context;
            _logger = logger;
        }


        #region Work orders: index + CRUD (the Maintenance page)
        // Paged, date-filterable and searchable. The executive dashboard's work-orders table reads
        // this same endpoint (without a search term), so its shape is unchanged.
        public async Task<PagedResultDto<MaintenanceWorkOrderDto>> GetWorkOrdersPagedAsync(
            int page, int pageSize, DateTime? fromDate, DateTime? toDate, string? search = null)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 200) pageSize = 200;

            var query = _context.MaintenanceWorkOrders.AsNoTracking().Include(o => o.Vehicle).AsQueryable();
            if (fromDate.HasValue) query = query.Where(o => o.Date >= fromDate.Value.Date);
            if (toDate.HasValue) query = query.Where(o => o.Date < toDate.Value.Date.AddDays(1));
            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                query = query.Where(o =>
                    (o.WorkOrderNumber != null && o.WorkOrderNumber.Contains(term)) ||
                    o.BreakdownDescription.Contains(term) ||
                    o.TechnicianName.Contains(term) ||
                    o.SupervisorName.Contains(term) ||
                    (o.Vehicle != null && ((o.Vehicle.BusNumber != null && o.Vehicle.BusNumber.Contains(term)) || o.Vehicle.LicensePlate.Contains(term))));
            }

            var totalCount = await query.CountAsync();
            var orders = await query
                .OrderByDescending(o => o.Date).ThenByDescending(o => o.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResultDto<MaintenanceWorkOrderDto>
            {
                Items = orders.Select(ToDto).ToList(),
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<MaintenanceWorkOrderDto?> GetWorkOrderByIdAsync(int id)
        {
            var order = await _context.MaintenanceWorkOrders.AsNoTracking()
                .Include(o => o.Vehicle)
                .FirstOrDefaultAsync(o => o.Id == id);
            return order == null ? null : ToDto(order);
        }

        public async Task<CrudResult<MaintenanceWorkOrderDto>> CreateWorkOrderAsync(MaintenanceWorkOrderDto dto)
        {
            var errors = Validate(dto);
            if (errors.Count == 0 && await WorkOrderNumberTakenAsync(dto.WorkOrderNumber!, excludeId: null))
                errors.Add("workOrderNumber", DuplicateNumberMessage);
            if (errors.Count > 0) return CrudResult<MaintenanceWorkOrderDto>.Invalid(errors);

            var vehicles = await _context.Vehicles.ToListAsync();
            var order = new MaintenanceWorkOrder();
            Apply(dto, order, ResolveVehicle(vehicles, dto.BusNumber!.Trim()));
            _context.MaintenanceWorkOrders.Add(order);
            await _context.SaveChangesAsync();

            return CrudResult<MaintenanceWorkOrderDto>.Ok(ToDto(order));
        }

        public async Task<CrudResult<MaintenanceWorkOrderDto>> UpdateWorkOrderAsync(int id, MaintenanceWorkOrderDto dto)
        {
            var order = await _context.MaintenanceWorkOrders.FirstOrDefaultAsync(o => o.Id == id);
            if (order == null) return CrudResult<MaintenanceWorkOrderDto>.Missing();

            var errors = Validate(dto);
            if (errors.Count == 0 && await WorkOrderNumberTakenAsync(dto.WorkOrderNumber!, excludeId: id))
                errors.Add("workOrderNumber", DuplicateNumberMessage);
            if (errors.Count > 0) return CrudResult<MaintenanceWorkOrderDto>.Invalid(errors);

            var vehicles = await _context.Vehicles.ToListAsync();
            Apply(dto, order, ResolveVehicle(vehicles, dto.BusNumber!.Trim()));
            await _context.SaveChangesAsync();

            return CrudResult<MaintenanceWorkOrderDto>.Ok(ToDto(order));
        }

        public async Task<bool> DeleteWorkOrderAsync(int id)
        {
            var order = await _context.MaintenanceWorkOrders.FirstOrDefaultAsync(o => o.Id == id);
            if (order == null) return false;

            // Parts recorded against the order by older uploads go with it.
            var parts = await _context.SparePartConsumptions.Where(p => p.MaintenanceWorkOrderId == id).ToListAsync();
            _context.SparePartConsumptions.RemoveRange(parts);
            _context.MaintenanceWorkOrders.Remove(order);
            await _context.SaveChangesAsync();
            return true;
        }

        private const string DuplicateNumberMessage = "رقم أمر العمل مسجل مسبقاً لأمر عمل آخر.";

        private Task<bool> WorkOrderNumberTakenAsync(string number, int? excludeId)
        {
            var value = number.Trim();
            return _context.MaintenanceWorkOrders.AnyAsync(o =>
                o.WorkOrderNumber == value && (excludeId == null || o.Id != excludeId));
        }

        // The single rule set for a work order. The Excel import runs every row through it and the
        // page's add/edit form does the same, so both entry paths accept exactly the same data.
        // Field names are the DTO's camelCase property names, which is what the page's form uses.
        private static List<FieldErrorDto> Validate(MaintenanceWorkOrderDto dto)
        {
            var errors = new List<FieldErrorDto>();

            if (string.IsNullOrWhiteSpace(dto.WorkOrderNumber)) errors.Add("workOrderNumber", "رقم أمر العمل مطلوب.");
            if (string.IsNullOrWhiteSpace(dto.BusNumber)) errors.Add("busNumber", "رقم الحافلة مطلوب.");
            if (string.IsNullOrWhiteSpace(dto.BreakdownDescription)) errors.Add("breakdownDescription", "وصف العطل مطلوب.");
            if (dto.TimeIn == default)
                errors.Add("timeIn", "تاريخ الدخول غير مقروء. استخدم الصيغة يوم/شهر/سنة.");
            if (string.IsNullOrWhiteSpace(dto.TechnicianName)) errors.Add("technicianName", "اسم الفني 1 مطلوب.");
            if (!Enum.IsDefined(typeof(WorkOrderStatus), dto.Status))
                errors.Add("status", "الحالة يجب أن تكون \"تم الانتهاء\" أو \"جاري العمل\" أو \"متوقف علي قطع غيار\".");

            if (dto.TimeIn != default && dto.TimeOut.HasValue && dto.TimeOut.Value < dto.TimeIn)
                errors.Add("timeOut", "تاريخ/ساعة الخروج تسبق تاريخ/ساعة الدخول.");
            if (dto.Status == WorkOrderStatus.Completed && dto.TimeOut == null)
                errors.Add("timeOut", "أمر عمل بحالة \"تم الانتهاء\" يجب أن يحتوي على تاريخ أو ساعة خروج.");

            if (dto.Odometer < 0 || dto.Odometer > 5000000)
                errors.Add("odometer", "قراءة العداد يجب أن تكون بين 0 و 5,000,000.");

            return errors;
        }

        private static void Apply(MaintenanceWorkOrderDto dto, MaintenanceWorkOrder order, Vehicle vehicle)
        {
            order.WorkOrderNumber = Truncate(dto.WorkOrderNumber!.Trim(), 50);
            order.Vehicle = vehicle;
            if (vehicle.Id > 0) order.VehicleId = vehicle.Id;
            order.Date = dto.TimeIn.Date;
            order.TimeIn = dto.TimeIn;
            order.TimeOut = dto.TimeOut;
            order.Odometer = dto.Odometer;
            order.BreakdownDescription = Truncate(dto.BreakdownDescription.Trim(), 500);
            order.BranchLocation = Truncate(string.IsNullOrWhiteSpace(dto.BranchLocation) ? "الورشة المركزية" : dto.BranchLocation.Trim(), 100);
            order.SupervisorName = Truncate((dto.SupervisorName ?? string.Empty).Trim(), 150);
            order.TechnicianName = Truncate(dto.TechnicianName.Trim(), 150);
            order.TechnicianName2 = TruncateOrNull(dto.TechnicianName2, 150);
            order.TechnicianName3 = TruncateOrNull(dto.TechnicianName3, 150);
            order.TechnicianName4 = TruncateOrNull(dto.TechnicianName4, 150);
            order.Status = dto.Status;
            order.Remarks = Truncate((dto.Remarks ?? string.Empty).Trim(), 500);
        }

        private static MaintenanceWorkOrderDto ToDto(MaintenanceWorkOrder o) => new()
        {
            Id = o.Id,
            WorkOrderNumber = o.WorkOrderNumber,
            VehicleId = o.VehicleId,
            VehiclePlate = o.Vehicle?.LicensePlate ?? "Unknown",
            BusNumber = o.Vehicle?.BusNumber,
            Date = o.Date,
            Odometer = o.Odometer,
            BreakdownDescription = o.BreakdownDescription,
            TimeIn = o.TimeIn,
            TimeOut = o.TimeOut,
            BranchLocation = o.BranchLocation,
            BreakdownLocation = o.BreakdownLocation,
            SupervisorName = o.SupervisorName,
            TechnicianName = o.TechnicianName,
            TechnicianName2 = o.TechnicianName2,
            TechnicianName3 = o.TechnicianName3,
            TechnicianName4 = o.TechnicianName4,
            Status = o.Status,
            Remarks = o.Remarks
        };
        #endregion

        #region KPIs
        // Eight indicators, every one of them read straight off the approved "Internal work orders"
        // sheet. Nothing here consults the Vehicles table for a fleet-wide availability figure any
        // more: the workshop sheet only knows about buses that entered the workshop, so a
        // whole-fleet percentage derived from it was never a number the upload could support.
        public async Task<MaintenanceKpisDto> GetMaintenanceKpisAsync()
        {
            var orders = (await _workOrderRepository.GetAllAsync()).ToList();
            var vehicles = (await _vehicleRepository.GetAllAsync()).ToList();
            var vehicleMap = vehicles.ToDictionary(v => v.Id, v => v);

            int totalWorkOrders = orders.Count;
            int completedWorkOrders = orders.Count(o => o.Status == WorkOrderStatus.Completed);
            int waitingParts = orders.Count(o => o.Status == WorkOrderStatus.WaitingParts);
            int inProgress = orders.Count(o => o.Status == WorkOrderStatus.InAnalysis || o.Status == WorkOrderStatus.Pending);
            int openWorkOrders = totalWorkOrders - completedWorkOrders;

            double completionRate = totalWorkOrders > 0
                ? (double)completedWorkOrders / totalWorkOrders * 100.0
                : 0;

            double backlogRate = totalWorkOrders > 0
                ? (double)openWorkOrders / totalWorkOrders * 100.0
                : 0;

            // Guard against bad data (e.g. an exit hour typed into the wrong column, producing a
            // negative or wildly long repair) skewing the average - cap at 30 days per repair.
            double mttr = 0;
            var validDurations = orders
                .Where(o => o.Status == WorkOrderStatus.Completed && o.TimeOut.HasValue)
                .Select(o => (o.TimeOut!.Value - o.TimeIn).TotalHours)
                .Where(h => h >= 0 && h <= 720)
                .ToList();
            if (validDurations.Any())
            {
                mttr = validDurations.Average();
            }

            int vehiclesServiced = orders.Select(o => o.VehicleId).Distinct().Count();

            // A work order can name up to four technicians; the headcount is how many distinct
            // people appear across all four slots, not how many rows were filled in.
            var technicians = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var o in orders)
            {
                AddTechnician(technicians, o.TechnicianName);
                AddTechnician(technicians, o.TechnicianName2);
                AddTechnician(technicians, o.TechnicianName3);
                AddTechnician(technicians, o.TechnicianName4);
            }

            var freqBreakdowns = orders.GroupBy(o => o.VehicleId)
                .Select(g => new BusBreakdownFrequencyDto
                {
                    VehiclePlate = vehicleMap.TryGetValue(g.Key, out var v) ? v.LicensePlate : $"Bus #{g.Key}",
                    BusNumber = vehicleMap.TryGetValue(g.Key, out var v2) ? v2.BusNumber : null,
                    BreakdownCount = g.Count()
                })
                .OrderByDescending(f => f.BreakdownCount)
                .Take(10)
                .ToList();

            // Fleet-wide figures, kept for the Fleet department's own cards. Availability is a
            // whole-fleet number, so it is computed against the Vehicles register rather than
            // against the buses that happen to appear in the workshop sheet.
            int openAgainstFleet = orders.Count(o => o.Status != WorkOrderStatus.Completed);
            double fleetAvailability = vehicles.Count > 0
                ? Math.Max(0, (double)(vehicles.Count - openAgainstFleet) / vehicles.Count) * 100.0
                : 100.0;

            var parts = await _partRepository.GetAllAsync();
            decimal totalPartsCost = parts.Sum(p => p.Quantity * p.UnitPrice);

            return new MaintenanceKpisDto
            {
                FleetAvailabilityRate = Math.Round(fleetAvailability, 2),
                TotalSparePartsCost = totalPartsCost,
                TotalWorkOrders = totalWorkOrders,
                CompletedWorkOrders = completedWorkOrders,
                CompletionRatePercent = Math.Round(completionRate, 1),
                MeanTimeToRepairHours = Math.Round(mttr, 2),
                WaitingPartsCount = waitingParts,
                InProgressCount = inProgress,
                MaintenanceBacklogRate = Math.Round(backlogRate, 1),
                VehiclesServicedCount = vehiclesServiced,
                ActiveTechniciansCount = technicians.Count,
                TopFrequentBreakdowns = freqBreakdowns
            };
        }

        private static void AddTechnician(HashSet<string> set, string? name)
        {
            if (!string.IsNullOrWhiteSpace(name)) set.Add(name.Trim());
        }

        #endregion

        #region Bulk upload (the approved "Internal work orders" template)
        // Excel column each DTO field is reported against when an import row is rejected.
        private static readonly Dictionary<string, string> ImportColumnFor = new()
        {
            ["workOrderNumber"] = "رقم امر العمل", ["busNumber"] = "رقم الحافلة",
            ["breakdownDescription"] = "وصف العطل", ["timeIn"] = "التاريخ الدخول",
            ["technicianName"] = "اسم الفنى 1", ["status"] = "حالة", ["timeOut"] = "تاريخ الخروج",
            ["odometer"] = "عداد كم الحالي"
        };

        // The work-order number identifies a row, so re-uploading a corrected month updates the
        // same work orders instead of appending a second copy of them.
        public async Task<ExcelImportResultDto> BulkUploadWorkOrdersAsync(Stream excelStream, string branchName)
        {
            // Loaded once and grown in memory as placeholder vehicles are created below. A workshop
            // log can carry thousands of rows, so a DB round trip per row would make a large upload
            // take minutes; everything below is only tracked, and the engine commits the whole
            // batch in a single save at the end.
            var vehicles = await _context.Vehicles.ToListAsync();
            var existingOrders = await _context.MaintenanceWorkOrders.ToListAsync();

            var result = await ExcelImportEngine.RunAsync(
                excelStream,
                DepartmentTemplates.Maintenance,
                async row =>
                {
                    var dateIn = row.GetDate(DepartmentTemplates.MaintenanceDateIn);

                    // Exit is only recorded when the sheet actually records one: an open work order
                    // must not be given a made-up exit time, because MTTR is averaged over these.
                    DateTime? timeOut = null;
                    var dateOut = row.GetDate(DepartmentTemplates.MaintenanceDateOut);
                    var hourOut = row.GetTime(DepartmentTemplates.MaintenanceTimeOut);
                    if (dateOut != null) timeOut = dateOut.Value.Date + (hourOut ?? TimeSpan.Zero);
                    else if (hourOut != null && dateIn != null) timeOut = dateIn.Value.Date + hourOut.Value; // same day

                    var statusText = row.GetString(DepartmentTemplates.MaintenanceStatus);
                    var status = ParseWorkOrderStatus(statusText);

                    var dto = new MaintenanceWorkOrderDto
                    {
                        WorkOrderNumber = row.GetString(DepartmentTemplates.MaintenanceWorkOrderNumber),
                        BusNumber = row.GetString(DepartmentTemplates.MaintenanceBusNumber),
                        Odometer = row.GetInt(DepartmentTemplates.MaintenanceOdometer) ?? 0,
                        BreakdownDescription = row.GetString(DepartmentTemplates.MaintenanceBreakdownDescription),
                        // A blank entry hour leaves the work order at midnight rather than inventing a shift start.
                        TimeIn = dateIn == null ? default : dateIn.Value.Date + (row.GetTime(DepartmentTemplates.MaintenanceTimeIn) ?? TimeSpan.Zero),
                        TimeOut = timeOut,
                        BranchLocation = branchName,
                        TechnicianName = row.GetString(DepartmentTemplates.MaintenanceTechnician1),
                        TechnicianName2 = row.GetString(DepartmentTemplates.MaintenanceTechnician2),
                        TechnicianName3 = row.GetString(DepartmentTemplates.MaintenanceTechnician3),
                        TechnicianName4 = row.GetString(DepartmentTemplates.MaintenanceTechnician4),
                        // An unreadable status is carried as an out-of-range value so the shared
                        // validator reports it with the same message the form shows.
                        Status = status ?? (WorkOrderStatus)(-1),
                        SupervisorName = row.GetString(DepartmentTemplates.MaintenanceSupervisor),
                        Remarks = row.GetString(DepartmentTemplates.MaintenanceNotes)
                    };

                    var errors = Validate(dto);
                    if (errors.Count > 0)
                        return ExcelRowOutcomeResult.Skipped(errors.Joined(), ImportColumnFor.GetValueOrDefault(errors[0].Field));

                    var number = dto.WorkOrderNumber!.Trim();
                    var order = existingOrders.FirstOrDefault(o =>
                        !string.IsNullOrEmpty(o.WorkOrderNumber) &&
                        string.Equals(o.WorkOrderNumber, number, StringComparison.OrdinalIgnoreCase));

                    bool isNew = order == null;
                    order ??= new MaintenanceWorkOrder();
                    Apply(dto, order, ResolveVehicle(vehicles, dto.BusNumber!.Trim()));

                    if (isNew)
                    {
                        _context.MaintenanceWorkOrders.Add(order);
                        existingOrders.Add(order);
                        await System.Threading.Tasks.Task.CompletedTask;
                        return ExcelRowOutcomeResult.Inserted();
                    }

                    return ExcelRowOutcomeResult.Updated();
                },
                () => _context.SaveChangesAsync(),
                _logger);

            if (result.Success && (result.InsertedRows > 0 || result.UpdatedRows > 0))
                await Repositories.DbMaintenanceHelper.RefreshStatisticsAsync(_context, _logger, "MaintenanceWorkOrders", "Vehicles");

            return result;
        }

        // The bus column is the short internal bus number (e.g. "2002"), not a license plate, so
        // it is matched against Vehicle.BusNumber first and only falls back to a plate substring
        // for older records that never had a BusNumber. A bus the fleet register has never heard
        // of gets a placeholder so the work order still saves: LicensePlate is capped at 20 chars
        // and unique in the DB, while BusNumber (30 chars, not unique) keeps the untruncated value
        // so a later row with the same label matches this same placeholder instead of colliding.
        private static Vehicle ResolveVehicle(List<Vehicle> vehicles, string busNumber)
        {
            var vehicle = vehicles.FirstOrDefault(v => v.BusNumber == busNumber)
                          ?? vehicles.FirstOrDefault(v => v.LicensePlate.Contains(busNumber) ||
                                                          v.LicensePlate.Replace(" ", "").Contains(busNumber));
            if (vehicle != null) return vehicle;

            vehicle = new Vehicle
            {
                BusNumber = Truncate(busNumber, 30),
                LicensePlate = Truncate("أ د ي " + busNumber, 20),
                Make = "Yutong",
                Model = "Placeholder",
                Year = 2020,
                Capacity = 49,
                Status = VehicleStatus.Available
            };
            vehicles.Add(vehicle);
            return vehicle;
        }

        // The approved sheet's status column is a three-value dropdown. Anything outside it is a
        // data-entry mistake and is reported back per-row rather than being silently bucketed.
        private static WorkOrderStatus? ParseWorkOrderStatus(string? raw)
        {
            var value = (raw ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(value)) return null;

            if (value.Contains("انتهاء") || value.Contains("مكتمل") || value.Contains("منتهي") ||
                value.IndexOf("complete", StringComparison.OrdinalIgnoreCase) >= 0 ||
                value.IndexOf("done", StringComparison.OrdinalIgnoreCase) >= 0)
                return WorkOrderStatus.Completed;

            if (value.Contains("قطع") || value.Contains("متوقف") || value.Contains("معلق") ||
                value.IndexOf("waiting", StringComparison.OrdinalIgnoreCase) >= 0 ||
                value.IndexOf("parts", StringComparison.OrdinalIgnoreCase) >= 0)
                return WorkOrderStatus.WaitingParts;

            if (value.Contains("جاري") || value.Contains("قيد") ||
                value.IndexOf("progress", StringComparison.OrdinalIgnoreCase) >= 0 ||
                value.IndexOf("working", StringComparison.OrdinalIgnoreCase) >= 0)
                return WorkOrderStatus.InAnalysis;

            return null;
        }

        private static string Truncate(string value, int maxLength) =>
            value.Length <= maxLength ? value : value.Substring(0, maxLength);

        private static string? TruncateOrNull(string? value, int maxLength) =>
            string.IsNullOrWhiteSpace(value) ? null : Truncate(value.Trim(), maxLength);
        #endregion
    }
}
