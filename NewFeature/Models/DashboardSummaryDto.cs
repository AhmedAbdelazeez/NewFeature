using System;
using System.Collections.Generic;

namespace NewFeature.Models
{
    public class DashboardSummaryDto
    {
        public ProjectsSummaryDto Projects { get; set; } = new();
        public FleetSummaryDto Fleet { get; set; } = new();
        public TripsSummaryDto Trips { get; set; } = new();
        public TasksSummaryDto Tasks { get; set; } = new();
        public MilestonesSummaryDto Milestones { get; set; } = new();
        public ClientsSummaryDto Clients { get; set; } = new();
        public RoutesSummaryDto Routes { get; set; } = new();
        public List<ProjectDetailSummary> ProjectDetails { get; set; } = new();
        public List<VehicleDto> AllVehicles { get; set; } = new();
        public List<TripDto> RecentTrips { get; set; } = new();
        public List<TaskDto> RecentTasks { get; set; } = new();
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    }

    public class ProjectsSummaryDto
    {
        public int Total { get; set; }
        public int Active { get; set; }
        public int Completed { get; set; }
        public int OnHold { get; set; }
        public int Planning { get; set; }
        public int Delayed { get; set; } // endDate < now && status != Completed
        public int NewThisMonth { get; set; } // startDate in current month
        public double AverageCompletion { get; set; }
        public decimal TotalContractValue { get; set; }
        public decimal AverageContractValue { get; set; }
        public double CompletionRate { get; set; }
        public double DelayRate { get; set; }
        public int TotalRequiredVehicles { get; set; }
        public int TotalEstimatedTrips { get; set; }
    }

    public class FleetSummaryDto
    {
        public int Total { get; set; }
        public int Available { get; set; }
        public int Active { get; set; }
        public int InMaintenance { get; set; }
        public int OutOfService { get; set; }
        public double UtilizationRate { get; set; }
        public double MaintenanceRate { get; set; }
        public double AvailabilityRate { get; set; }
        public decimal TotalCapacity { get; set; }
        public decimal AverageCapacity { get; set; }

        // Operational Sub-metrics
        public double BusReplacementTime { get; set; } // ops-d-15
        public double StationEvacuationTime { get; set; } // ops-d-16
        public double DriverAbsenceRate { get; set; } // ops-d-17
        public double OperatorComplianceRate { get; set; } // ops-d-18
        public double UniformComplianceRate { get; set; } // ops-d-19
        public int BreakdownsCount { get; set; } // ops-d-20
        public double StationBreakdownResponseTime { get; set; } // ops-d-21
        public double InnerRouteBreakdownResponseTime { get; set; } // ops-d-22
        public double OuterRouteBreakdownResponseTime { get; set; } // ops-d-23
        public double ContractStandardComplianceRate { get; set; } // ops-d-24
        public double CapacityComplianceRate { get; set; } // ops-d-25
        public double BusCountComplianceRate { get; set; } // ops-d-26
        public double GuideBoardsComplianceRate { get; set; } // ops-d-27
        public double OperationalBoardsComplianceRate { get; set; } // ops-d-28
        public int UnauthorizedBusEntryViolations { get; set; } // ops-d-29
        public double SecurityGuardAvailabilityRate { get; set; } // ops-d-30
        public double SafetyQualifiedBusesRate { get; set; } // ops-d-31
        public double BusTrackingComplianceRate { get; set; } // ops-d-32
    }

    public class TripsSummaryDto
    {
        public int Total { get; set; }
        public int Scheduled { get; set; }
        public int InProgress { get; set; }
        public int Completed { get; set; }
        public int Cancelled { get; set; }
        public double CompletionRate { get; set; }
        public double CancellationRate { get; set; }
        public double OnTimeRate { get; set; }
        public decimal TotalDistanceKm { get; set; }
    }

    public class TasksSummaryDto
    {
        public int Total { get; set; }
        public int ToDo { get; set; }
        public int InProgress { get; set; }
        public int InReview { get; set; }
        public int Done { get; set; }
        public int Overdue { get; set; } // dueDate < now && status != Done
        public double CompletionRate { get; set; }
        public decimal TotalEstimatedHours { get; set; }
    }

    public class MilestonesSummaryDto
    {
        public int Total { get; set; }
        public int Completed { get; set; }
        public int Overdue { get; set; } // dueDate < now && !isCompleted
        public int Upcoming { get; set; } // next 30 days, not completed
        public double CompletionRate { get; set; }
    }

    public class ClientsSummaryDto
    {
        public int TotalClients { get; set; }
        public int TotalContracts { get; set; }
        public decimal TotalContractValue { get; set; }
    }

    public class RoutesSummaryDto
    {
        public int TotalRoutes { get; set; }
        public decimal TotalDistanceKm { get; set; }
    }

    public class ProjectDetailSummary
    {
        public int Id { get; set; }
        public string NameEn { get; set; } = string.Empty;
        public string NameAr { get; set; } = string.Empty;
        public string DescriptionEn { get; set; } = string.Empty;
        public string DescriptionAr { get; set; } = string.Empty;
        public string ClientName { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int Status { get; set; } // ProjectStatus enum value
        public string StatusName { get; set; } = string.Empty;
        public decimal ContractValue { get; set; }
        public int RequiredVehiclesCount { get; set; }
        public int EstimatedTripsCount { get; set; }
        public int TotalTrips { get; set; }
        public int CompletedTrips { get; set; }
        public int ActiveTrips { get; set; }
        public int CancelledTrips { get; set; }
        public double CompletionPercentage { get; set; }
        public int TotalTasks { get; set; }
        public int CompletedTasks { get; set; }
        public int TotalMilestones { get; set; }
        public int CompletedMilestones { get; set; }
        public bool IsDelayed { get; set; }
        public List<TaskDto> Tasks { get; set; } = new();
        public List<ProjectMilestoneSummaryDto> Milestones { get; set; } = new();
    }

    public class ProjectMilestoneSummaryDto
    {
        public int Id { get; set; }
        public string TitleEn { get; set; } = string.Empty;
        public string TitleAr { get; set; } = string.Empty;
        public DateTime DueDate { get; set; }
        public bool IsCompleted { get; set; }
    }
}
