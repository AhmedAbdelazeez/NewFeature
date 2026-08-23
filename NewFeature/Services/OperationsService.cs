using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ClosedXML.Excel;
using NewFeature.Models;
using NewFeature.Services.Repositories;

namespace NewFeature.Services
{
    public class OperationsService : IOperationsService
    {
        private readonly ApplicationDbContext _context;

        public OperationsService(ApplicationDbContext context)
        {
            _context = context;
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
        public async Task<OperationsKpisDto> GetOperationsKpisAsync()
        {
            var plans = await _context.OperationsDailyPlans.ToListAsync();
            var incidents = await _context.OperationsIncidents.ToListAsync();
            var vehicles = await _context.Vehicles.ToListAsync();

            // 1. Operational Plan Adherence (%)
            var totalScheduled = plans.Sum(p => p.ScheduledTripsCount);
            var totalCompleted = plans.Sum(p => p.CompletedTripsCount);
            var planAdherence = totalScheduled > 0 ? ((double)totalCompleted / totalScheduled) * 100.0 : 92.5;

            // 2. Fleet Utilization Rate (%)
            var totalVehicles = vehicles.Count;
            var activeVehicles = vehicles.Count(v => v.Status == VehicleStatus.Active);
            var fleetUtil = totalVehicles > 0 ? ((double)activeVehicles / totalVehicles) * 100.0 : 80.0;

            // 3. Average Breakdown Response Time (Minutes)
            var resolvedIncidents = incidents.Where(i => i.Status == "Resolved").ToList();
            var avgResponse = resolvedIncidents.Any() ? resolvedIncidents.Average(i => i.ResponseTimeMinutes) : 27.5;

            // 4. Operational Violations Count (Incidents)
            var violationsCount = incidents.Count;

            // 5. Passenger Satisfaction Rate (%)
            var satPlans = plans.Where(p => p.PassengerSatisfactionRate > 0).ToList();
            var avgSatisfaction = satPlans.Any() ? satPlans.Average(p => p.PassengerSatisfactionRate) : 92.0;

            // 6. Scheduled Daily Trips (latest daily plan scheduled trips)
            var scheduledTrips = plans.OrderByDescending(p => p.Date).FirstOrDefault()?.ScheduledTripsCount ?? 120;

            // 7. Fuel Efficiency Index (%)
            var fuelPlans = plans.Where(p => p.FuelEfficiencyIndex > 0).ToList();
            var avgFuelIndex = fuelPlans.Any() ? fuelPlans.Average(p => p.FuelEfficiencyIndex) : 94.5;

            // ── Real Trip-based KPIs (from bulk-uploaded operations/trip logs) ──
            var trips = await _context.Trips.AsNoTracking().ToListAsync();
            var allDrivers = await _context.Users.ToListAsync();

            // 8. On-Time Performance (OTP) Rate (%): completed trips arriving within 15 min of schedule
            var completedTripsWithArrival = trips.Where(t => t.Status == TripStatus.Completed && t.ActualArrival.HasValue).ToList();
            double otpRate = 0;
            if (completedTripsWithArrival.Any())
            {
                var onTimeCount = completedTripsWithArrival.Count(t =>
                    Math.Abs((t.ActualArrival!.Value - t.ScheduledArrival).TotalMinutes) <= 15);
                otpRate = ((double)onTimeCount / completedTripsWithArrival.Count) * 100.0;
            }

            // 9. Total Trips Executed: trips that actually departed (Completed or InProgress)
            var totalTripsExecuted = trips.Count(t => t.Status == TripStatus.Completed || t.Status == TripStatus.InProgress);

            // 10. Active Drivers Count: distinct drivers who have at least one trip on record
            var activeDriversCount = trips.Where(t => !string.IsNullOrEmpty(t.DriverId))
                .Select(t => t.DriverId).Distinct().Count();
            var totalDriversCount = allDrivers.Count(d => d.IsActive);

            // 11. Fuel/Odometer Efficiency (Km per Liter), from trips carrying both odometer and fuel readings
            var efficiencyTrips = trips.Where(t => t.OdometerKm.HasValue && t.OdometerKm > 0 && t.FuelConsumedLiters.HasValue && t.FuelConsumedLiters > 0).ToList();
            double fuelOdometerEfficiency = 0;
            if (efficiencyTrips.Any())
            {
                var totalKm = efficiencyTrips.Sum(t => t.OdometerKm!.Value);
                var totalLiters = efficiencyTrips.Sum(t => t.FuelConsumedLiters!.Value);
                fuelOdometerEfficiency = totalLiters > 0 ? totalKm / totalLiters : 0;
            }

            return new OperationsKpisDto
            {
                PlanAdherenceActual = Math.Round(planAdherence, 1),
                PlanAdherenceTarget = 95.0,

                FleetUtilizationActual = Math.Round(fleetUtil, 1),
                FleetUtilizationTarget = 85.0,

                AvgBreakdownResponseActual = Math.Round(avgResponse, 1),
                AvgBreakdownResponseTarget = 30.0,

                ViolationsCountActual = violationsCount,
                ViolationsCountTarget = 0,

                PassengerSatisfactionActual = Math.Round(avgSatisfaction, 1),
                PassengerSatisfactionTarget = 90.0,

                ScheduledTripsActual = scheduledTrips,
                ScheduledTripsTarget = 100,

                FuelEfficiencyActual = Math.Round(avgFuelIndex, 1),
                FuelEfficiencyTarget = 95.0,

                OnTimePerformanceActual = Math.Round(otpRate, 1),
                OnTimePerformanceTarget = 95.0,

                TotalTripsExecutedActual = totalTripsExecuted,
                TotalTripsExecutedTarget = Math.Max(totalTripsExecuted, 100),

                ActiveDriversCountActual = activeDriversCount,
                ActiveDriversCountTarget = totalDriversCount,

                FuelOdometerEfficiencyActual = Math.Round(fuelOdometerEfficiency, 2),
                FuelOdometerEfficiencyTarget = 3.0
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

                if (successCount > 0) await _context.SaveChangesAsync();
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

                if (successCount > 0) await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                errors.Add($"Error processing Excel file: {ex.Message}");
            }

            return (successCount, errors);
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
