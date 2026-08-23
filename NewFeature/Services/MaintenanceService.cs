using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NewFeature.Models;
using NewFeature.Services.Repositories;
using ClosedXML.Excel;

namespace NewFeature.Services
{
    public class MaintenanceService : IMaintenanceService
    {
        private readonly IRepository<MaintenanceWorkOrder> _workOrderRepository;
        private readonly IRepository<SparePartConsumption> _partRepository;
        private readonly IRepository<Vehicle> _vehicleRepository;
        private readonly IRepository<InventoryItem> _inventoryRepository;

        public MaintenanceService(
            IRepository<MaintenanceWorkOrder> workOrderRepository,
            IRepository<SparePartConsumption> partRepository,
            IRepository<Vehicle> vehicleRepository,
            IRepository<InventoryItem> inventoryRepository)
        {
            _workOrderRepository = workOrderRepository;
            _partRepository = partRepository;
            _vehicleRepository = vehicleRepository;
            _inventoryRepository = inventoryRepository;
        }

        public async Task<IEnumerable<MaintenanceWorkOrderDto>> GetAllWorkOrdersAsync()
        {
            var orders = await _workOrderRepository.GetAllAsync();
            var vehicles = await _vehicleRepository.GetAllAsync();
            var parts = await _partRepository.GetAllAsync();

            var vehicleMap = vehicles.ToDictionary(v => v.Id, v => v.LicensePlate);

            return orders.OrderByDescending(o => o.Date).Select(o => new MaintenanceWorkOrderDto
            {
                Id = o.Id,
                VehicleId = o.VehicleId,
                VehiclePlate = vehicleMap.TryGetValue(o.VehicleId, out var plate) ? plate : "Unknown",
                Date = o.Date,
                Odometer = o.Odometer,
                BreakdownDescription = o.BreakdownDescription,
                TimeIn = o.TimeIn,
                TimeOut = o.TimeOut,
                BranchLocation = o.BranchLocation,
                BreakdownLocation = o.BreakdownLocation,
                SupervisorName = o.SupervisorName,
                TechnicianName = o.TechnicianName,
                Status = o.Status,
                Remarks = o.Remarks,
                ConsumedParts = parts.Where(p => p.MaintenanceWorkOrderId == o.Id).Select(p => new SparePartConsumptionDto
                {
                    Id = p.Id,
                    MaintenanceWorkOrderId = p.MaintenanceWorkOrderId,
                    PartName = p.PartName,
                    Quantity = p.Quantity,
                    UnitPrice = p.UnitPrice,
                    InventoryItemId = p.InventoryItemId
                }).ToList()
            }).ToList();
        }

        public async Task<PagedResultDto<MaintenanceWorkOrderDto>> GetWorkOrdersPagedAsync(int page, int pageSize, DateTime? fromDate, DateTime? toDate)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 200) pageSize = 200;

            var allOrders = (await _workOrderRepository.GetAllAsync()).AsEnumerable();

            if (fromDate.HasValue)
                allOrders = allOrders.Where(o => o.Date >= fromDate.Value.Date);
            if (toDate.HasValue)
                allOrders = allOrders.Where(o => o.Date < toDate.Value.Date.AddDays(1));

            var filteredOrders = allOrders.OrderByDescending(o => o.Date).ToList();
            var totalCount = filteredOrders.Count;

            var pagedOrders = filteredOrders
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var vehicles = await _vehicleRepository.GetAllAsync();
            var vehicleMap = vehicles.ToDictionary(v => v.Id, v => v.LicensePlate);
            var parts = await _partRepository.GetAllAsync();

            var items = pagedOrders.Select(o => new MaintenanceWorkOrderDto
            {
                Id = o.Id,
                VehicleId = o.VehicleId,
                VehiclePlate = vehicleMap.TryGetValue(o.VehicleId, out var plate) ? plate : "Unknown",
                Date = o.Date,
                Odometer = o.Odometer,
                BreakdownDescription = o.BreakdownDescription,
                TimeIn = o.TimeIn,
                TimeOut = o.TimeOut,
                BranchLocation = o.BranchLocation,
                BreakdownLocation = o.BreakdownLocation,
                SupervisorName = o.SupervisorName,
                TechnicianName = o.TechnicianName,
                Status = o.Status,
                Remarks = o.Remarks,
                ConsumedParts = parts.Where(p => p.MaintenanceWorkOrderId == o.Id).Select(p => new SparePartConsumptionDto
                {
                    Id = p.Id,
                    MaintenanceWorkOrderId = p.MaintenanceWorkOrderId,
                    PartName = p.PartName,
                    Quantity = p.Quantity,
                    UnitPrice = p.UnitPrice,
                    InventoryItemId = p.InventoryItemId
                }).ToList()
            }).ToList();

            return new PagedResultDto<MaintenanceWorkOrderDto>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<MaintenanceWorkOrderDto?> GetWorkOrderByIdAsync(int id)
        {
            var o = await _workOrderRepository.GetByIdAsync(id);
            if (o == null) return null;

            var vehicle = await _vehicleRepository.GetByIdAsync(o.VehicleId);
            var parts = (await _partRepository.GetAllAsync()).Where(p => p.MaintenanceWorkOrderId == id).ToList();

            return new MaintenanceWorkOrderDto
            {
                Id = o.Id,
                VehicleId = o.VehicleId,
                VehiclePlate = vehicle?.LicensePlate ?? "Unknown",
                Date = o.Date,
                Odometer = o.Odometer,
                BreakdownDescription = o.BreakdownDescription,
                TimeIn = o.TimeIn,
                TimeOut = o.TimeOut,
                BranchLocation = o.BranchLocation,
                BreakdownLocation = o.BreakdownLocation,
                SupervisorName = o.SupervisorName,
                TechnicianName = o.TechnicianName,
                Status = o.Status,
                Remarks = o.Remarks,
                ConsumedParts = parts.Select(p => new SparePartConsumptionDto
                {
                    Id = p.Id,
                    MaintenanceWorkOrderId = p.MaintenanceWorkOrderId,
                    PartName = p.PartName,
                    Quantity = p.Quantity,
                    UnitPrice = p.UnitPrice,
                    InventoryItemId = p.InventoryItemId
                }).ToList()
            };
        }

        public async Task<MaintenanceWorkOrderDto> CreateWorkOrderAsync(MaintenanceWorkOrderDto dto)
        {
            var o = new MaintenanceWorkOrder
            {
                VehicleId = dto.VehicleId,
                Date = dto.Date,
                Odometer = dto.Odometer,
                BreakdownDescription = dto.BreakdownDescription,
                TimeIn = dto.TimeIn,
                TimeOut = dto.TimeOut,
                BranchLocation = dto.BranchLocation,
                BreakdownLocation = dto.BreakdownLocation,
                SupervisorName = dto.SupervisorName,
                TechnicianName = dto.TechnicianName,
                Status = dto.Status,
                Remarks = dto.Remarks
            };

            await _workOrderRepository.AddAsync(o);
            await _workOrderRepository.SaveChangesAsync();

            dto.Id = o.Id;

            if (dto.ConsumedParts != null && dto.ConsumedParts.Any())
            {
                foreach (var p in dto.ConsumedParts)
                {
                    var part = new SparePartConsumption
                    {
                        MaintenanceWorkOrderId = o.Id,
                        PartName = p.PartName,
                        Quantity = p.Quantity,
                        UnitPrice = p.UnitPrice,
                        InventoryItemId = p.InventoryItemId
                    };
                    await _partRepository.AddAsync(part);
                }
                await _partRepository.SaveChangesAsync();
            }

            return dto;
        }

        public async Task<bool> UpdateWorkOrderAsync(MaintenanceWorkOrderDto dto)
        {
            var o = await _workOrderRepository.GetByIdAsync(dto.Id);
            if (o == null) return false;

            o.VehicleId = dto.VehicleId;
            o.Date = dto.Date;
            o.Odometer = dto.Odometer;
            o.BreakdownDescription = dto.BreakdownDescription;
            o.TimeIn = dto.TimeIn;
            o.TimeOut = dto.TimeOut;
            o.BranchLocation = dto.BranchLocation;
            o.BreakdownLocation = dto.BreakdownLocation;
            o.SupervisorName = dto.SupervisorName;
            o.TechnicianName = dto.TechnicianName;
            o.Status = dto.Status;
            o.Remarks = dto.Remarks;

            await _workOrderRepository.UpdateAsync(o);

            // Simple parts reconciliation: delete old parts, insert new
            var existingParts = (await _partRepository.GetAllAsync()).Where(p => p.MaintenanceWorkOrderId == o.Id).ToList();
            foreach (var p in existingParts)
            {
                await _partRepository.DeleteAsync(p);
            }

            if (dto.ConsumedParts != null && dto.ConsumedParts.Any())
            {
                foreach (var p in dto.ConsumedParts)
                {
                    var part = new SparePartConsumption
                    {
                        MaintenanceWorkOrderId = o.Id,
                        PartName = p.PartName,
                        Quantity = p.Quantity,
                        UnitPrice = p.UnitPrice,
                        InventoryItemId = p.InventoryItemId
                    };
                    await _partRepository.AddAsync(part);
                }
            }

            await _workOrderRepository.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteWorkOrderAsync(int id)
        {
            var o = await _workOrderRepository.GetByIdAsync(id);
            if (o == null) return false;

            await _workOrderRepository.DeleteAsync(o);
            await _workOrderRepository.SaveChangesAsync();
            return true;
        }

        public async Task<MaintenanceKpisDto> GetMaintenanceKpisAsync()
        {
            var orders = await _workOrderRepository.GetAllAsync();
            var parts = await _partRepository.GetAllAsync();
            var vehicles = await _vehicleRepository.GetAllAsync();

            var completedOrders = orders.Where(o => o.Status == WorkOrderStatus.Completed && o.TimeOut.HasValue).ToList();
            
            // Guard against bad data (e.g. a bulk-uploaded row with a missing/garbled Time Out that
            // parses to a wildly wrong date) skewing the average — cap at 30 days per repair.
            double mttr = 0;
            var validDurations = completedOrders
                .Select(o => (o.TimeOut!.Value - o.TimeIn).TotalHours)
                .Where(h => h >= 0 && h <= 720)
                .ToList();
            if (validDurations.Any())
            {
                mttr = validDurations.Average();
            }

            int totalBreakdowns = orders.Count();

            int activeMaintenanceCount = orders.Count(o => o.Status == WorkOrderStatus.Pending || 
                                                           o.Status == WorkOrderStatus.InAnalysis || 
                                                           o.Status == WorkOrderStatus.WaitingParts);

            int totalVehiclesCount = vehicles.Count();
            double fleetAvailability = 100;
            if (totalVehiclesCount > 0)
            {
                fleetAvailability = ((double)(totalVehiclesCount - activeMaintenanceCount) / totalVehiclesCount) * 100;
            }

            decimal totalPartsCost = parts.Sum(p => (decimal)p.Quantity * p.UnitPrice);

            double backlogRate = 0;
            if (totalBreakdowns > 0)
            {
                backlogRate = ((double)activeMaintenanceCount / totalBreakdowns) * 100;
            }

            var vehicleMap = vehicles.ToDictionary(v => v.Id, v => v.LicensePlate);
            var freqBreakdowns = orders.GroupBy(o => o.VehicleId)
                .Select(g => new BusBreakdownFrequencyDto
                {
                    VehiclePlate = vehicleMap.TryGetValue(g.Key, out var plate) ? plate : $"Bus #{g.Key}",
                    BreakdownCount = g.Count()
                })
                .OrderByDescending(f => f.BreakdownCount)
                .Take(5)
                .ToList();

            // Repairs by breakdown location ("موقع العطل" from the on-site branch reports).
            // Only work orders that actually have a location recorded are counted, since the
            // central-workshop sheet doesn't have this column at all.
            var locatedOrders = orders.Where(o => !string.IsNullOrWhiteSpace(o.BreakdownLocation)).ToList();
            var totalLocated = locatedOrders.Count;
            var topLocations = locatedOrders
                .GroupBy(o => o.BreakdownLocation!.Trim())
                .Select(g => new BreakdownLocationFrequencyDto
                {
                    Location = g.Key,
                    BreakdownCount = g.Count(),
                    SharePercentage = totalLocated > 0 ? Math.Round((double)g.Count() / totalLocated * 100.0, 1) : 0
                })
                .OrderByDescending(f => f.BreakdownCount)
                .Take(10)
                .ToList();

            return new MaintenanceKpisDto
            {
                MeanTimeToRepairHours = Math.Round(mttr, 2),
                TotalBreakdowns = totalBreakdowns,
                FleetAvailabilityRate = Math.Round(fleetAvailability, 2),
                TotalSparePartsCost = totalPartsCost,
                ActiveBusesRate = Math.Round(100.0 - fleetAvailability, 2), // % in maintenance or active
                MaintenanceBacklogRate = Math.Round(backlogRate, 2),
                TopFrequentBreakdowns = freqBreakdowns,
                TopBreakdownLocations = topLocations
            };
        }

        public async Task<(int SuccessCount, List<string> Errors)> BulkUploadWorkshopLogsAsync(Stream excelStream, string branchName)
        {
            var errors = new List<string>();
            int successCount = 0;

            try
            {
                using var workbook = new XLWorkbook(excelStream);
                var worksheet = workbook.Worksheets.FirstOrDefault();
                if (worksheet == null) return (0, new List<string> { "Excel file is empty." });

                var firstRow = worksheet.Row(1);
                var headers = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                for (int i = 1; i <= firstRow.LastCellUsed().Address.ColumnNumber; i++)
                {
                    var val = firstRow.Cell(i).GetString().Trim();
                    if (!string.IsNullOrEmpty(val))
                    {
                        headers[val] = i;
                    }
                }

                int busCol = FindColumn(headers, "حافلة", "حافغلة", "bus");
                int dateCol = FindColumn(headers, "التاريخ", "date");
                int odometerCol = FindColumn(headers, "عداد", "odometer", "km");
                int descCol = FindColumn(headers, "عطل", "description");
                int timeInCol = FindColumn(headers, "ساعة الدخول", "وقت حدوث العطل", "time in");
                int timeOutCol = FindColumn(headers, "ساعة الخروج", "وقت  الانتهاء", "time out");
                int techCol = FindColumn(headers, "الفنى", "technician");
                int statusCol = FindColumn(headers, "حالة", "status");
                int supervisorCol = FindColumn(headers, "المشرف", "supervisor");
                int remarksCol = FindColumn(headers, "ملاحظات", "ملاحظة", "remarks");
                int partsCol = FindColumn(headers, "القطع", "parts");
                int locationCol = FindColumn(headers, "موقع العطل", "موقع", "location");

                // The bus/plate column is what identifies this as a maintenance workshop log at
                // all - if it can't be found by header keyword, refuse the file instead of
                // silently reading whatever happens to be in column 1 (which produces garbage
                // work orders if the wrong sheet gets uploaded here by mistake).
                if (busCol == -1)
                {
                    errors.Add("This file doesn't look like a Maintenance/Workshop sheet - no column matching \"Bus\" / \"الحافلة\" was found in the header row. Please check you uploaded the right file.");
                    return (0, errors);
                }
                if (dateCol == -1) dateCol = 2;
                if (odometerCol == -1) odometerCol = 3;
                if (descCol == -1) descCol = 4;
                if (timeInCol == -1) timeInCol = 5;
                if (timeOutCol == -1) timeOutCol = 6;
                if (techCol == -1) techCol = 7;
                if (statusCol == -1) statusCol = 8;
                if (supervisorCol == -1) supervisorCol = 9;
                if (remarksCol == -1) remarksCol = 10;

                var rows = worksheet.RowsUsed().Skip(1); // Skip header row
                var dbVehicles = await _vehicleRepository.GetAllAsync();

                foreach (var row in rows)
                {
                    try
                    {
                        var vehicleVal = row.Cell(busCol).GetString().Trim(); // رقم الحافلة
                        if (string.IsNullOrEmpty(vehicleVal)) continue;

                        // Try to find vehicle
                        var vehicle = dbVehicles.FirstOrDefault(v => v.LicensePlate.Contains(vehicleVal) || 
                                                                     v.LicensePlate.Replace(" ", "").Contains(vehicleVal));
                        if (vehicle == null)
                        {
                            // Create temporary vehicle placeholder so the work order seeds successfully
                            vehicle = new Vehicle
                            {
                                LicensePlate = "أ د ي " + vehicleVal,
                                Make = "Yutong",
                                Model = "Placeholder",
                                Year = 2020,
                                Capacity = 49,
                                Status = VehicleStatus.Available
                            };
                            await _vehicleRepository.AddAsync(vehicle);
                            await _vehicleRepository.SaveChangesAsync();
                            // Refresh dbVehicles list
                            dbVehicles = await _vehicleRepository.GetAllAsync();
                        }

                        // Parse date
                        var dateStr = row.Cell(dateCol).GetString();
                        DateTime.TryParse(dateStr, out DateTime date);
                        if (date == default) date = DateTime.UtcNow;

                        // Parse odometer
                        var odoStr = row.Cell(odometerCol).GetString();
                        int.TryParse(odoStr, out int odometer);

                        // Parse description
                        var description = row.Cell(descCol).GetString().Trim();
                        if (string.IsNullOrEmpty(description)) description = "صيانة دورية";

                        // Time In / Out
                        var timeInStr = timeInCol != -1 ? row.Cell(timeInCol).GetString().Trim() : string.Empty;
                        var timeOutStr = timeOutCol != -1 ? row.Cell(timeOutCol).GetString().Trim() : string.Empty;
                        
                        DateTime timeIn = date;
                        if (!string.IsNullOrEmpty(timeInStr) && TimeSpan.TryParse(timeInStr, out TimeSpan tsIn))
                        {
                            timeIn = date.Date + tsIn;
                        }
                        else
                        {
                            timeIn = date.Date.AddHours(9); // Default 9 AM
                        }

                        DateTime? timeOut = null;
                        if (!string.IsNullOrEmpty(timeOutStr) && TimeSpan.TryParse(timeOutStr, out TimeSpan tsOut))
                        {
                            timeOut = date.Date + tsOut;
                        }
                        else
                        {
                            timeOut = timeIn.AddHours(2); // Default 2 hours duration
                        }

                        // Technician & Supervisor
                        var technicianName = techCol != -1 ? row.Cell(techCol).GetString().Trim() : string.Empty;
                        var supervisorName = supervisorCol != -1 ? row.Cell(supervisorCol).GetString().Trim() : string.Empty;

                        // Repair status
                        var statusStr = statusCol != -1 ? row.Cell(statusCol).GetString().Trim() : string.Empty;
                        var status = WorkOrderStatus.Completed;
                        if (statusStr.Contains("معلق") || statusStr.Contains("قطع") || statusStr.Contains("متوقف"))
                        {
                            status = WorkOrderStatus.WaitingParts;
                        }

                        var remarks = remarksCol != -1 ? row.Cell(remarksCol).GetString().Trim() : string.Empty;
                        var breakdownLocation = locationCol != -1 ? row.Cell(locationCol).GetString().Trim() : null;
                        if (string.IsNullOrWhiteSpace(breakdownLocation)) breakdownLocation = null;

                        var order = new MaintenanceWorkOrder
                        {
                            VehicleId = vehicle.Id,
                            Date = date,
                            Odometer = odometer,
                            BreakdownDescription = description,
                            TimeIn = timeIn,
                            TimeOut = timeOut,
                            BranchLocation = branchName,
                            BreakdownLocation = breakdownLocation,
                            SupervisorName = supervisorName,
                            TechnicianName = technicianName,
                            Status = status,
                            Remarks = remarks
                        };

                        await _workOrderRepository.AddAsync(order);
                        await _workOrderRepository.SaveChangesAsync();

                        // Check for spare parts consumed column
                        if (partsCol != -1)
                        {
                            var sparePartsVal = row.Cell(partsCol).GetString().Trim();
                            if (!string.IsNullOrEmpty(sparePartsVal) && sparePartsVal != "لا يوجد" && sparePartsVal != "صرف")
                            {
                                // Create spare part consumption record
                                var part = new SparePartConsumption
                                {
                                    MaintenanceWorkOrderId = order.Id,
                                    PartName = sparePartsVal,
                                    Quantity = 1,
                                    UnitPrice = sparePartsVal.Contains("مرايه") ? 150 : 350 // Mock values
                                };
                                await _partRepository.AddAsync(part);
                                await _partRepository.SaveChangesAsync();
                            }
                        }

                        successCount++;
                    }
                    catch (Exception ex)
                    {
                        errors.Add($"Row {row.RowNumber()}: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                errors.Add($"Error reading file: {ex.Message}");
            }

            return (successCount, errors);
        }

        private int FindColumn(Dictionary<string, int> headers, params string[] searchTerms)
        {
            foreach (var term in searchTerms)
            {
                var match = headers.Keys.FirstOrDefault(k => k.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0);
                if (match != null) return headers[match];
            }
            return -1;
        }
    }
}
