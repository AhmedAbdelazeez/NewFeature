using System;

namespace NewFeature.Models
{
    public class OperationsDailyPlanDto
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public int ScheduledTripsCount { get; set; }
        public int CompletedTripsCount { get; set; }
        public double FuelEfficiencyIndex { get; set; }
        public double PassengerSatisfactionRate { get; set; }
        public string Status { get; set; } = "Pending";
    }

    public class OperationsIncidentDto
    {
        public int Id { get; set; }
        public string DescriptionAr { get; set; } = string.Empty;
        public string DescriptionEn { get; set; } = string.Empty;
        public string Severity { get; set; } = "Medium";
        public double ResponseTimeMinutes { get; set; }
        public DateTime Date { get; set; }
        public string Status { get; set; } = "Open";
    }

    public class OperationsKpisDto
    {
        public double PlanAdherenceActual { get; set; }
        public double PlanAdherenceTarget { get; set; }

        public double FleetUtilizationActual { get; set; }
        public double FleetUtilizationTarget { get; set; }

        public double AvgBreakdownResponseActual { get; set; }
        public double AvgBreakdownResponseTarget { get; set; }

        public int ViolationsCountActual { get; set; }
        public int ViolationsCountTarget { get; set; }

        public double PassengerSatisfactionActual { get; set; }
        public double PassengerSatisfactionTarget { get; set; }

        public int ScheduledTripsActual { get; set; }
        public int ScheduledTripsTarget { get; set; }

        public double FuelEfficiencyActual { get; set; }
        public double FuelEfficiencyTarget { get; set; }
    }
}
