using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace NewFeature.Models
{
    [Index(nameof(LicensePlate), IsUnique = true)]
    public class Vehicle
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "License Plate is required")]
        [StringLength(20, ErrorMessage = "License Plate cannot exceed 20 characters")]
        public string LicensePlate { get; set; } = string.Empty;

        [Required(ErrorMessage = "Make is required")]
        [StringLength(50, ErrorMessage = "Make cannot exceed 50 characters")]
        public string Make { get; set; } = string.Empty;

        [Required(ErrorMessage = "Model is required")]
        [StringLength(50, ErrorMessage = "Model cannot exceed 50 characters")]
        public string Model { get; set; } = string.Empty;

        [Required(ErrorMessage = "Year is required")]
        [Range(1900, 2100, ErrorMessage = "Please enter a valid year between 1900 and 2100")]
        public int Year { get; set; }

        [Required(ErrorMessage = "Capacity is required")]
        [Range(0.1, 1000.0, ErrorMessage = "Capacity must be greater than zero")]
        public decimal Capacity { get; set; }

        [Required(ErrorMessage = "Vehicle Status is required")]
        public VehicleStatus Status { get; set; } = VehicleStatus.Available;

        // Navigation property
        public ICollection<Trip> Trips { get; set; } = new List<Trip>();
    }
}
