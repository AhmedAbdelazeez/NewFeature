using System;
using System.Collections.Generic;

namespace NewFeature.Models
{
    public class ProjectKpiDto
    {
        // 1. Project Completion Rate
        public double ProjectCompletionRateActual { get; set; }
        public double ProjectCompletionRateTarget { get; set; } = 100.0;

        // 2. Project Delay Rate
        public double ProjectDelayRateActual { get; set; }
        public double ProjectDelayRateTarget { get; set; } = 0.0;

        // 3. Task Completion Rate
        public double TaskCompletionRateActual { get; set; }
        public double TaskCompletionRateTarget { get; set; } = 100.0;

        // 4. Overdue Task Rate
        public double OverdueTaskRateActual { get; set; }
        public double OverdueTaskRateTarget { get; set; } = 0.0;

        // 5. Milestone Completion Rate
        public double MilestoneCompletionRateActual { get; set; }
        public double MilestoneCompletionRateTarget { get; set; } = 100.0;

        // 6. Avg Project Completion %
        public double AverageProjectProgressActual { get; set; }
        public double AverageProjectProgressTarget { get; set; } = 100.0;

        // 7. Trip Fulfillment Rate
        public double TripFulfillmentRateActual { get; set; }
        public double TripFulfillmentRateTarget { get; set; } = 100.0;

        // 8. On-Time Trip Delivery Rate
        public double OnTimeTripDeliveryActual { get; set; }
        public double OnTimeTripDeliveryTarget { get; set; } = 95.0;

        // 9. Fleet Utilization Rate
        public double FleetUtilizationRateActual { get; set; }
        public double FleetUtilizationRateTarget { get; set; } = 85.0;

        // 10. Resource Efficiency
        public double ResourceEfficiencyActual { get; set; }
        public double ResourceEfficiencyTarget { get; set; } = 100.0;

        // Charts Data
        public List<ChartDataPoint> ProjectsByStatus { get; set; } = new();
        public List<ChartDataPoint> TasksByStatus { get; set; } = new();
        public List<ChartDataPoint> MonthlyTaskCompletion { get; set; } = new();
        public List<ChartDataPoint> TripFulfillmentPerProject { get; set; } = new();
    }

    public class ChartDataPoint
    {
        public string Label { get; set; } = string.Empty;
        public decimal Value { get; set; }
        public string? Color { get; set; }
    }
}

