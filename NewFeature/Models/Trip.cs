using System.ComponentModel.DataAnnotations;

namespace NewFeature.Models
{
    public class Trip
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Vehicle selection is required")]
        public int VehicleId { get; set; }

        [Required(ErrorMessage = "Route selection is required")]
        public int RouteId { get; set; }

        [Required(ErrorMessage = "Driver selection is required")]
        public string DriverId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Scheduled Departure is required")]
        public DateTime ScheduledDeparture { get; set; }

        [Required(ErrorMessage = "Scheduled Arrival is required")]
        public DateTime ScheduledArrival { get; set; }

        public DateTime? ActualDeparture { get; set; }
        public DateTime? ActualArrival { get; set; }

        [Required(ErrorMessage = "Trip Status is required")]
        public TripStatus Status { get; set; } = TripStatus.Scheduled;

        public int? ProjectId { get; set; }

        // Operational metrics captured from daily operations logs (optional - populated via bulk upload)
        public int? PassengerCount { get; set; }
        public double? OdometerKm { get; set; }
        public double? FuelConsumedLiters { get; set; }

        // Captured from the daily dispatch sheets ("العميل" / "امر الايجار") so trips can be traced
        // back to the Sales client and rental order without requiring a full Client FK resolution.
        [StringLength(200)]
        public string? ClientName { get; set; }

        [StringLength(50)]
        public string? BookingReference { get; set; }

        // Navigation properties
        public Vehicle? Vehicle { get; set; }
        public Route? Route { get; set; }
        public ApplicationUser? Driver { get; set; }
        public Project? Project { get; set; }
    }
}
