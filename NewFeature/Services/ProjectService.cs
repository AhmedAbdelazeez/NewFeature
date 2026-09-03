using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NewFeature.Models;
using Microsoft.Extensions.Logging;
using NewFeature.Services.ExcelImport;
using NewFeature.Services.Repositories;

namespace NewFeature.Services
{
    public class ProjectService : IProjectService
    {
        private readonly IRepository<Project> _projectRepository;
        private readonly IRepository<Client> _clientRepository;
        private readonly IRepository<Trip> _tripRepository;
        private readonly IRepository<Vehicle> _vehicleRepository;
        private readonly IRepository<Models.Route> _routeRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<ProjectService> _logger;

        public ProjectService(
            IRepository<Project> projectRepository, 
            IRepository<Client> clientRepository, 
            IRepository<Trip> tripRepository,
            IRepository<Vehicle> vehicleRepository,
            IRepository<Models.Route> routeRepository,
            UserManager<ApplicationUser> userManager,
            IHttpContextAccessor httpContextAccessor,
            ILogger<ProjectService> logger)
        {
            _projectRepository = projectRepository;
            _clientRepository = clientRepository;
            _tripRepository = tripRepository;
            _vehicleRepository = vehicleRepository;
            _routeRepository = routeRepository;
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
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

        public async Task<IEnumerable<ProjectDto>> GetAllProjectsAsync()
        {
            var projects = await _projectRepository.GetAllAsync();
            var clients = await _clientRepository.GetAllAsync();
            var isAr = IsArabic();
            var clientMap = clients.ToDictionary(c => c.Id, c => isAr ? c.NameAr : c.NameEn);

            return projects.OrderByDescending(p => p.Id).Select(p => new ProjectDto
            {
                Id = p.Id,
                ClientId = p.ClientId,
                ClientName = clientMap.TryGetValue(p.ClientId, out var name) ? name : "Unknown",
                NameEn = p.NameEn,
                NameAr = p.NameAr,
                DescriptionEn = p.DescriptionEn,
                DescriptionAr = p.DescriptionAr,
                StartDate = p.StartDate,
                EndDate = p.EndDate,
                Status = p.Status,
                ContractValue = p.ContractValue,
                RequiredVehiclesCount = p.RequiredVehiclesCount,
                EstimatedTripsCount = p.EstimatedTripsCount,
                Name = isAr ? p.NameAr : p.NameEn,
                Description = isAr ? p.DescriptionAr : p.DescriptionEn
            }).ToList();
        }

        // Backs the Project Management page's table. Same "materialize then page/search/order in
        // memory" approach as FleetService.GetVehiclesPagedAsync - IRepository<Project> has no
        // IQueryable, so this keeps the change scoped here instead of touching the shared
        // repository abstraction.
        public async Task<PagedResultDto<ProjectDto>> GetProjectsPagedAsync(int page, int pageSize, string? search)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 20;
            if (pageSize > 200) pageSize = 200;

            var projects = await _projectRepository.GetAllAsync();
            var clients = await _clientRepository.GetAllAsync();
            var isAr = IsArabic();
            var clientMap = clients.ToDictionary(c => c.Id, c => isAr ? c.NameAr : c.NameEn);

            var dtos = projects.Select(p => new ProjectDto
            {
                Id = p.Id,
                ClientId = p.ClientId,
                ClientName = clientMap.TryGetValue(p.ClientId, out var name) ? name : "Unknown",
                NameEn = p.NameEn,
                NameAr = p.NameAr,
                DescriptionEn = p.DescriptionEn,
                DescriptionAr = p.DescriptionAr,
                StartDate = p.StartDate,
                EndDate = p.EndDate,
                Status = p.Status,
                ContractValue = p.ContractValue,
                RequiredVehiclesCount = p.RequiredVehiclesCount,
                EstimatedTripsCount = p.EstimatedTripsCount,
                Name = isAr ? p.NameAr : p.NameEn,
                Description = isAr ? p.DescriptionAr : p.DescriptionEn
            }).AsEnumerable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                dtos = dtos.Where(d =>
                    Contains(d.NameEn, term) ||
                    Contains(d.NameAr, term) ||
                    Contains(d.ClientName, term) ||
                    Contains(d.Status.ToString(), term));
            }

            var ordered = dtos.OrderByDescending(d => d.Id).ToList();
            var totalCount = ordered.Count;

            var items = ordered.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            return new PagedResultDto<ProjectDto>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        private static bool Contains(string? haystack, string term) =>
            !string.IsNullOrEmpty(haystack) && haystack.IndexOf(term, System.StringComparison.OrdinalIgnoreCase) >= 0;

        public async Task<ProjectDto?> GetProjectByIdAsync(int id)
        {
            var p = await _projectRepository.GetByIdAsync(id);
            if (p == null) return null;

            var client = await _clientRepository.GetByIdAsync(p.ClientId);
            var isAr = IsArabic();

            return new ProjectDto
            {
                Id = p.Id,
                ClientId = p.ClientId,
                ClientName = client != null ? (isAr ? client.NameAr : client.NameEn) : "Unknown",
                NameEn = p.NameEn,
                NameAr = p.NameAr,
                DescriptionEn = p.DescriptionEn,
                DescriptionAr = p.DescriptionAr,
                StartDate = p.StartDate,
                EndDate = p.EndDate,
                Status = p.Status,
                ContractValue = p.ContractValue,
                RequiredVehiclesCount = p.RequiredVehiclesCount,
                EstimatedTripsCount = p.EstimatedTripsCount,
                Name = isAr ? p.NameAr : p.NameEn,
                Description = isAr ? p.DescriptionAr : p.DescriptionEn
            };
        }

        public async Task<ProjectDto> CreateProjectAsync(ProjectDto dto)
        {
            var project = new Project
            {
                ClientId = dto.ClientId,
                NameEn = dto.NameEn,
                NameAr = dto.NameAr,
                DescriptionEn = dto.DescriptionEn,
                DescriptionAr = dto.DescriptionAr,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                Status = dto.Status,
                ContractValue = dto.ContractValue,
                RequiredVehiclesCount = dto.RequiredVehiclesCount,
                EstimatedTripsCount = dto.EstimatedTripsCount
            };

            await _projectRepository.AddAsync(project);
            await _projectRepository.SaveChangesAsync();

            dto.Id = project.Id;
            return dto;
        }

        public async Task<bool> UpdateProjectAsync(ProjectDto dto)
        {
            var project = await _projectRepository.GetByIdAsync(dto.Id);
            if (project == null) return false;

            project.ClientId = dto.ClientId;
            project.NameEn = dto.NameEn;
            project.NameAr = dto.NameAr;
            project.DescriptionEn = dto.DescriptionEn;
            project.DescriptionAr = dto.DescriptionAr;
            project.StartDate = dto.StartDate;
            project.EndDate = dto.EndDate;
            project.Status = dto.Status;
            project.ContractValue = dto.ContractValue;
            project.RequiredVehiclesCount = dto.RequiredVehiclesCount;
            project.EstimatedTripsCount = dto.EstimatedTripsCount;

            await _projectRepository.UpdateAsync(project);
            await _projectRepository.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteProjectAsync(int id)
        {
            var project = await _projectRepository.GetByIdAsync(id);
            if (project == null) return false;

            await _projectRepository.DeleteAsync(project);
            await _projectRepository.SaveChangesAsync();
            return true;
        }

        public async Task<ProjectDetailsDto?> GetProjectDetailsAsync(int id)
        {
            var p = await _projectRepository.GetByIdAsync(id);
            if (p == null) return null;

            var client = await _clientRepository.GetByIdAsync(p.ClientId);
            var isAr = IsArabic();

            var projectDto = new ProjectDto
            {
                Id = p.Id,
                ClientId = p.ClientId,
                ClientName = client != null ? (isAr ? client.NameAr : client.NameEn) : "Unknown",
                NameEn = p.NameEn,
                NameAr = p.NameAr,
                DescriptionEn = p.DescriptionEn,
                DescriptionAr = p.DescriptionAr,
                StartDate = p.StartDate,
                EndDate = p.EndDate,
                Status = p.Status,
                ContractValue = p.ContractValue,
                RequiredVehiclesCount = p.RequiredVehiclesCount,
                EstimatedTripsCount = p.EstimatedTripsCount,
                Name = isAr ? p.NameAr : p.NameEn,
                Description = isAr ? p.DescriptionAr : p.DescriptionEn
            };

            // Get all trips for this project
            var allTrips = await _tripRepository.GetAllAsync();
            var projectTrips = allTrips.Where(t => t.ProjectId == id).ToList();

            var vehicles = await _vehicleRepository.GetAllAsync();
            var routes = await _routeRepository.GetAllAsync();
            var drivers = await _userManager.Users.ToListAsync();

            var vehicleMap = vehicles.ToDictionary(v => v.Id, v => v.LicensePlate);
            var routeMap = routes.ToDictionary(r => r.Id, r => isAr ? r.NameAr : r.NameEn);
            var driverMap = drivers.ToDictionary(d => d.Id, d => isAr ? d.FullNameAr : d.FullNameEn);

            var tripDtos = projectTrips.Select(t => new TripDto
            {
                Id = t.Id,
                VehicleId = t.VehicleId,
                VehiclePlate = vehicleMap.TryGetValue(t.VehicleId, out var plate) ? plate : "Unknown",
                RouteId = t.RouteId,
                RouteName = routeMap.TryGetValue(t.RouteId, out var routeName) ? routeName : "Unknown",
                DriverId = t.DriverId,
                DriverName = driverMap.TryGetValue(t.DriverId, out var driverName) ? driverName : "Unknown",
                ProjectId = t.ProjectId,
                ProjectName = projectDto.Name,
                ScheduledDeparture = t.ScheduledDeparture,
                ScheduledArrival = t.ScheduledArrival,
                ActualDeparture = t.ActualDeparture,
                ActualArrival = t.ActualArrival,
                Status = t.Status
            }).ToList();

            int completed = tripDtos.Count(t => t.Status == TripStatus.Completed);
            int cancelled = tripDtos.Count(t => t.Status == TripStatus.Cancelled);
            int active = tripDtos.Count(t => t.Status == TripStatus.Scheduled || t.Status == TripStatus.InProgress);

            double percentage = 0;
            if (p.EstimatedTripsCount > 0)
            {
                percentage = Math.Round(((double)completed / p.EstimatedTripsCount) * 100, 2);
            }
            else if (tripDtos.Count > 0)
            {
                percentage = Math.Round(((double)completed / tripDtos.Count) * 100, 2);
            }

            return new ProjectDetailsDto
            {
                Project = projectDto,
                TotalTrips = tripDtos.Count,
                CompletedTrips = completed,
                ActiveTrips = active,
                CancelledTrips = cancelled,
                CompletionPercentage = percentage,
                Trips = tripDtos
            };
        }
        #region Bulk upload (approved single-sheet projects register)
        // Imports the approved Project Management template: one row per project. A client named in
        // the sheet that is not on file yet is created as a minimal record so the coordinator does
        // not have to register clients first; a project already on file (matched on Arabic name) is
        // updated rather than duplicated.
        public async Task<ExcelImportResultDto> BulkUploadProjectsAsync(System.IO.Stream excelStream)
        {
            var clients = (await _clientRepository.GetAllAsync()).ToList();
            var projects = (await _projectRepository.GetAllAsync()).ToList();

            return await ExcelImportEngine.RunAsync(
                excelStream,
                DepartmentTemplates.Projects,
                async row =>
                {
                    var name = row.GetString(DepartmentTemplates.ProjectName);
                    if (string.IsNullOrWhiteSpace(name))
                        return ExcelRowOutcomeResult.Skipped("اسم المشروع مطلوب.", "اسم المشروع");

                    var startDate = row.GetDate(DepartmentTemplates.ProjectStartDate);
                    if (startDate == null)
                        return ExcelRowOutcomeResult.Skipped("تاريخ البداية غير مقروء. استخدم الصيغة يوم/شهر/سنة.", "تاريخ البداية");

                    var endDate = row.GetDate(DepartmentTemplates.ProjectEndDate);
                    if (endDate == null)
                        return ExcelRowOutcomeResult.Skipped("تاريخ النهاية غير مقروء. استخدم الصيغة يوم/شهر/سنة.", "تاريخ النهاية");

                    // An end date before the start date would make the "delayed projects" count
                    // meaningless, so the row is rejected rather than imported.
                    if (endDate.Value.Date < startDate.Value.Date)
                        return ExcelRowOutcomeResult.Skipped("تاريخ النهاية أسبق من تاريخ البداية.", "تاريخ النهاية");

                    var clientName = row.GetString(DepartmentTemplates.ProjectClient);
                    if (string.IsNullOrWhiteSpace(clientName))
                        return ExcelRowOutcomeResult.Skipped("اسم العميل مطلوب.", "العميل");

                    var client = clients.FirstOrDefault(c =>
                        string.Equals(c.NameAr, clientName, StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(c.NameEn, clientName, StringComparison.OrdinalIgnoreCase));
                    if (client == null)
                    {
                        client = new Client
                        {
                            NameAr = clientName,
                            NameEn = clientName,
                            Code = $"CLI-{clients.Count + 1:D3}",
                            // Email/Phone are non-nullable on the entity but are not part of this
                            // template - placeholders keep the insert valid and are obviously
                            // placeholders when someone opens the client record to complete it.
                            Email = $"client{clients.Count + 1}@rawahel.local",
                            Phone = "-"
                        };
                        await _clientRepository.AddAsync(client);
                        clients.Add(client);
                    }

                    var status = ParseProjectStatus(row.GetString(DepartmentTemplates.ProjectStatus));
                    var contractValue = row.GetDecimal(DepartmentTemplates.ProjectContractValue) ?? 0m;
                    if (contractValue < 0) contractValue = 0m;
                    var vehicles = row.GetInt(DepartmentTemplates.ProjectVehicles) ?? 0;
                    if (vehicles < 0) vehicles = 0;
                    var trips = row.GetInt(DepartmentTemplates.ProjectTrips) ?? 0;
                    if (trips < 0) trips = 0;

                    var existing = projects.FirstOrDefault(p =>
                        string.Equals(p.NameAr, name, StringComparison.OrdinalIgnoreCase));

                    if (existing != null)
                    {
                        existing.NameEn = name;
                        existing.Client = client;
                        existing.StartDate = startDate.Value.Date;
                        existing.EndDate = endDate.Value.Date;
                        existing.Status = status;
                        existing.ContractValue = contractValue;
                        existing.RequiredVehiclesCount = vehicles;
                        existing.EstimatedTripsCount = trips;
                        return ExcelRowOutcomeResult.Updated();
                    }

                    var project = new Project
                    {
                        NameAr = name,
                        NameEn = name,
                        DescriptionAr = name,
                        DescriptionEn = name,
                        Client = client,
                        StartDate = startDate.Value.Date,
                        EndDate = endDate.Value.Date,
                        Status = status,
                        ContractValue = contractValue,
                        RequiredVehiclesCount = vehicles,
                        EstimatedTripsCount = trips
                    };
                    await _projectRepository.AddAsync(project);
                    projects.Add(project);
                    return ExcelRowOutcomeResult.Inserted();
                },
                () => _projectRepository.SaveChangesAsync(),
                _logger);
        }

        private static ProjectStatus ParseProjectStatus(string? raw)
        {
            var value = (raw ?? string.Empty).Trim();

            if (value.Contains("مكتمل") || value.Contains("منجز") ||
                value.IndexOf("completed", StringComparison.OrdinalIgnoreCase) >= 0)
                return ProjectStatus.Completed;

            if (value.Contains("متوقف") || value.Contains("معلق") ||
                value.IndexOf("hold", StringComparison.OrdinalIgnoreCase) >= 0)
                return ProjectStatus.OnHold;

            if (value.Contains("نشط") || value.Contains("جاري") || value.Contains("قيد التنفيذ") ||
                value.IndexOf("active", StringComparison.OrdinalIgnoreCase) >= 0 ||
                value.IndexOf("progress", StringComparison.OrdinalIgnoreCase) >= 0)
                return ProjectStatus.Active;

            return ProjectStatus.Planning;
        }
        #endregion
    }
}
