using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NewFeature.Models;
using NewFeature.Services.Repositories;

namespace NewFeature.Services
{
    public class FleetService : IFleetService
    {
        private readonly IRepository<Vehicle> _vehicleRepository;
        private readonly IRepository<Models.Route> _routeRepository;
        private readonly IRepository<Trip> _tripRepository;
        private readonly IRepository<Project> _projectRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public FleetService(
            IRepository<Vehicle> vehicleRepository,
            IRepository<Models.Route> routeRepository,
            IRepository<Trip> tripRepository,
            IRepository<Project> projectRepository,
            UserManager<ApplicationUser> userManager,
            IHttpContextAccessor httpContextAccessor)
        {
            _vehicleRepository = vehicleRepository;
            _routeRepository = routeRepository;
            _tripRepository = tripRepository;
            _projectRepository = projectRepository;
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
        }

        private bool IsArabic()
        {
            var context = _httpContextAccessor.HttpContext;
            if (context != null)
            {
                if (context.Request.Headers.TryGetValue("Accept-Language", out var lang))
                {
                    if (lang.ToString().ToLower().Contains("ar")) return true;
                }
                if (context.Request.Headers.TryGetValue("X-Language", out var xLang))
                {
                    if (xLang.ToString().ToLower().Contains("ar")) return true;
                }
            }
            return false;
        }

        #region Vehicles CRUD
        public async Task<IEnumerable<VehicleDto>> GetAllVehiclesAsync()
        {
            var vehicles = await _vehicleRepository.GetAllAsync();
            return vehicles.OrderByDescending(v => v.Id).Select(v => new VehicleDto
            {
                Id = v.Id,
                LicensePlate = v.LicensePlate,
                Make = v.Make,
                Model = v.Model,
                Year = v.Year,
                Capacity = v.Capacity,
                Status = v.Status
            }).ToList();
        }

        public async Task<VehicleDto?> GetVehicleByIdAsync(int id)
        {
            var v = await _vehicleRepository.GetByIdAsync(id);
            if (v == null) return null;

            return new VehicleDto
            {
                Id = v.Id,
                LicensePlate = v.LicensePlate,
                Make = v.Make,
                Model = v.Model,
                Year = v.Year,
                Capacity = v.Capacity,
                Status = v.Status
            };
        }

        public async Task<VehicleDto> CreateVehicleAsync(VehicleDto dto)
        {
            var vehicle = new Vehicle
            {
                LicensePlate = dto.LicensePlate,
                Make = dto.Make,
                Model = dto.Model,
                Year = dto.Year,
                Capacity = dto.Capacity,
                Status = dto.Status
            };

            await _vehicleRepository.AddAsync(vehicle);
            await _vehicleRepository.SaveChangesAsync();

            dto.Id = vehicle.Id;
            return dto;
        }

        public async Task<bool> UpdateVehicleAsync(VehicleDto dto)
        {
            var vehicle = await _vehicleRepository.GetByIdAsync(dto.Id);
            if (vehicle == null) return false;

            vehicle.LicensePlate = dto.LicensePlate;
            vehicle.Make = dto.Make;
            vehicle.Model = dto.Model;
            vehicle.Year = dto.Year;
            vehicle.Capacity = dto.Capacity;
            vehicle.Status = dto.Status;

            await _vehicleRepository.UpdateAsync(vehicle);
            await _vehicleRepository.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteVehicleAsync(int id)
        {
            var vehicle = await _vehicleRepository.GetByIdAsync(id);
            if (vehicle == null) return false;

            await _vehicleRepository.DeleteAsync(vehicle);
            await _vehicleRepository.SaveChangesAsync();
            return true;
        }
        #endregion

        #region Routes CRUD
        public async Task<IEnumerable<RouteDto>> GetAllRoutesAsync()
        {
            var routes = await _routeRepository.GetAllAsync();
            var isAr = IsArabic();
            return routes.OrderByDescending(r => r.Id).Select(r => new RouteDto
            {
                Id = r.Id,
                NameEn = r.NameEn,
                NameAr = r.NameAr,
                StartLocationEn = r.StartLocationEn,
                StartLocationAr = r.StartLocationAr,
                EndLocationEn = r.EndLocationEn,
                EndLocationAr = r.EndLocationAr,
                DistanceKm = r.DistanceKm,
                Name = isAr ? r.NameAr : r.NameEn,
                StartLocation = isAr ? r.StartLocationAr : r.StartLocationEn,
                EndLocation = isAr ? r.EndLocationAr : r.EndLocationEn
            }).ToList();
        }

        public async Task<RouteDto?> GetRouteByIdAsync(int id)
        {
            var r = await _routeRepository.GetByIdAsync(id);
            if (r == null) return null;

            var isAr = IsArabic();
            return new RouteDto
            {
                Id = r.Id,
                NameEn = r.NameEn,
                NameAr = r.NameAr,
                StartLocationEn = r.StartLocationEn,
                StartLocationAr = r.StartLocationAr,
                EndLocationEn = r.EndLocationEn,
                EndLocationAr = r.EndLocationAr,
                DistanceKm = r.DistanceKm,
                Name = isAr ? r.NameAr : r.NameEn,
                StartLocation = isAr ? r.StartLocationAr : r.StartLocationEn,
                EndLocation = isAr ? r.EndLocationAr : r.EndLocationEn
            };
        }

        public async Task<RouteDto> CreateRouteAsync(RouteDto dto)
        {
            var route = new Models.Route
            {
                NameEn = dto.NameEn,
                NameAr = dto.NameAr,
                StartLocationEn = dto.StartLocationEn,
                StartLocationAr = dto.StartLocationAr,
                EndLocationEn = dto.EndLocationEn,
                EndLocationAr = dto.EndLocationAr,
                DistanceKm = dto.DistanceKm
            };

            await _routeRepository.AddAsync(route);
            await _routeRepository.SaveChangesAsync();

            dto.Id = route.Id;
            return dto;
        }

        public async Task<bool> UpdateRouteAsync(RouteDto dto)
        {
            var route = await _routeRepository.GetByIdAsync(dto.Id);
            if (route == null) return false;

            route.NameEn = dto.NameEn;
            route.NameAr = dto.NameAr;
            route.StartLocationEn = dto.StartLocationEn;
            route.StartLocationAr = dto.StartLocationAr;
            route.EndLocationEn = dto.EndLocationEn;
            route.EndLocationAr = dto.EndLocationAr;
            route.DistanceKm = dto.DistanceKm;

            await _routeRepository.UpdateAsync(route);
            await _routeRepository.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteRouteAsync(int id)
        {
            var route = await _routeRepository.GetByIdAsync(id);
            if (route == null) return false;

            await _routeRepository.DeleteAsync(route);
            await _routeRepository.SaveChangesAsync();
            return true;
        }
        #endregion

        #region Trips CRUD
        public async Task<IEnumerable<TripDto>> GetAllTripsAsync()
        {
            var trips = await _tripRepository.GetAllAsync();
            var vehicles = await _vehicleRepository.GetAllAsync();
            var routes = await _routeRepository.GetAllAsync();
            var projects = await _projectRepository.GetAllAsync();
            var drivers = await _userManager.Users.ToListAsync();
            var isAr = IsArabic();

            var vehicleMap = vehicles.ToDictionary(v => v.Id, v => v.LicensePlate);
            var routeMap = routes.ToDictionary(r => r.Id, r => isAr ? r.NameAr : r.NameEn);
            var projectMap = projects.ToDictionary(p => p.Id, p => isAr ? p.NameAr : p.NameEn);
            var driverMap = drivers.ToDictionary(d => d.Id, d => isAr ? d.FullNameAr : d.FullNameEn);

            return trips.OrderByDescending(t => t.Id).Select(t => new TripDto
            {
                Id = t.Id,
                VehicleId = t.VehicleId,
                VehiclePlate = vehicleMap.TryGetValue(t.VehicleId, out var plate) ? plate : "Unknown",
                RouteId = t.RouteId,
                RouteName = routeMap.TryGetValue(t.RouteId, out var routeName) ? routeName : "Unknown",
                DriverId = t.DriverId,
                DriverName = driverMap.TryGetValue(t.DriverId, out var driverName) ? driverName : "Unknown",
                ProjectId = t.ProjectId,
                ProjectName = t.ProjectId.HasValue && projectMap.TryGetValue(t.ProjectId.Value, out var projName) ? projName : "None",
                ScheduledDeparture = t.ScheduledDeparture,
                ScheduledArrival = t.ScheduledArrival,
                ActualDeparture = t.ActualDeparture,
                ActualArrival = t.ActualArrival,
                Status = t.Status
            }).ToList();
        }

        public async Task<TripDto?> GetTripByIdAsync(int id)
        {
            var t = await _tripRepository.GetByIdAsync(id);
            if (t == null) return null;

            var vehicle = await _vehicleRepository.GetByIdAsync(t.VehicleId);
            var route = await _routeRepository.GetByIdAsync(t.RouteId);
            var project = t.ProjectId.HasValue ? await _projectRepository.GetByIdAsync(t.ProjectId.Value) : null;
            var driver = await _userManager.FindByIdAsync(t.DriverId);
            var isAr = IsArabic();

            return new TripDto
            {
                Id = t.Id,
                VehicleId = t.VehicleId,
                VehiclePlate = vehicle?.LicensePlate ?? "Unknown",
                RouteId = t.RouteId,
                RouteName = route != null ? (isAr ? route.NameAr : route.NameEn) : "Unknown",
                DriverId = t.DriverId,
                DriverName = driver != null ? (isAr ? driver.FullNameAr : driver.FullNameEn) : "Unknown",
                ProjectId = t.ProjectId,
                ProjectName = project != null ? (isAr ? project.NameAr : project.NameEn) : "None",
                ScheduledDeparture = t.ScheduledDeparture,
                ScheduledArrival = t.ScheduledArrival,
                ActualDeparture = t.ActualDeparture,
                ActualArrival = t.ActualArrival,
                Status = t.Status
            };
        }

        public async Task<TripDto> CreateTripAsync(TripDto dto)
        {
            var trip = new Trip
            {
                VehicleId = dto.VehicleId,
                RouteId = dto.RouteId,
                DriverId = dto.DriverId,
                ProjectId = dto.ProjectId,
                ScheduledDeparture = dto.ScheduledDeparture,
                ScheduledArrival = dto.ScheduledArrival,
                ActualDeparture = dto.ActualDeparture,
                ActualArrival = dto.ActualArrival,
                Status = dto.Status
            };

            await _tripRepository.AddAsync(trip);
            await _tripRepository.SaveChangesAsync();

            dto.Id = trip.Id;
            return dto;
        }

        public async Task<bool> UpdateTripAsync(TripDto dto)
        {
            var trip = await _tripRepository.GetByIdAsync(dto.Id);
            if (trip == null) return false;

            trip.VehicleId = dto.VehicleId;
            trip.RouteId = dto.RouteId;
            trip.DriverId = dto.DriverId;
            trip.ProjectId = dto.ProjectId;
            trip.ScheduledDeparture = dto.ScheduledDeparture;
            trip.ScheduledArrival = dto.ScheduledArrival;
            trip.ActualDeparture = dto.ActualDeparture;
            trip.ActualArrival = dto.ActualArrival;
            trip.Status = dto.Status;

            await _tripRepository.UpdateAsync(trip);
            await _tripRepository.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteTripAsync(int id)
        {
            var trip = await _tripRepository.GetByIdAsync(id);
            if (trip == null) return false;

            await _tripRepository.DeleteAsync(trip);
            await _tripRepository.SaveChangesAsync();
            return true;
        }
        #endregion

        #region Bulk Upload
        public async Task<(int SuccessCount, List<string> Errors)> BulkUploadVehiclesAsync(System.IO.Stream excelStream)
        {
            var errors = new List<string>();
            int successCount = 0;

            try
            {
                using var workbook = new ClosedXML.Excel.XLWorkbook(excelStream);
                var worksheet = workbook.Worksheets.FirstOrDefault();
                if (worksheet == null)
                {
                    errors.Add("Excel file is empty.");
                    return (successCount, errors);
                }

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

                int plateCol = FindColumn(headers, "licnse", "اللوحة", "id no");
                int makeCol = FindColumn(headers, "صانع", "make", "brand");
                int modelCol = FindColumn(headers, "bus type", "نوع الحافلة", "name");
                int yearCol = FindColumn(headers, "year", "سنة الصنع", "model");
                int capacityCol = FindColumn(headers, "pass", "سعة", "capacity");

                // Default fallbacks if column not found
                if (plateCol == -1) plateCol = 1;
                if (makeCol == -1) makeCol = 2;
                if (modelCol == -1) modelCol = 3;
                if (yearCol == -1) yearCol = 4;
                if (capacityCol == -1) capacityCol = 5;

                var rows = worksheet.RowsUsed().Skip(1); // Skip header

                foreach (var row in rows)
                {
                    try
                    {
                        var licensePlate = row.Cell(plateCol).GetString().Trim();
                        var make = makeCol != -1 ? row.Cell(makeCol).GetString().Trim() : "Yutong";
                        var model = modelCol != -1 ? row.Cell(modelCol).GetString().Trim() : "City Bus";
                        
                        // Parse year and capacity safely
                        int year = 2026;
                        if (yearCol != -1)
                        {
                            var yearStr = row.Cell(yearCol).GetString();
                            int.TryParse(yearStr, out year);
                        }
                        if (year == 0) year = 2026;

                        decimal capacity = 49;
                        if (capacityCol != -1)
                        {
                            var capStr = row.Cell(capacityCol).GetString();
                            decimal.TryParse(capStr, out capacity);
                        }
                        if (capacity == 0) capacity = 49;

                        if (string.IsNullOrEmpty(licensePlate))
                        {
                            errors.Add($"Row {row.RowNumber()}: License plate is required.");
                            continue;
                        }

                        // Check uniqueness
                        var existing = (await _vehicleRepository.GetAllAsync())
                            .FirstOrDefault(v => v.LicensePlate.Replace(" ", "").Equals(licensePlate.Replace(" ", ""), System.StringComparison.OrdinalIgnoreCase));
                        if (existing != null)
                        {
                            // Vehicle already exists, skip or update
                            continue;
                        }

                        if (string.IsNullOrEmpty(make)) make = "Yutong";
                        if (string.IsNullOrEmpty(model)) model = "City Bus";

                        var vehicle = new Vehicle
                        {
                            LicensePlate = licensePlate,
                            Make = make,
                            Model = model,
                            Year = year,
                            Capacity = capacity,
                            Status = VehicleStatus.Available
                        };

                        await _vehicleRepository.AddAsync(vehicle);
                        await _vehicleRepository.SaveChangesAsync();
                        successCount++;
                    }
                    catch (System.Exception ex)
                    {
                        errors.Add($"Row {row.RowNumber()}: {ex.Message}");
                    }
                }
            }
            catch (System.Exception ex)
            {
                errors.Add($"Error processing Excel file: {ex.Message}");
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

        public async Task<(int SuccessCount, List<string> Errors)> BulkUploadRoutesAsync(System.IO.Stream excelStream)
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
                        var nameEn = row.Cell(1).GetString().Trim();
                        var nameAr = row.Cell(2).GetString().Trim();
                        var startEn = row.Cell(3).GetString().Trim();
                        var startAr = row.Cell(4).GetString().Trim();
                        var endEn = row.Cell(5).GetString().Trim();
                        var endAr = row.Cell(6).GetString().Trim();
                        decimal.TryParse(row.Cell(7).GetString(), out decimal distance);

                        if (string.IsNullOrEmpty(nameEn) || string.IsNullOrEmpty(nameAr))
                        {
                            errors.Add($"Row {row.RowNumber()}: Name is required.");
                            continue;
                        }

                        var route = new Models.Route
                        {
                            NameEn = nameEn,
                            NameAr = nameAr,
                            StartLocationEn = startEn,
                            StartLocationAr = startAr,
                            EndLocationEn = endEn,
                            EndLocationAr = endAr,
                            DistanceKm = distance
                        };

                        await _routeRepository.AddAsync(route);
                        successCount++;
                    }
                    catch (System.Exception ex)
                    {
                        errors.Add($"Row {row.RowNumber()}: {ex.Message}");
                    }
                }

                if (successCount > 0) await _routeRepository.SaveChangesAsync();
            }
            catch (System.Exception ex) { errors.Add(ex.Message); }

            return (successCount, errors);
        }

        public async Task<(int SuccessCount, List<string> Errors)> BulkUploadTripsAsync(System.IO.Stream excelStream)
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
                        int.TryParse(row.Cell(1).GetString(), out int vehicleId);
                        int.TryParse(row.Cell(2).GetString(), out int routeId);
                        var driverId = row.Cell(3).GetString().Trim();
                        int.TryParse(row.Cell(4).GetString(), out int projectId);
                        System.DateTime.TryParse(row.Cell(5).GetString(), out System.DateTime scheduledDeparture);
                        System.DateTime.TryParse(row.Cell(6).GetString(), out System.DateTime scheduledArrival);

                        if (vehicleId == 0 || routeId == 0 || scheduledDeparture == default)
                        {
                            errors.Add($"Row {row.RowNumber()}: Required trip data missing.");
                            continue;
                        }

                        var trip = new Trip
                        {
                            VehicleId = vehicleId,
                            RouteId = routeId,
                            DriverId = driverId,
                            ProjectId = projectId == 0 ? null : projectId,
                            ScheduledDeparture = scheduledDeparture,
                            ScheduledArrival = scheduledArrival,
                            Status = TripStatus.Scheduled
                        };

                        await _tripRepository.AddAsync(trip);
                        successCount++;
                    }
                    catch (System.Exception ex)
                    {
                        errors.Add($"Row {row.RowNumber()}: {ex.Message}");
                    }
                }

                if (successCount > 0) await _tripRepository.SaveChangesAsync();
            }
            catch (System.Exception ex) { errors.Add(ex.Message); }

            return (successCount, errors);
        }
        #endregion
    }
}
