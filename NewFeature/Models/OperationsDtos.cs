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

    // Deliberately minimal: every field here is a plain, directly-observable count/rate computed
    // from real Trip records (which come straight from the monthly dispatch sheets, one row per bus
    // assignment). No illustrative targets, no metric that the source data can't actually support -
    // e.g. On-Time Performance and Fuel Efficiency were removed because the real dispatch sheets have
    // no actual-arrival or fuel/odometer reading, so those could only ever be fake numbers.
    public class OperationsKpisDto
    {
        public int TotalTrips { get; set; }
        public int CancelledTrips { get; set; }
        public double CancellationRatePercent { get; set; }
        public int ActiveDriversCount { get; set; }
        public int VehiclesDeployedCount { get; set; }
        public int ClientsServedCount { get; set; }
        public double AverageTripsPerDay { get; set; }
    }
}
