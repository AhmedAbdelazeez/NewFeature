using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ClosedXML.Excel;
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

        #region Daily Plans CRUD
        public async Task<IEnumerable<OperationsDailyPlanDto>> GetAllDailyPlansAsync()
        {
            return await _context.OperationsDailyPlans
                .Select(p => MapToDto(p))
                .ToListAsync();
        }

        public async Task<OperationsDailyPlanDto?> GetDailyPlanByIdAsync(int id)
        {
            var plan = await _context.OperationsDailyPlans.FindAsync(id);
            return plan == null ? null : MapToDto(plan);
        }

        public async Task<OperationsDailyPlanDto> CreateDailyPlanAsync(OperationsDailyPlanDto dto)
        {
            var plan = new OperationsDailyPlan
            {
                Date = dto.Date,
                ScheduledTripsCount = dto.ScheduledTripsCount,
                CompletedTripsCount = dto.CompletedTripsCount,
                FuelEfficiencyIndex = dto.FuelEfficiencyIndex,
                PassengerSatisfactionRate = dto.PassengerSatisfactionRate,
                Status = dto.Status
            };

            _context.OperationsDailyPlans.Add(plan);
            await _context.SaveChangesAsync();
            dto.Id = plan.Id;
            return dto;
        }

        public async Task<bool> UpdateDailyPlanAsync(OperationsDailyPlanDto dto)
        {
            var plan = await _context.OperationsDailyPlans.FindAsync(dto.Id);
            if (plan == null) return false;

            plan.Date = dto.Date;
            plan.ScheduledTripsCount = dto.ScheduledTripsCount;
            plan.CompletedTripsCount = dto.CompletedTripsCount;
            plan.FuelEfficiencyIndex = dto.FuelEfficiencyIndex;
            plan.PassengerSatisfactionRate = dto.PassengerSatisfactionRate;
            plan.Status = dto.Status;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteDailyPlanAsync(int id)
        {
            var plan = await _context.OperationsDailyPlans.FindAsync(id);
            if (plan == null) return false;

            _context.OperationsDailyPlans.Remove(plan);
            await _context.SaveChangesAsync();
            return true;
        }
        #endregion

        #region Incidents CRUD
        public async Task<IEnumerable<OperationsIncidentDto>> GetAllIncidentsAsync()
        {
            return await _context.OperationsIncidents
                .Select(i => MapToDto(i))
                .ToListAsync();
        }

        public async Task<OperationsIncidentDto?> GetIncidentByIdAsync(int id)
        {
            var incident = await _context.OperationsIncidents.FindAsync(id);
            return incident == null ? null : MapToDto(incident);
        }

        public async Task<OperationsIncidentDto> CreateIncidentAsync(OperationsIncidentDto dto)
        {
            var incident = new OperationsIncident
            {
                DescriptionAr = dto.DescriptionAr,
                DescriptionEn = dto.DescriptionEn,
                Severity = dto.Severity,
                ResponseTimeMinutes = dto.ResponseTimeMinutes,
                Date = dto.Date,
                Status = dto.Status
            };

            _context.OperationsIncidents.Add(incident);
            await _context.SaveChangesAsync();
            dto.Id = incident.Id;
            return dto;
        }

        public async Task<bool> UpdateIncidentAsync(OperationsIncidentDto dto)
        {
            var incident = await _context.OperationsIncidents.FindAsync(dto.Id);
            if (incident == null) return false;

            incident.DescriptionAr = dto.DescriptionAr;
            incident.DescriptionEn = dto.DescriptionEn;
            incident.Severity = dto.Severity;
            incident.ResponseTimeMinutes = dto.ResponseTimeMinutes;
            incident.Date = dto.Date;
            incident.Status = dto.Status;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteIncidentAsync(int id)
        {
            var incident = await _context.OperationsIncidents.FindAsync(id);
            if (incident == null) return false;

            _context.OperationsIncidents.Remove(incident);
            await _context.SaveChangesAsync();
            return true;
        }
        #endregion

        #region KPIs Calculation
        // Six basic counts/rates, all computed directly from real Trip records (one row per real bus
        // assignment from the monthly dispatch sheets). Deliberately does not touch
        // OperationsDailyPlans/OperationsIncidents - those tables hold synthetic seed data with no
        // real source file, and mixing them in here would present fake numbers as if they were real.
        public async Task<OperationsKpisDto> GetOperationsKpisAsync()
        {
            var trips = await _context.Trips.AsNoTracking().ToListAsync();

            var totalTrips = trips.Count;
            var cancelledTrips = trips.Count(t => t.Status == TripStatus.Cancelled);
            var cancellationRate = totalTrips > 0 ? (double)cancelledTrips / totalTrips * 100.0 : 0;

            var activeDriversCount = trips.Where(t => !string.IsNullOrEmpty(t.DriverId))
                .Select(t => t.DriverId).Distinct().Count();

            var vehiclesDeployedCount = trips.Select(t => t.VehicleId).Distinct().Count();

            var clientsServedCount = trips.Where(t => !string.IsNullOrWhiteSpace(t.ClientName))
                .Select(t => t.ClientName!.Trim()).Distinct(StringComparer.OrdinalIgnoreCase).Count();

            var distinctDays = trips.Select(t => t.ScheduledDeparture.Date).Distinct().Count();
            var averageTripsPerDay = distinctDays > 0 ? (double)totalTrips / distinctDays : 0;

            var registeredDriversCount = await _context.OfficialDrivers.AsNoTracking().CountAsync();

            var scheduleRequests = await _context.RouteScheduleRequests.AsNoTracking().ToListAsync();
            double? schedulingSuccessRate = scheduleRequests.Count > 0
                ? Math.Round(scheduleRequests.Count(r => r.IsScheduled) / (double)scheduleRequests.Count * 100.0, 1)
                : null;

            return new OperationsKpisDto
            {
                TotalTrips = totalTrips,
                CancelledTrips = cancelledTrips,
                CancellationRatePercent = Math.Round(cancellationRate, 1),
                ActiveDriversCount = activeDriversCount,
                VehiclesDeployedCount = vehiclesDeployedCount,
                ClientsServedCount = clientsServedCount,
                AverageTripsPerDay = Math.Round(averageTripsPerDay, 1),
                RegisteredDriversCount = registeredDriversCount > 0 ? registeredDriversCount : null,
                SchedulingSuccessRatePercent = schedulingSuccessRate
            };
        }

        public async Task<PagedResultDto<OperationsTripDto>> GetTripsPagedAsync(int page, int pageSize, string? search, DateTime? fromDate, DateTime? toDate)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 20;
            if (pageSize > 200) pageSize = 200;

            var query = _context.Trips.AsNoTracking().AsQueryable();
            if (fromDate.HasValue) query = query.Where(t => t.ScheduledDeparture >= fromDate.Value.Date);
            if (toDate.HasValue) query = query.Where(t => t.ScheduledDeparture < toDate.Value.Date.AddDays(1));
            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                query = query.Where(t =>
                    (t.ClientName != null && t.ClientName.Contains(term)) ||
                    (t.BookingReference != null && t.BookingReference.Contains(term)));
            }

            var totalCount = await query.CountAsync();

            // Skip/Take happens before the Includes are materialized, so only this one page (max
            // 200 rows) is ever joined against Vehicles/Routes - not the whole Trips table, which is
            // exactly the pattern that made the dashboard summary query slow before it was fixed.
            var pageTrips = await query
                .OrderByDescending(t => t.ScheduledDeparture).ThenByDescending(t => t.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Include(t => t.Vehicle)
                .Include(t => t.Route)
                .ToListAsync();

            var driverIds = pageTrips.Select(t => t.DriverId).Where(id => !string.IsNullOrEmpty(id)).Distinct().ToList();
            var driverMap = await _context.Users.AsNoTracking()
                .Where(u => driverIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, u => u.FullNameAr ?? u.FullNameEn ?? u.UserName ?? "Unknown");

            var items = pageTrips.Select(t => new OperationsTripDto
            {
                Id = t.Id,
                ScheduledDeparture = t.ScheduledDeparture,
                ClientName = t.ClientName,
                VehiclePlate = t.Vehicle?.LicensePlate ?? "Unknown",
                DriverName = driverMap.GetValueOrDefault(t.DriverId, "Unknown"),
                RouteName = t.Route?.NameAr ?? t.Route?.NameEn ?? "Unknown",
                Status = t.Status.ToString(),
                BookingReference = t.BookingReference
            }).ToList();

            return new PagedResultDto<OperationsTripDto>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }
        #endregion

        #region Bulk Upload
        public async Task<(int SuccessCount, List<string> Errors)> BulkUploadDailyPlansAsync(System.IO.Stream excelStream)
        {
            var errors = new List<string>();
            int successCount = 0;

            try
            {
                using var workbook = new ClosedXML.Excel.XLWorkbook(excelStream);
                var worksheet = workbook.Worksheets.FirstOrDefault();
                if (worksheet == null) return (0, new List<string> { "Excel file is empty." });

                var rows = worksheet.RowsUsed().Skip(1);
                foreach (var row in rows)
                {
                    try
                    {
                        System.DateTime.TryParse(row.Cell(1).GetString(), out System.DateTime date);
                        int.TryParse(row.Cell(2).GetString(), out int scheduled);
                        int.TryParse(row.Cell(3).GetString(), out int completed);
                        double.TryParse(row.Cell(4).GetString(), out double fuel);
                        double.TryParse(row.Cell(5).GetString(), out double sat);
                        var status = row.Cell(6).GetString().Trim();

                        if (date == default)
                        {
                            errors.Add($"Row {row.RowNumber()}: Valid date is required.");
                            continue;
                        }

                        var plan = new OperationsDailyPlan
                        {
                            Date = date,
                            ScheduledTripsCount = scheduled,
                            CompletedTripsCount = completed,
                            FuelEfficiencyIndex = fuel,
                            PassengerSatisfactionRate = sat,
                            Status = string.IsNullOrEmpty(status) ? "Pending" : status
                        };

                        await _context.OperationsDailyPlans.AddAsync(plan);
                        successCount++;
                    }
                    catch (System.Exception ex)
                    {
                        errors.Add($"Row {row.RowNumber()}: {ex.Message}");
                    }
                }

                if (successCount > 0)
                {
                    await _context.SaveChangesAsync();
                    await DbMaintenanceHelper.RefreshStatisticsAsync(_context, _logger, "OperationsDailyPlans");
                }
            }
            catch (System.Exception ex) { errors.Add(ex.Message); }

            return (successCount, errors);
        }

        // Parses the real monthly operations dispatch workbooks (e.g. "تشغيل شهر مايو 2026.xlsx").
        // These workbooks have ONE WORKSHEET PER OPERATING DAY (sheet names like "01-08-2026"), and each
        // sheet has a few title/notice rows before the real header row (columns: العميل/client,
        // اسم السائق/driver, رقم الهوية/national-ID, رقم اللوحة/plate, الوقت/dispatch time,
        // امر الايجار/booking ref, نوع الخدمه/service-route, ملاحظات/notes). Header position and exact
        // wording can drift month to month, so both the header row and the columns are located
        // dynamically, mirroring the approach used in MaintenanceService.BulkUploadWorkshopLogsAsync.
        public async Task<(int SuccessCount, List<string> Errors)> BulkUploadOperationsTripsAsync(Stream excelStream)
        {
            var errors = new List<string>();
            int successCount = 0;

            try
            {
                using var workbook = new XLWorkbook(excelStream);
                if (!workbook.Worksheets.Any()) return (0, new List<string> { "Excel file is empty." });

                var dbVehicles = await _context.Vehicles.ToListAsync();
                var dbRoutes = await _context.Routes.ToListAsync();
                var dbDrivers = await _context.Users.ToListAsync();

                var vehicleCache = new Dictionary<string, Vehicle>(StringComparer.OrdinalIgnoreCase);
                var routeCache = new Dictionary<string, Models.Route>(StringComparer.OrdinalIgnoreCase);
                var driverCache = new Dictionary<string, ApplicationUser>(StringComparer.OrdinalIgnoreCase);

                foreach (var worksheet in workbook.Worksheets)
                {
                    int headerRowNumber = FindHeaderRow(worksheet);
                    if (headerRowNumber == -1) continue; // no recognizable trip table on this tab (e.g. a summary sheet)

                    var headerRow = worksheet.Row(headerRowNumber);
                    var lastUsedCell = headerRow.LastCellUsed();
                    if (lastUsedCell == null) continue;

                    var headers = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                    for (int i = 1; i <= lastUsedCell.Address.ColumnNumber; i++)
                    {
                        var val = headerRow.Cell(i).GetString().Trim();
                        if (!string.IsNullOrEmpty(val) && !headers.ContainsKey(val)) headers[val] = i;
                    }

                    int clientCol = FindColumn(headers, "العميل", "client");
                    int vehicleCol = FindColumn(headers, "رقم اللوحة", "اللوحة", "plate", "رقم الحافلة", "bus", "vehicle");
                    int driverCol = FindColumn(headers, "اسم السائق", "السائق", "driver");
                    int nationalIdCol = FindColumn(headers, "رقم الهوية", "الهويه", "الهوية", "iqama", "national");
                    int timeCol = FindColumn(headers, "الوقت", "time");
                    int bookingRefCol = FindColumn(headers, "امر الايجار", "أمر الايجار", "order");
                    int routeCol = FindColumn(headers, "نوع الخدمه", "نوع الخدمة", "الخط", "المسار", "route", "service");
                    int notesCol = FindColumn(headers, "ملاحظات", "remarks", "notes");

                    // Operating date for this sheet: prefer the worksheet name (e.g. "01-08-2026"),
                    // fall back to scanning the title rows above the header for a dd-mm-yyyy pattern.
                    DateTime sheetDate = ParseSheetDate(worksheet.Name) ?? ParseTitleDate(worksheet, headerRowNumber) ?? DateTime.UtcNow.Date;

                    var rows = worksheet.RowsUsed().Where(r => r.RowNumber() > headerRowNumber);

                    foreach (var row in rows)
                    {
                        try
                        {
                            var vehicleRaw = vehicleCol != -1 ? row.Cell(vehicleCol).GetString().Trim() : string.Empty;
                            var driverRaw = driverCol != -1 ? row.Cell(driverCol).GetString().Trim() : string.Empty;
                            var routeRaw = routeCol != -1 ? row.Cell(routeCol).GetString().Trim() : string.Empty;
                            var clientRaw = clientCol != -1 ? row.Cell(clientCol).GetString().Trim() : string.Empty;

                            // Skip blank / section-separator rows
                            if (string.IsNullOrEmpty(vehicleRaw) && string.IsNullOrEmpty(driverRaw) && string.IsNullOrEmpty(clientRaw))
                                continue;

                            if (string.IsNullOrEmpty(vehicleRaw))
                            {
                                errors.Add($"[{worksheet.Name}] Row {row.RowNumber()}: Bus/vehicle plate missing.");
                                continue;
                            }

                            // Resolve / create Vehicle. Match by normalized plate first, then by the
                            // digits-only sequence (Arabic-letter and Latin-transliterated plates for the
                            // same bus in this fleet's data share the same digit sequence).
                            var plateKey = NormalizePlate(vehicleRaw);
                            if (!vehicleCache.TryGetValue(plateKey, out var vehicle))
                            {
                                var digitsOnly = DigitsOnly(vehicleRaw);
                                vehicle = dbVehicles.FirstOrDefault(v => NormalizePlate(v.LicensePlate) == plateKey)
                                    ?? (digitsOnly.Length >= 3 ? dbVehicles.FirstOrDefault(v => DigitsOnly(v.LicensePlate) == digitsOnly) : null);

                                if (vehicle == null)
                                {
                                    vehicle = new Vehicle
                                    {
                                        LicensePlate = vehicleRaw,
                                        Make = "Yutong",
                                        Model = "Unknown",
                                        Year = DateTime.UtcNow.Year,
                                        Capacity = 49,
                                        Status = VehicleStatus.Active
                                    };
                                    _context.Vehicles.Add(vehicle);
                                    await _context.SaveChangesAsync();
                                    dbVehicles.Add(vehicle);
                                }
                                vehicleCache[plateKey] = vehicle;
                            }

                            // Resolve / create Route from the service-type / line description column
                            var routeKey = string.IsNullOrEmpty(routeRaw) ? "غير محدد" : routeRaw;
                            if (!routeCache.TryGetValue(routeKey, out var route))
                            {
                                route = dbRoutes.FirstOrDefault(r => r.NameAr.Equals(routeKey, StringComparison.OrdinalIgnoreCase));
                                if (route == null)
                                {
                                    route = new Models.Route
                                    {
                                        NameAr = routeKey,
                                        NameEn = routeKey,
                                        StartLocationAr = "-",
                                        StartLocationEn = "-",
                                        EndLocationAr = "-",
                                        EndLocationEn = "-",
                                        DistanceKm = 0.1m
                                    };
                                    _context.Routes.Add(route);
                                    await _context.SaveChangesAsync();
                                    dbRoutes.Add(route);
                                }
                                routeCache[routeKey] = route;
                            }

                            // Resolve / create Driver. The national-ID/Iqama column is the reliable
                            // de-dup key (matches "IqamaNoForBank" in the official drivers roster);
                            // falls back to name matching when the ID column isn't present.
                            var nationalIdRaw = nationalIdCol != -1 ? DigitsOnly(row.Cell(nationalIdCol).GetString()) : string.Empty;
                            var driverKey = !string.IsNullOrEmpty(nationalIdRaw) ? $"ID:{nationalIdRaw}"
                                : (!string.IsNullOrEmpty(driverRaw) ? $"NAME:{driverRaw}" : "UNKNOWN");

                            if (!driverCache.TryGetValue(driverKey, out var driver))
                            {
                                driver = !string.IsNullOrEmpty(nationalIdRaw)
                                    ? dbDrivers.FirstOrDefault(d => d.UserName == $"drv_{nationalIdRaw}")
                                    : null;
                                if (driver == null && !string.IsNullOrEmpty(driverRaw))
                                {
                                    driver = dbDrivers.FirstOrDefault(d =>
                                        d.FullNameAr.Equals(driverRaw, StringComparison.OrdinalIgnoreCase) ||
                                        d.FullNameEn.Equals(driverRaw, StringComparison.OrdinalIgnoreCase));
                                }

                                if (driver == null)
                                {
                                    var seed = !string.IsNullOrEmpty(nationalIdRaw) ? nationalIdRaw : Guid.NewGuid().ToString("N").Substring(0, 10);
                                    driver = new ApplicationUser
                                    {
                                        UserName = $"drv_{seed}",
                                        NormalizedUserName = $"DRV_{seed}".ToUpperInvariant(),
                                        FullNameAr = string.IsNullOrEmpty(driverRaw) ? "غير محدد" : driverRaw,
                                        FullNameEn = string.IsNullOrEmpty(driverRaw) ? "Unknown" : driverRaw,
                                        IsActive = true,
                                        SecurityStamp = Guid.NewGuid().ToString(),
                                        ConcurrencyStamp = Guid.NewGuid().ToString(),
                                        EmailConfirmed = false
                                    };
                                    _context.Users.Add(driver);
                                    await _context.SaveChangesAsync();
                                    dbDrivers.Add(driver);
                                }
                                driverCache[driverKey] = driver;
                            }

                            // Dispatch time -> Scheduled/Actual Departure. This sheet layout has no
                            // separate arrival time, so Arrival defaults to Departure + 1h.
                            var timeStr = timeCol != -1 ? row.Cell(timeCol).GetString().Trim() : string.Empty;
                            DateTime scheduledDeparture = sheetDate.Date.AddHours(6);
                            if (!string.IsNullOrEmpty(timeStr) && TimeSpan.TryParse(timeStr, out var depTs))
                                scheduledDeparture = sheetDate.Date + depTs;
                            var scheduledArrival = scheduledDeparture.AddHours(1);

                            var notesRaw = notesCol != -1 ? row.Cell(notesCol).GetString().Trim() : string.Empty;
                            var bookingRefRaw = bookingRefCol != -1 ? row.Cell(bookingRefCol).GetString().Trim() : string.Empty;

                            // Cancellation remarks show up as free text in the notes column
                            // (e.g. "كنسل من قبل العميل" = cancelled by client).
                            var isCancelled = notesRaw.Contains("كنسل") || notesRaw.Contains("ملغ") ||
                                               notesRaw.IndexOf("cancel", StringComparison.OrdinalIgnoreCase) >= 0;

                            var trip = new Trip
                            {
                                VehicleId = vehicle.Id,
                                RouteId = route.Id,
                                DriverId = driver.Id,
                                ScheduledDeparture = scheduledDeparture,
                                ScheduledArrival = scheduledArrival,
                                ActualDeparture = isCancelled ? null : scheduledDeparture,
                                ActualArrival = isCancelled ? null : scheduledArrival,
                                Status = isCancelled ? TripStatus.Cancelled : TripStatus.Completed,
                                ClientName = string.IsNullOrEmpty(clientRaw) ? null : clientRaw,
                                BookingReference = string.IsNullOrEmpty(bookingRefRaw) ? null : bookingRefRaw
                            };

                            _context.Trips.Add(trip);
                            successCount++;
                        }
                        catch (Exception ex)
                        {
                            errors.Add($"[{worksheet.Name}] Row {row.RowNumber()}: {ex.Message}");
                        }
                    }
                }

                if (successCount > 0)
                {
                    await _context.SaveChangesAsync();
                    // This importer also opportunistically creates Vehicle/Route/driver rows inline
                    // (see the row loop above), so all four tables need fresh statistics, not just Trips.
                    await DbMaintenanceHelper.RefreshStatisticsAsync(_context, _logger, "Trips", "Vehicles", "Routes", "AspNetUsers");
                }
            }
            catch (Exception ex)
            {
                errors.Add($"Error processing Excel file: {ex.Message}");
            }

            return (successCount, errors);
        }

        // Replaces the official-drivers compliance roster snapshot (اسطول الحافلات - السائقين
        // الرسميين.xlsx, sheet "السائقين الرسميين") - a new upload always supersedes the previous one.
        public async Task<(int SuccessCount, List<string> Errors)> BulkUploadOfficialDriversAsync(Stream excelStream)
        {
            var errors = new List<string>();
            var newRows = new List<OfficialDriver>();

            try
            {
                using var workbook = new XLWorkbook(excelStream);
                var ws = workbook.Worksheets.FirstOrDefault(w => FindHeaderRowByTerms(w, "Employee", "Arabic Name", "IqamaNoForBank") > 0)
                          ?? workbook.Worksheets.FirstOrDefault();
                if (ws == null) return (0, new List<string> { "Excel file has no worksheets." });

                int headerRow = FindHeaderRowByTerms(ws, "Employee", "Arabic Name", "IqamaNoForBank");
                if (headerRow == -1) return (0, new List<string> { "Could not locate the header row (expected columns like 'Employee' / 'Arabic Name')." });

                var headers = BuildHeaderMap(ws, headerRow);
                int employeeCol = FindColumn(headers, "Employee");
                int iqamaCol = FindColumn(headers, "IqamaNoForBank", "iqama");
                int arabicNameCol = FindColumn(headers, "Arabic Name", "الاسم");
                int englishNameCol = FindColumn(headers, "Employee Name", "English Name");
                int nationalityCol = FindColumn(headers, "Nationality");
                int licenseExpiryCol = FindColumn(headers, "تاريخ انتهاء الرخصة", "license");
                int notesCol = FindColumn(headers, "عمود2", "ملاحظات", "notes");

                if (arabicNameCol == -1) return (0, new List<string> { "Required column (Arabic Name) was not found." });

                var lastRow = ws.LastRowUsed()?.RowNumber() ?? headerRow;
                for (int r = headerRow + 1; r <= lastRow; r++)
                {
                    try
                    {
                        var arabicName = CellText(ws, r, arabicNameCol);
                        var employeeCode = CellText(ws, r, employeeCol);
                        if (arabicName.Length == 0 && employeeCode.Length == 0) continue; // blank padding row

                        if (arabicName.Length == 0)
                        {
                            errors.Add($"Row {r}: missing driver name - row skipped.");
                            continue;
                        }

                        DateTime? licenseExpiry = null;
                        var licenseText = licenseExpiryCol != -1 ? CellText(ws, r, licenseExpiryCol) : string.Empty;
                        if (!string.IsNullOrEmpty(licenseText) && DateTime.TryParse(licenseText, out var parsedDate))
                            licenseExpiry = parsedDate;

                        newRows.Add(new OfficialDriver
                        {
                            EmployeeCode = employeeCode.Length > 0 ? employeeCode : null,
                            IqamaNumber = iqamaCol != -1 ? (CellText(ws, r, iqamaCol) is var iq && iq.Length > 0 ? iq : null) : null,
                            ArabicName = arabicName,
                            EnglishName = englishNameCol != -1 ? (CellText(ws, r, englishNameCol) is var en && en.Length > 0 ? en : null) : null,
                            Nationality = nationalityCol != -1 ? (CellText(ws, r, nationalityCol) is var nat && nat.Length > 0 ? nat : null) : null,
                            LicenseExpiryDate = licenseExpiry,
                            Notes = notesCol != -1 ? (CellText(ws, r, notesCol) is var nt && nt.Length > 0 ? nt : null) : null
                        });
                    }
                    catch (Exception ex)
                    {
                        errors.Add($"Row {r}: {ex.Message}");
                    }
                }

                if (newRows.Count == 0)
                    return (0, errors.Count > 0 ? errors : new List<string> { "No valid driver rows found in the file." });

                // Point-in-time roster snapshot - a new upload replaces the previous one entirely.
                _context.OfficialDrivers.RemoveRange(_context.OfficialDrivers);
                _context.OfficialDrivers.AddRange(newRows);
                await _context.SaveChangesAsync();
                await DbMaintenanceHelper.RefreshStatisticsAsync(_context, _logger, "OfficialDrivers");
            }
            catch (Exception ex)
            {
                errors.Add($"Error processing Excel file: {ex.Message}");
                return (0, errors);
            }

            return (newRows.Count, errors);
        }

        // Replaces the route-scheduling requests snapshot (جدولة الخطوط.xlsx).
        public async Task<(int SuccessCount, List<string> Errors)> BulkUploadRouteSchedulesAsync(Stream excelStream)
        {
            var errors = new List<string>();
            var newRows = new List<RouteScheduleRequest>();

            try
            {
                using var workbook = new XLWorkbook(excelStream);
                var ws = workbook.Worksheets.FirstOrDefault(w => FindHeaderRowByTerms(w, "تاريخ التنفيذ", "رقم أمر الايجار", "الجدولة") > 0)
                          ?? workbook.Worksheets.FirstOrDefault();
                if (ws == null) return (0, new List<string> { "Excel file has no worksheets." });

                int headerRow = FindHeaderRowByTerms(ws, "تاريخ التنفيذ", "رقم أمر الايجار", "الجدولة");
                if (headerRow == -1) return (0, new List<string> { "Could not locate the header row (expected columns like 'تاريخ التنفيذ' / 'الجدولة')." });

                var headers = BuildHeaderMap(ws, headerRow);
                int dateCol = FindColumn(headers, "تاريخ التنفيذ");
                int orderCol = FindColumn(headers, "رقم أمر الايجار", "امر الايجار");
                int clientCol = FindColumn(headers, "أسم العميل", "اسم العميل");
                int serviceCol = FindColumn(headers, "اسم الصنف");
                int locationCol = FindColumn(headers, "مكان التشغيل");
                int scheduleCol = FindColumn(headers, "الجدولة");

                if (scheduleCol == -1) return (0, new List<string> { "Required column (الجدولة) was not found." });

                var lastRow = ws.LastRowUsed()?.RowNumber() ?? headerRow;
                for (int r = headerRow + 1; r <= lastRow; r++)
                {
                    try
                    {
                        var scheduleRaw = CellText(ws, r, scheduleCol);
                        var orderRaw = orderCol != -1 ? CellText(ws, r, orderCol) : string.Empty;
                        if (scheduleRaw.Length == 0 && orderRaw.Length == 0) continue; // blank padding row

                        DateTime? execDate = null;
                        var dateText = dateCol != -1 ? CellText(ws, r, dateCol) : string.Empty;
                        if (!string.IsNullOrEmpty(dateText) && DateTime.TryParse(dateText, out var parsedDate))
                            execDate = parsedDate;

                        // "الجدولة" holds a schedulable count/"1" when scheduled, or the literal text
                        // "غير مجدول" when not - anything that isn't that phrase and isn't blank counts
                        // as scheduled.
                        var isScheduled = scheduleRaw.Length > 0 && !scheduleRaw.Contains("غير مجدول");

                        newRows.Add(new RouteScheduleRequest
                        {
                            ExecutionDate = execDate,
                            RentalOrderNumber = orderRaw.Length > 0 ? orderRaw : null,
                            ClientName = clientCol != -1 ? (CellText(ws, r, clientCol) is var cn && cn.Length > 0 ? cn : null) : null,
                            ServiceName = serviceCol != -1 ? (CellText(ws, r, serviceCol) is var sn && sn.Length > 0 ? sn : null) : null,
                            Location = locationCol != -1 ? (CellText(ws, r, locationCol) is var loc && loc.Length > 0 ? loc : null) : null,
                            ScheduleStatusRaw = scheduleRaw.Length > 0 ? scheduleRaw : null,
                            IsScheduled = isScheduled
                        });
                    }
                    catch (Exception ex)
                    {
                        errors.Add($"Row {r}: {ex.Message}");
                    }
                }

                if (newRows.Count == 0)
                    return (0, errors.Count > 0 ? errors : new List<string> { "No valid scheduling rows found in the file." });

                _context.RouteScheduleRequests.RemoveRange(_context.RouteScheduleRequests);
                _context.RouteScheduleRequests.AddRange(newRows);
                await _context.SaveChangesAsync();
                await DbMaintenanceHelper.RefreshStatisticsAsync(_context, _logger, "RouteScheduleRequests");
            }
            catch (Exception ex)
            {
                errors.Add($"Error processing Excel file: {ex.Message}");
                return (0, errors);
            }

            return (newRows.Count, errors);
        }

        // Scans the first few rows of a worksheet for the real header row (the real workbooks have
        // title/notice rows before it), looking for a row containing at least 2 recognizable column names.
        private static int FindHeaderRow(IXLWorksheet worksheet)
        {
            var lastRow = Math.Min(worksheet.LastRowUsed()?.RowNumber() ?? 1, 10);
            for (int r = 1; r <= lastRow; r++)
            {
                var row = worksheet.Row(r);
                var lastCell = row.LastCellUsed();
                if (lastCell == null) continue;

                int hits = 0;
                for (int c = 1; c <= lastCell.Address.ColumnNumber; c++)
                {
                    var val = row.Cell(c).GetString();
                    if (val.IndexOf("السائق", StringComparison.OrdinalIgnoreCase) >= 0 ||
                        val.IndexOf("اللوحة", StringComparison.OrdinalIgnoreCase) >= 0 ||
                        val.IndexOf("الوقت", StringComparison.OrdinalIgnoreCase) >= 0 ||
                        val.IndexOf("العميل", StringComparison.OrdinalIgnoreCase) >= 0 ||
                        val.IndexOf("driver", StringComparison.OrdinalIgnoreCase) >= 0 ||
                        val.IndexOf("plate", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        hits++;
                    }
                }
                if (hits >= 2) return r;
            }
            return -1;
        }

        private static DateTime? ParseSheetDate(string sheetName)
        {
            var cleaned = (sheetName ?? string.Empty).Trim();
            if (DateTime.TryParseExact(cleaned, new[] { "dd-MM-yyyy", "d-M-yyyy", "dd/MM/yyyy" },
                    System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var d))
                return d;
            if (DateTime.TryParse(cleaned, out d)) return d;
            return null;
        }

        private static DateTime? ParseTitleDate(IXLWorksheet worksheet, int headerRowNumber)
        {
            for (int r = 1; r < headerRowNumber; r++)
            {
                var text = worksheet.Row(r).CellsUsed().Select(c => c.GetString()).FirstOrDefault(t => !string.IsNullOrWhiteSpace(t));
                if (string.IsNullOrEmpty(text)) continue;

                var match = System.Text.RegularExpressions.Regex.Match(text, @"(\d{1,2})\s*-\s*(\d{1,2})\s*-\s*(\d{4})");
                if (match.Success &&
                    int.TryParse(match.Groups[1].Value, out int day) &&
                    int.TryParse(match.Groups[2].Value, out int month) &&
                    int.TryParse(match.Groups[3].Value, out int year))
                {
                    try { return new DateTime(year, month, day); } catch { /* ignore invalid date text */ }
                }
            }
            return null;
        }

        private static string NormalizePlate(string plate) =>
            new string((plate ?? string.Empty).Where(ch => !char.IsWhiteSpace(ch) && ch != '-').ToArray()).ToUpperInvariant();

        private static string DigitsOnly(string value) =>
            new string((value ?? string.Empty).Where(char.IsDigit).ToArray());

        private int FindColumn(Dictionary<string, int> headers, params string[] searchTerms)
        {
            foreach (var term in searchTerms)
            {
                var match = headers.Keys.FirstOrDefault(k => k.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0);
                if (match != null) return headers[match];
            }
            return -1;
        }

        // Generic header-row finder used by the roster/scheduling snapshot uploaders (unlike
        // FindHeaderRow(IXLWorksheet) above, which is hard-coded to the trip-dispatch column names).
        private static int FindHeaderRowByTerms(IXLWorksheet ws, params string[] keywords)
        {
            var lastRow = Math.Min(ws.LastRowUsed()?.RowNumber() ?? 0, 15);
            for (int r = 1; r <= lastRow; r++)
            {
                var row = ws.Row(r);
                var lastCell = row.LastCellUsed();
                if (lastCell == null) continue;

                int hits = 0;
                for (int c = 1; c <= lastCell.Address.ColumnNumber; c++)
                {
                    var text = row.Cell(c).GetString().Trim();
                    if (text.Length == 0) continue;
                    if (keywords.Any(kw => text.Contains(kw, StringComparison.OrdinalIgnoreCase))) hits++;
                }
                if (hits >= 2) return r;
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

        private static string CellText(IXLWorksheet ws, int row, int col) =>
            col == -1 ? string.Empty : ws.Cell(row, col).GetString().Trim();
        #endregion

        #region Mappers
        private static OperationsDailyPlanDto MapToDto(OperationsDailyPlan p) => new()
        {
            Id = p.Id,
            Date = p.Date,
            ScheduledTripsCount = p.ScheduledTripsCount,
            CompletedTripsCount = p.CompletedTripsCount,
            FuelEfficiencyIndex = p.FuelEfficiencyIndex,
            PassengerSatisfactionRate = p.PassengerSatisfactionRate,
            Status = p.Status
        };

        private static OperationsIncidentDto MapToDto(OperationsIncident i) => new()
        {
            Id = i.Id,
            DescriptionAr = i.DescriptionAr,
            DescriptionEn = i.DescriptionEn,
            Severity = i.Severity,
            ResponseTimeMinutes = i.ResponseTimeMinutes,
            Date = i.Date,
            Status = i.Status
        };
        #endregion
    }
}
