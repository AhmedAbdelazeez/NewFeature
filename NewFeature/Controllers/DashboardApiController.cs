using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NewFeature.Models;
using NewFeature.Services.Repositories;

namespace NewFeature.Controllers
{
    [ApiController]
    [Route("api/dashboard")]
    [AllowAnonymous]
    public class DashboardApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public DashboardApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("summary")]
        public async Task<ActionResult<DashboardSummaryDto>> GetSummary()
        {
            try
            {
                var now = DateTime.UtcNow;

                // Fetch raw data in parallel or sequentially depending on EF core limitation (sequential is safer for single context)
                var projects = await _context.Projects.Include(p => p.Client).ToListAsync();
                var vehicles = await _context.Vehicles.ToListAsync();
                var trips = await _context.Trips.Include(t => t.Route).Include(t => t.Driver).ToListAsync();
                var tasks = await _context.Tasks.ToListAsync();
                var milestones = await _context.ProjectMilestones.ToListAsync();
                var clients = await _context.Clients.ToListAsync();
                var contracts = await _context.Contracts.ToListAsync();
                var routes = await _context.Routes.ToListAsync();
                var users = await _context.Users.ToListAsync();

                var userMap = users.ToDictionary(u => u.Id, u => u.FullNameAr ?? u.FullNameEn ?? u.UserName ?? "Unknown");

                // 1. Projects Summary
                var projSummary = new ProjectsSummaryDto
                {
                    Total = projects.Count,
                    Active = projects.Count(p => p.Status == ProjectStatus.Active),
                    Completed = projects.Count(p => p.Status == ProjectStatus.Completed),
                    OnHold = projects.Count(p => p.Status == ProjectStatus.OnHold),
                    Planning = projects.Count(p => p.Status == ProjectStatus.Planning),
                    Delayed = projects.Count(p => p.EndDate < now && p.Status != ProjectStatus.Completed),
                    NewThisMonth = projects.Count(p => p.StartDate.Year == now.Year && p.StartDate.Month == now.Month),
                    TotalContractValue = projects.Sum(p => p.ContractValue),
                    TotalRequiredVehicles = projects.Sum(p => p.RequiredVehiclesCount),
                    TotalEstimatedTrips = projects.Sum(p => p.EstimatedTripsCount)
                };
                projSummary.AverageContractValue = projSummary.Total > 0 ? projSummary.TotalContractValue / projSummary.Total : 0;
                projSummary.CompletionRate = projSummary.Total > 0 ? (double)projSummary.Completed / projSummary.Total * 100 : 0;
                projSummary.DelayRate = projSummary.Total > 0 ? (double)projSummary.Delayed / projSummary.Total * 100 : 0;

                // Calculate Project Details
                var projectDetailsList = new List<ProjectDetailSummary>();
                foreach (var p in projects)
                {
                    var projTrips = trips.Where(t => t.ProjectId == p.Id).ToList();
                    var projTasks = tasks.Where(t => t.ProjectId == p.Id).ToList();
                    var projMilestones = milestones.Where(m => m.ProjectId == p.Id).ToList();

                    var completedTrips = projTrips.Count(t => t.Status == TripStatus.Completed);
                    var activeTrips = projTrips.Count(t => t.Status == TripStatus.InProgress);
                    var cancelledTrips = projTrips.Count(t => t.Status == TripStatus.Cancelled);

                    double completionPercentage = 0;
                    if (projTasks.Count > 0)
                    {
                        completionPercentage = (double)projTasks.Count(t => t.Status == NewFeature.Models.TaskStatus.Done) / projTasks.Count * 100;
                    }
                    else if (p.EstimatedTripsCount > 0)
                    {
                        completionPercentage = (double)completedTrips / p.EstimatedTripsCount * 100;
                    }

                    projectDetailsList.Add(new ProjectDetailSummary
                    {
                        Id = p.Id,
                        NameEn = p.NameEn,
                        NameAr = p.NameAr,
                        DescriptionEn = p.DescriptionEn,
                        DescriptionAr = p.DescriptionAr,
                        ClientName = p.Client?.NameAr ?? p.Client?.NameEn ?? "Unknown",
                        StartDate = p.StartDate,
                        EndDate = p.EndDate,
                        Status = (int)p.Status,
                        StatusName = p.Status.ToString(),
                        ContractValue = p.ContractValue,
                        RequiredVehiclesCount = p.RequiredVehiclesCount,
                        EstimatedTripsCount = p.EstimatedTripsCount,
                        TotalTrips = projTrips.Count,
                        CompletedTrips = completedTrips,
                        ActiveTrips = activeTrips,
                        CancelledTrips = cancelledTrips,
                        CompletionPercentage = completionPercentage,
                        TotalTasks = projTasks.Count,
                        CompletedTasks = projTasks.Count(t => t.Status == NewFeature.Models.TaskStatus.Done),
                        TotalMilestones = projMilestones.Count,
                        CompletedMilestones = projMilestones.Count(m => m.IsCompleted),
                        IsDelayed = p.EndDate < now && p.Status != ProjectStatus.Completed,
                        Tasks = projTasks.Select(t => new TaskDto
                        {
                            Id = t.Id,
                            ProjectId = t.ProjectId,
                            ProjectName = p.NameAr,
                            TitleEn = t.TitleEn,
                            TitleAr = t.TitleAr,
                            DescriptionEn = t.DescriptionEn,
                            DescriptionAr = t.DescriptionAr,
                            StartDate = t.StartDate,
                            DueDate = t.DueDate,
                            EstimatedHours = t.EstimatedHours,
                            Status = t.Status,
                            AssignedToUserId = t.AssignedToUserId,
                            AssignedToUserName = t.AssignedToUserId != null && userMap.TryGetValue(t.AssignedToUserId, out var tun) ? tun : "Unassigned"
                        }).ToList(),
                        Milestones = projMilestones.Select(m => new ProjectMilestoneSummaryDto
                        {
                            Id = m.Id,
                            TitleEn = m.TitleEn,
                            TitleAr = m.TitleAr,
                            DueDate = m.DueDate,
                            IsCompleted = m.IsCompleted
                        }).ToList()
                    });
                }

                projSummary.AverageCompletion = projectDetailsList.Count > 0 ? projectDetailsList.Average(pd => pd.CompletionPercentage) : 0;

                // 2. Fleet Summary
                var fleetSummary = new FleetSummaryDto
                {
                    Total = vehicles.Count,
                    Available = vehicles.Count(v => v.Status == VehicleStatus.Available),
                    Active = vehicles.Count(v => v.Status == VehicleStatus.Active),
                    InMaintenance = vehicles.Count(v => v.Status == VehicleStatus.InMaintenance),
                    OutOfService = vehicles.Count(v => v.Status == VehicleStatus.OutOfService),
                    TotalCapacity = vehicles.Sum(v => v.Capacity)
                };
                fleetSummary.AverageCapacity = fleetSummary.Total > 0 ? fleetSummary.TotalCapacity / fleetSummary.Total : 0;
                fleetSummary.UtilizationRate = fleetSummary.Total > fleetSummary.OutOfService ? 
                    (double)fleetSummary.Active / (fleetSummary.Total - fleetSummary.OutOfService) * 100 : 0;
                fleetSummary.MaintenanceRate = fleetSummary.Total > 0 ? (double)fleetSummary.InMaintenance / fleetSummary.Total * 100 : 0;
                fleetSummary.AvailabilityRate = fleetSummary.Total > 0 ? (double)(fleetSummary.Available + fleetSummary.Active) / fleetSummary.Total * 100 : 0;

                // 3. Trips Summary
                var completedTripsList = trips.Where(t => t.Status == TripStatus.Completed).ToList();
                var tripsSummary = new TripsSummaryDto
                {
                    Total = trips.Count,
                    Scheduled = trips.Count(t => t.Status == TripStatus.Scheduled),
                    InProgress = trips.Count(t => t.Status == TripStatus.InProgress),
                    Completed = trips.Count(t => t.Status == TripStatus.Completed),
                    Cancelled = trips.Count(t => t.Status == TripStatus.Cancelled),
                    TotalDistanceKm = completedTripsList.Sum(t => t.Route?.DistanceKm ?? 0)
                };
                tripsSummary.CompletionRate = tripsSummary.Total > tripsSummary.Cancelled ? 
                    (double)tripsSummary.Completed / (tripsSummary.Total - tripsSummary.Cancelled) * 100 : 0;
                tripsSummary.CancellationRate = tripsSummary.Total > 0 ? (double)tripsSummary.Cancelled / tripsSummary.Total * 100 : 0;

                // On Time Rate (ActualArrival <= ScheduledArrival)
                int onTimeCompleted = completedTripsList.Count(t => t.ActualArrival.HasValue && t.ActualArrival.Value <= t.ScheduledArrival);
                tripsSummary.OnTimeRate = tripsSummary.Completed > 0 ? (double)onTimeCompleted / tripsSummary.Completed * 100 : 0;

                // 4. Tasks Summary
                var tasksSummary = new TasksSummaryDto
                {
                    Total = tasks.Count,
                    ToDo = tasks.Count(t => t.Status == NewFeature.Models.TaskStatus.ToDo),
                    InProgress = tasks.Count(t => t.Status == NewFeature.Models.TaskStatus.InProgress),
                    InReview = tasks.Count(t => t.Status == NewFeature.Models.TaskStatus.InReview),
                    Done = tasks.Count(t => t.Status == NewFeature.Models.TaskStatus.Done),
                    Overdue = tasks.Count(t => t.DueDate < now && t.Status != NewFeature.Models.TaskStatus.Done),
                    TotalEstimatedHours = tasks.Sum(t => t.EstimatedHours)
                };
                tasksSummary.CompletionRate = tasksSummary.Total > 0 ? (double)tasksSummary.Done / tasksSummary.Total * 100 : 0;

                // 5. Milestones Summary
                var milestonesSummary = new MilestonesSummaryDto
                {
                    Total = milestones.Count,
                    Completed = milestones.Count(m => m.IsCompleted),
                    Overdue = milestones.Count(m => m.DueDate < now && !m.IsCompleted),
                    Upcoming = milestones.Count(m => m.DueDate >= now && m.DueDate <= now.AddDays(30) && !m.IsCompleted)
                };
                milestonesSummary.CompletionRate = milestonesSummary.Total > 0 ? (double)milestonesSummary.Completed / milestonesSummary.Total * 100 : 0;

                // 6. Clients Summary
                var clientsSummary = new ClientsSummaryDto
                {
                    TotalClients = clients.Count,
                    TotalContracts = contracts.Count,
                    TotalContractValue = contracts.Sum(c => c.TotalAmount)
                };

                // 7. Routes Summary
                var routesSummary = new RoutesSummaryDto
                {
                    TotalRoutes = routes.Count,
                    TotalDistanceKm = routes.Sum(r => r.DistanceKm)
                };

                // 8. Vehicle List
                var vehicleList = vehicles.Select(v => new VehicleDto
                {
                    Id = v.Id,
                    LicensePlate = v.LicensePlate,
                    Make = v.Make,
                    Model = v.Model,
                    Year = v.Year,
                    Capacity = v.Capacity,
                    Status = v.Status
                }).ToList();

                // 9. Recent lists
                var recentTrips = trips.OrderByDescending(t => t.ScheduledDeparture).Take(10).Select(t => new TripDto
                {
                    Id = t.Id,
                    VehicleId = t.VehicleId,
                    VehiclePlate = t.Vehicle?.LicensePlate ?? "Unknown",
                    RouteId = t.RouteId,
                    RouteName = t.Route?.NameAr ?? t.Route?.NameEn ?? "Unknown",
                    DriverId = t.DriverId,
                    DriverName = t.DriverId != null && userMap.TryGetValue(t.DriverId, out var dn) ? dn : "Unknown",
                    ScheduledDeparture = t.ScheduledDeparture,
                    ScheduledArrival = t.ScheduledArrival,
                    ActualDeparture = t.ActualDeparture,
                    ActualArrival = t.ActualArrival,
                    Status = t.Status,
                    ProjectId = t.ProjectId,
                    ProjectName = t.ProjectId.HasValue ? (projects.FirstOrDefault(pr => pr.Id == t.ProjectId.Value)?.NameAr ?? "Unknown") : "None"
                }).ToList();

                var recentTasks = tasks.OrderBy(t => t.DueDate).Take(10).Select(t => new TaskDto
                {
                    Id = t.Id,
                    ProjectId = t.ProjectId,
                    ProjectName = projects.FirstOrDefault(pr => pr.Id == t.ProjectId)?.NameAr ?? "Unknown",
                    TitleEn = t.TitleEn,
                    TitleAr = t.TitleAr,
                    DescriptionEn = t.DescriptionEn,
                    DescriptionAr = t.DescriptionAr,
                    StartDate = t.StartDate,
                    DueDate = t.DueDate,
                    EstimatedHours = t.EstimatedHours,
                    Status = t.Status,
                    AssignedToUserId = t.AssignedToUserId,
                    AssignedToUserName = t.AssignedToUserId != null && userMap.TryGetValue(t.AssignedToUserId, out var tun) ? tun : "Unassigned"
                }).ToList();

                return Ok(new DashboardSummaryDto
                {
                    Projects = projSummary,
                    Fleet = fleetSummary,
                    Trips = tripsSummary,
                    Tasks = tasksSummary,
                    Milestones = milestonesSummary,
                    Clients = clientsSummary,
                    Routes = routesSummary,
                    ProjectDetails = projectDetailsList,
                    AllVehicles = vehicleList,
                    RecentTrips = recentTrips,
                    RecentTasks = recentTasks,
                    GeneratedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}\n{ex.StackTrace}");
            }
        }
    }
}
