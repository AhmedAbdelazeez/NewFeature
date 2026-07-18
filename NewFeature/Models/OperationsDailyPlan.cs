using System;
using System.ComponentModel.DataAnnotations;

namespace NewFeature.Models
{
    public class OperationsDailyPlan
    {
        public int Id { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Range(0, 10000)]
        public int ScheduledTripsCount { get; set; }

        [Range(0, 10000)]
        public int CompletedTripsCount { get; set; }

        [Range(0, 100)]
        public double FuelEfficiencyIndex { get; set; } // %

        [Range(0, 100)]
        public double PassengerSatisfactionRate { get; set; } // %

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Pending"; // Pending, InProgress, Completed
    }
}
