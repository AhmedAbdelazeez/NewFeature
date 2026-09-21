using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NewFeature.Models;
using NewFeature.Services.ExcelImport;
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
        private readonly ILogger<FleetService> _logger;
        private readonly ApplicationDbContext _context;

        public FleetService(
            IRepository<Vehicle> vehicleRepository,
            IRepository<Models.Route> routeRepository,
            IRepository<Trip> tripRepository,
            IRepository<Project> projectRepository,
            UserManager<ApplicationUser> userManager,
            IHttpContextAccessor httpContextAccessor,
            ILogger<FleetService> logger,
            ApplicationDbContext context)
        {
            _vehicleRepository = vehicleRepository;
            _routeRepository = routeRepository;
            _tripRepository = tripRepository;
            _projectRepository = projectRepository;
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
            _context = context;
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
        private static VehicleDto MapVehicleToDto(Vehicle v) => new()
        {
            Id = v.Id,
            BusNumber = v.BusNumber,
            LicensePlate = v.LicensePlate,
            Make = v.Make,
            Model = v.Model,
            Year = v.Year,
            Capacity = v.Capacity,
            Status = v.Status,
            Mileage = v.Mileage,
            ChassisNumber = v.ChassisNumber,
            BusTypeCode = v.BusTypeCode,
            HasAirConditioning = v.HasAirConditioning,
            MaxKilometers = v.MaxKilometers,
            IsInStorage = v.IsInStorage,
            ResponsibilityCode = v.ResponsibilityCode,
            ResponsibleEmployeeName = v.ResponsibleEmployeeName
        };

        public async Task<IEnumerable<VehicleDto>> GetAllVehiclesAsync()
        {
            var vehicles = await _vehicleRepository.GetAllAsync();
            return vehicles.OrderByDescending(v => v.Id).Select(MapVehicleToDto).ToList();
        }

        // Backs the Vehicles management page's table. The underlying IRepository<Vehicle> only
        // exposes GetAllAsync() (no IQueryable), so - matching how this method's caller was going
        // to use it anyway - we materialize once and page/search in memory; at ~700 rows that's
        // trivial and keeps this change scoped to FleetService instead of touching the shared
        // repository abstraction other departments also depend on.
        public async Task<PagedResultDto<VehicleDto>> GetVehiclesPagedAsync(int page, int pageSize, string? search)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 20;
            if (pageSize > 200) pageSize = 200;

            var vehicles = (await _vehicleRepository.GetAllAsync()).AsEnumerable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                vehicles = vehicles.Where(v =>
                    Contains(v.LicensePlate, term) ||
                    Contains(v.BusNumber, term) ||
                    Contains(v.Make, term) ||
                    Contains(v.Model, term) ||
                    Contains(v.ChassisNumber, term) ||
                    Contains(v.BusTypeCode, term) ||
                    Contains(v.ResponsibilityCode, term) ||
                    Contains(v.ResponsibleEmployeeName, term) ||
                    Contains(v.Status.ToString(), term));
            }

            var ordered = vehicles.OrderByDescending(v => v.Id).ToList();
            var totalCount = ordered.Count;

            var items = ordered
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(MapVehicleToDto)
                .ToList();

            return new PagedResultDto<VehicleDto>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        private static bool Contains(string? haystack, string term) =>
            !string.IsNullOrEmpty(haystack) && haystack.IndexOf(term, System.StringComparison.OrdinalIgnoreCase) >= 0;

        public async Task<VehicleDto?> GetVehicleByIdAsync(int id)
        {
            var v = await _vehicleRepository.GetByIdAsync(id);
            if (v == null) return null;

            return MapVehicleToDto(v);
        }

        public async Task<VehicleDto> CreateVehicleAsync(VehicleDto dto)
        {
            var vehicle = new Vehicle
            {
                BusNumber = dto.BusNumber,
                LicensePlate = dto.LicensePlate,
                Make = dto.Make,
                Model = dto.Model,
                Year = dto.Year,
                Capacity = dto.Capacity,
                Status = dto.Status,
                Mileage = dto.Mileage,
                ChassisNumber = dto.ChassisNumber,
                BusTypeCode = dto.BusTypeCode,
                HasAirConditioning = dto.HasAirConditioning,
                MaxKilometers = dto.MaxKilometers,
                IsInStorage = dto.IsInStorage,
                ResponsibilityCode = dto.ResponsibilityCode,
                ResponsibleEmployeeName = dto.ResponsibleEmployeeName
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

            vehicle.BusNumber = dto.BusNumber;
            vehicle.LicensePlate = dto.LicensePlate;
            vehicle.Make = dto.Make;
            vehicle.Model = dto.Model;
            vehicle.Year = dto.Year;
            vehicle.Capacity = dto.Capacity;
            vehicle.Status = dto.Status;
            vehicle.Mileage = dto.Mileage;
            vehicle.ChassisNumber = dto.ChassisNumber;
            vehicle.BusTypeCode = dto.BusTypeCode;
            vehicle.HasAirConditioning = dto.HasAirConditioning;
            vehicle.MaxKilometers = dto.MaxKilometers;
            vehicle.IsInStorage = dto.IsInStorage;
            vehicle.ResponsibilityCode = dto.ResponsibilityCode;
            vehicle.ResponsibleEmployeeName = dto.ResponsibleEmployeeName;

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

        // Backs the Routes management page's table. Materializes once via the existing
        // repository call, then filters/orders/pages in memory - matching
        // GetVehiclesPagedAsync's house style above.
        public async Task<PagedResultDto<RouteDto>> GetRoutesPagedAsync(int page, int pageSize, string? search)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 20;
            if (pageSize > 200) pageSize = 200;

            var routes = (await _routeRepository.GetAllAsync()).AsEnumerable();
            var isAr = IsArabic();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                routes = routes.Where(r =>
                    Contains(r.NameEn, term) ||
                    Contains(r.NameAr, term) ||
                    Contains(r.StartLocationEn, term) ||
                    Contains(r.StartLocationAr, term) ||
                    Contains(r.EndLocationEn, term) ||
                    Contains(r.EndLocationAr, term));
            }

            var ordered = routes.OrderByDescending(r => r.Id).ToList();
            var totalCount = ordered.Count;

            var items = ordered
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(r => new RouteDto
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
                })
                .ToList();

            return new PagedResultDto<RouteDto>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
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
                Status = t.Status,
                PassengerCount = t.PassengerCount,
                OdometerKm = t.OdometerKm,
                FuelConsumedLiters = t.FuelConsumedLiters,
                ClientName = t.ClientName,
                BookingReference = t.BookingReference
            }).ToList();
        }

        // Paginated + search listing for the Trips scheduling page. Reuses GetAllTripsAsync's
        // already-mapped, already-lookup-joined list (vehicle plate/route/driver/project names)
        // and just adds search filtering + Skip/Take on top, matching the in-memory paging
        // pattern used by GetVehiclesPagedAsync above.
        public async Task<PagedResultDto<TripDto>> GetTripsPagedAsync(int page, int pageSize, string? search)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 20;
            if (pageSize > 200) pageSize = 200;

            var trips = (await GetAllTripsAsync()).AsEnumerable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                trips = trips.Where(t =>
                    Contains(t.VehiclePlate, term) ||
                    Contains(t.RouteName, term) ||
                    Contains(t.DriverName, term) ||
                    Contains(t.ProjectName, term) ||
                    Contains(t.ClientName, term) ||
                    Contains(t.BookingReference, term) ||
                    Contains(t.Status.ToString(), term));
            }

            var ordered = trips.ToList();
            var totalCount = ordered.Count;

            var items = ordered
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return new PagedResultDto<TripDto>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
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
                Status = t.Status,
                PassengerCount = t.PassengerCount,
                OdometerKm = t.OdometerKm,
                FuelConsumedLiters = t.FuelConsumedLiters,
                ClientName = t.ClientName,
                BookingReference = t.BookingReference
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
                Status = dto.Status,
                PassengerCount = dto.PassengerCount,
                OdometerKm = dto.OdometerKm,
                FuelConsumedLiters = dto.FuelConsumedLiters,
                ClientName = dto.ClientName,
                BookingReference = dto.BookingReference
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
            trip.PassengerCount = dto.PassengerCount;
            trip.OdometerKm = dto.OdometerKm;
            trip.FuelConsumedLiters = dto.FuelConsumedLiters;
            trip.ClientName = dto.ClientName;
            trip.BookingReference = dto.BookingReference;

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

        #region Fleet KPIs
        // 5 new indicators computed from the Vehicle table, populated via the Fleet Details bulk
        // upload (plate/make/model/year/capacity). Displayed under "إدارة الأسطول".
        public async Task<FleetKpisDto> GetFleetKpisAsync()
        {
            var vehicles = (await _vehicleRepository.GetAllAsync()).ToList();
            var totalVehicles = vehicles.Count;

            // Capacity is optional now (the Vehicle Management register this table is seeded from
            // has no seat-count column, so ~700 seeded vehicles have no Capacity value at all).
            // Treating an unknown capacity as 0 would make these two KPIs collapse toward zero the
            // moment the real register is seeded, which reads as a bug rather than "no data yet".
            // Instead both figures are computed only over vehicles that actually have a Capacity,
            // same as a person skimming the spreadsheet would do by eye.
            var vehiclesWithCapacity = vehicles.Where(v => v.Capacity.HasValue).ToList();
            var totalCapacity = vehiclesWithCapacity.Sum(v => v.Capacity!.Value);

            var currentYear = System.DateTime.UtcNow.Year;
            double avgAge = totalVehicles > 0 ? vehicles.Average(v => (double)(currentYear - v.Year)) : 0;

            int modernCount = vehicles.Count(v => (currentYear - v.Year) <= 5);
            double modernizationRate = totalVehicles > 0 ? ((double)modernCount / totalVehicles) * 100.0 : 0;

            int busTypeVariety = vehicles.Select(v => v.Model).Distinct(System.StringComparer.OrdinalIgnoreCase).Count();

            double avgCapacityPerBus = vehiclesWithCapacity.Count > 0 ? (double)(totalCapacity / vehiclesWithCapacity.Count) : 0;

            return new FleetKpisDto
            {
                TotalSeatingCapacityActual = totalCapacity,
                TotalSeatingCapacityTarget = totalCapacity,

                AverageBusAgeActual = System.Math.Round(avgAge, 1),
                AverageBusAgeTarget = 5.0,

                FleetModernizationRateActual = System.Math.Round(modernizationRate, 1),
                FleetModernizationRateTarget = 80.0,

                BusTypeVarietyCountActual = busTypeVariety,
                BusTypeVarietyCountTarget = busTypeVariety,

                AverageCapacityPerBusActual = System.Math.Round(avgCapacityPerBus, 1),
                AverageCapacityPerBusTarget = 49.0
            };
        }
        #endregion

        #region Vehicle Management Excel Template

        // The approved Vehicle Management template. This is the single source of truth for what
        // columns are accepted, which are required, and what header text/keywords (English +
        // Arabic) identify each one. Column semantics follow the existing Make/Model split already
        // used throughout this codebase: Make = manufacturer/brand, Model = bus type/model name.
        private static readonly ExcelTemplateDefinition VehicleTemplate = new()
        {
            TemplateName = "Vehicle Management",
            IdentityColumnKey = "LicensePlate",
            Columns = new List<ExcelColumnDefinition>
            {
                new() { Key = "LicensePlate", DisplayName = "Vehicle Plate Number", Required = true,
                    HeaderAliases = new[] { "vehicle plate number", "license plate", "plate number", "licnse", "plate", "اللوحة", "رقم اللوحة", "id no" } },
                // Optional for the same reason as Capacity: the real register has no manufacturer
                // column for a large share of vehicles (rented units, generic "Coaster" entries).
                new() { Key = "Make", DisplayName = "Make", Required = false,
                    HeaderAliases = new[] { "make", "manufacturer", "brand", "صانع" } },
                new() { Key = "Model", DisplayName = "Model / Vehicle Type", Required = true,
                    HeaderAliases = new[] { "bus type", "نوع الحافلة", "vehicle type", "model" } },
                new() { Key = "Year", DisplayName = "Year", Required = true,
                    HeaderAliases = new[] { "year", "سنة الصنع", "سنة" } },
                // Optional: the official Vehicle Management register most real vehicles are imported
                // from has no seat-count column at all (see DashboardApiController's TotalCapacity/
                // AverageCapacity comment on the NewFeature side, and Vehicle.Capacity being decimal?).
                new() { Key = "Capacity", DisplayName = "Capacity", Required = false,
                    HeaderAliases = new[] { "capacity", "سعة", "seats", "passengers" } },
                new() { Key = "Status", DisplayName = "Status", Required = false,
                    HeaderAliases = new[] { "status", "حالة" } },
                new() { Key = "Mileage", DisplayName = "Mileage", Required = false,
                    HeaderAliases = new[] { "mileage", "عداد", "kilometers", "km" } },
                // Optional short internal fleet/bus number (e.g. the real register's "COMP. Serial"),
                // distinct from the license plate. Maintenance and Operations work-order/trip imports
                // reference vehicles by this shorter number rather than the plate, so populating it
                // here is what lets those uploads resolve to real vehicles instead of creating
                // placeholders.
                new() { Key = "BusNumber", DisplayName = "Bus Number", Required = false,
                    HeaderAliases = new[] { "bus number", "comp. serial", "comp serial", "رقم الحافلة", "الرقم التسلسلي" } },
            }
        };

        private static bool TryParseVehicleStatus(string raw, out VehicleStatus status)
        {
            var normalized = raw.Trim().ToLowerInvariant();
            switch (normalized)
            {
                case "available":
                case "متاح":
                case "متاحة":
                    status = VehicleStatus.Available; return true;
                case "active":
                case "نشط":
                case "نشطة":
                    status = VehicleStatus.Active; return true;
                case "inmaintenance":
                case "in maintenance":
                case "maintenance":
                case "تحت الصيانة":
                case "صيانة":
                    status = VehicleStatus.InMaintenance; return true;
                case "outofservice":
                case "out of service":
                case "خارج الخدمة":
                    status = VehicleStatus.OutOfService; return true;
                default:
                    status = VehicleStatus.Available; return false;
            }
        }

        #endregion

        #region Bulk Upload
        public async Task<ExcelImportResultDto> BulkUploadVehiclesAsync(System.IO.Stream excelStream)
        {
            // Loaded once, up front - the row handler below only ever reads/writes this in-memory
            // dictionary, never queries the database again per row (fixes the N+1 query pattern
            // the previous implementation had via GetAllAsync() inside the loop).
            var existingVehicles = (await _vehicleRepository.GetAllAsync())
                .GroupBy(v => NormalizePlate(v.LicensePlate))
                .ToDictionary(g => g.Key, g => g.First());

            async Task<ExcelRowOutcomeResult> ProcessRow(ExcelRowContext ctx)
            {
                var plate = ctx.GetString("LicensePlate");
                if (string.IsNullOrEmpty(plate))
                    return ExcelRowOutcomeResult.Skipped("Vehicle Plate Number is required.", "Vehicle Plate Number");
                if (plate.Length > 20)
                    return ExcelRowOutcomeResult.Skipped("Vehicle Plate Number cannot exceed 20 characters.", "Vehicle Plate Number");

                var make = ctx.GetString("Make");
                if (string.IsNullOrEmpty(make)) make = "Unknown";
                if (make.Length > 50)
                    return ExcelRowOutcomeResult.Skipped("Make cannot exceed 50 characters.", "Make");

                var model = ctx.GetString("Model");
                if (string.IsNullOrEmpty(model))
                    return ExcelRowOutcomeResult.Skipped("Model is required.", "Model / Vehicle Type");
                if (model.Length > 50)
                    return ExcelRowOutcomeResult.Skipped("Model cannot exceed 50 characters.", "Model / Vehicle Type");

                var yearStr = ctx.GetString("Year");
                if (!int.TryParse(yearStr, out var year))
                    return ExcelRowOutcomeResult.Skipped("Year is required and must be a whole number.", "Year");
                if (year < 1900 || year > 2100)
                    return ExcelRowOutcomeResult.Skipped("Year must be between 1900 and 2100.", "Year");

                decimal? capacity = null;
                var capacityStr = ctx.GetString("Capacity");
                if (!string.IsNullOrEmpty(capacityStr))
                {
                    if (!decimal.TryParse(capacityStr, out var parsedCapacity))
                        return ExcelRowOutcomeResult.Skipped("Capacity must be a valid number.", "Capacity");
                    if (parsedCapacity <= 0 || parsedCapacity > 1000)
                        return ExcelRowOutcomeResult.Skipped("Capacity must be greater than 0 and at most 1000.", "Capacity");
                    capacity = parsedCapacity;
                }

                var status = VehicleStatus.Available;
                var statusStr = ctx.GetString("Status");
                if (!string.IsNullOrEmpty(statusStr) && !TryParseVehicleStatus(statusStr, out status))
                    return ExcelRowOutcomeResult.Skipped($"Status \"{statusStr}\" is not recognized. Use one of: Available, Active, InMaintenance, OutOfService.", "Status");

                decimal? mileage = null;
                var mileageStr = ctx.GetString("Mileage");
                if (!string.IsNullOrEmpty(mileageStr))
                {
                    if (!decimal.TryParse(mileageStr, out var parsedMileage))
                        return ExcelRowOutcomeResult.Skipped("Mileage must be a valid number.", "Mileage");
                    if (parsedMileage < 0)
                        return ExcelRowOutcomeResult.Skipped("Mileage cannot be negative.", "Mileage");
                    mileage = parsedMileage;
                }

                var busNumber = ctx.GetString("BusNumber");
                if (string.IsNullOrEmpty(busNumber)) busNumber = null;

                // Vehicle Plate Number is the business key: a plate already on file is updated in
                // place rather than duplicated, matching the requirement that re-uploading a known
                // vehicle should not create a second record.
                var key = NormalizePlate(plate);
                if (existingVehicles.TryGetValue(key, out var existing))
                {
                    existing.Make = make;
                    existing.Model = model;
                    existing.Year = year;
                    existing.Capacity = capacity;
                    existing.Status = status;
                    existing.Mileage = mileage;
                    existing.BusNumber = busNumber;
                    return ExcelRowOutcomeResult.Updated();
                }

                var vehicle = new Vehicle
                {
                    LicensePlate = plate,
                    Make = make,
                    Model = model,
                    Year = year,
                    Capacity = capacity,
                    Status = status,
                    Mileage = mileage,
                    BusNumber = busNumber
                };
                await _vehicleRepository.AddAsync(vehicle);
                existingVehicles[key] = vehicle; // so a duplicate plate later in the same file updates, not conflicts
                return ExcelRowOutcomeResult.Inserted();
            }

            var result = await ExcelImportEngine.RunAsync(
                excelStream,
                VehicleTemplate,
                ProcessRow,
                _vehicleRepository.SaveChangesAsync,
                _logger);

            if (result.InsertedRows + result.UpdatedRows > 0)
                await DbMaintenanceHelper.RefreshStatisticsAsync(_context, _logger, "Vehicles");

            return result;
        }

        private static string NormalizePlate(string plate) =>
            (plate ?? string.Empty).Replace(" ", "").ToUpperInvariant();

        private int FindColumn(Dictionary<string, int> headers, params string[] searchTerms)
        {
            foreach (var term in searchTerms)
            {
                var match = headers.Keys.FirstOrDefault(k => k.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0);
                if (match != null) return headers[match];
            }
            return -1;
        }

        private static bool HeaderLooksLikeTripsSheet(ClosedXML.Excel.IXLWorksheet worksheet)
        {
            string[] keywords = { "vehicle", "مركبة", "حافلة", "route", "مسار", "driver", "سائق", "departure", "مغادرة", "arrival", "وصول" };
            return HeaderContainsAny(worksheet, keywords);
        }

        private static bool HeaderContainsAny(ClosedXML.Excel.IXLWorksheet worksheet, string[] keywords)
        {
            var firstRow = worksheet.Row(1);
            var lastCell = firstRow.LastCellUsed();
            if (lastCell == null) return false;

            for (int i = 1; i <= lastCell.Address.ColumnNumber; i++)
            {
                var val = firstRow.Cell(i).GetString();
                if (string.IsNullOrWhiteSpace(val)) continue;

                foreach (var kw in keywords)
                {
                    if (val.IndexOf(kw, StringComparison.OrdinalIgnoreCase) >= 0) return true;
                }
            }
            return false;
        }

        // Route bulk-upload used to live here (positional columns 1-7, no header validation, no
        // per-row structured result). It has been superseded by RouteOperationsService.
        // BulkUploadRoutesAsync, which uses the shared ExcelImportEngine with proper header
        // matching, per-row validation, and update-by-NameEn duplicate handling. See
        // Services/RouteOperationsService.cs and Services/IRouteOperationsService.cs.

        public async Task<(int SuccessCount, List<string> Errors)> BulkUploadTripsAsync(System.IO.Stream excelStream)
        {
            var errors = new List<string>();
            int successCount = 0;

            try
            {
                using var workbook = new ClosedXML.Excel.XLWorkbook(excelStream);
                var worksheet = workbook.Worksheets.FirstOrDefault();
                if (worksheet == null) return (0, new List<string> { "Excel file is empty." });

                if (!HeaderLooksLikeTripsSheet(worksheet))
                {
                    errors.Add("This file doesn't look like a Trips sheet - expected columns like \"Vehicle\" / \"مركبة\", \"Route\" / \"مسار\", \"Driver\" / \"سائق\", or \"Departure\" / \"مغادرة\" were not found. Please check you uploaded the right file.");
                    return (0, errors);
                }

                var rows = worksheet.RowsUsed().Skip(1);
                foreach (var row in rows)
                {
                    try
                    {
                        int.TryParse(row.Cell(1).GetString(), out int vehicleId);
                        int.TryParse(row.Cell(2).GetString(), out int routeId);
                        var driverId = row.Cell(3).GetString().Trim();
                        int.TryParse(row.Cell(4).GetString(), out int projectId);
                        var scheduledDeparture = FlexibleDateParser.Parse(row.Cell(5).GetString()) ?? default;
                        var scheduledArrival = FlexibleDateParser.Parse(row.Cell(6).GetString()) ?? default;

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

                if (successCount > 0)
                {
                    await _tripRepository.SaveChangesAsync();
                    await DbMaintenanceHelper.RefreshStatisticsAsync(_context, _logger, "Trips");
                }
            }
            catch (System.Exception ex) { errors.Add(ex.Message); }

            return (successCount, errors);
        }
        #endregion
    }
}
