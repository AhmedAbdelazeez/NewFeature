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
    public class ProjectService : IProjectService
    {
        private readonly IRepository<Project> _projectRepository;
        private readonly IRepository<Client> _clientRepository;
        private readonly IRepository<Trip> _tripRepository;
        private readonly IRepository<Vehicle> _vehicleRepository;
        private readonly IRepository<Models.Route> _routeRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ProjectService(
            IRepository<Project> projectRepository, 
            IRepository<Client> clientRepository, 
            IRepository<Trip> tripRepository,
            IRepository<Vehicle> vehicleRepository,
            IRepository<Models.Route> routeRepository,
            UserManager<ApplicationUser> userManager,
            IHttpContextAccessor httpContextAccessor)
        {
            _projectRepository = projectRepository;
            _clientRepository = clientRepository;
            _tripRepository = tripRepository;
            _vehicleRepository = vehicleRepository;
            _routeRepository = routeRepository;
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
    }
}
