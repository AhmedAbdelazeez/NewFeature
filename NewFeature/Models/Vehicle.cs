using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace NewFeature.Models
{
    [Index(nameof(LicensePlate), IsUnique = true)]
    public class Vehicle
    {
        public int Id { get; set; }

        // The fleet's own internal asset number (e.g. "VHC_00000622", "Bus_0001135"). Populated for
        // vehicles that came from the official Vehicle Management register bulk-seed; null for
        // vehicles created by hand through the Add Vehicle form, which has no field for it. Not
        // enforced unique at the database level - SQL Server unique indexes only permit a single
        // NULL row, and this column will legitimately hold many nulls, so a DB-level unique
        // constraint would break the second manually-created vehicle. The source register is
        // already internally unique on this field.
        [StringLength(30, ErrorMessage = "Bus Number cannot exceed 30 characters")]
        public string? BusNumber { get; set; }

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

        // Optional now: the official Vehicle Management register this table was seeded from does
        // not carry seat counts, so seeded vehicles have no Capacity until someone fills it in by
        // hand or a future bulk upload supplies it. Manually-created/edited vehicles can still set
        // it via the existing form, which continues to require it client-side.
        [Range(0.1, 1000.0, ErrorMessage = "Capacity must be greater than zero")]
        public decimal? Capacity { get; set; }

        [Required(ErrorMessage = "Vehicle Status is required")]
        public VehicleStatus Status { get; set; } = VehicleStatus.Available;

        // Optional odometer reading, in kilometers, as of the last Vehicle Management upload or
        // manual update. Distinct from MaintenanceWorkOrder.Odometer, which captures the reading
        // at the time of a specific repair - this field is the vehicle's current/last-known value.
        [Range(0, 5000000, ErrorMessage = "Mileage must be zero or greater")]
        public decimal? Mileage { get; set; }

        // Chassis / VIN number, from the register's "Shasia NO." column. Null for vehicles missing
        // full registration papers (all rented buses in the initial seed).
        [StringLength(30, ErrorMessage = "Chassis Number cannot exceed 30 characters")]
        public string? ChassisNumber { get; set; }

        // Internal body/type code from the register's "Bus Type" column (e.g. "CityYT2020"),
        // distinct from Model which carries the full descriptive name.
        [StringLength(30, ErrorMessage = "Bus Type Code cannot exceed 30 characters")]
        public string? BusTypeCode { get; set; }

        public bool HasAirConditioning { get; set; }

        // A maximum/limit kilometer value from the register ("MAxkilo"). This is not a live
        // odometer reading (see Mileage above) - the source register has no odometer column at all.
        [Range(0, 5000000, ErrorMessage = "Max Kilometers must be zero or greater")]
        public decimal? MaxKilometers { get; set; }

        // True when the register's "Responsibility" column reads "Storage" - the vehicle is parked
        // and not currently checked out to anyone. False means it's assigned out under
        // ResponsibilityCode/ResponsibleEmployeeName below.
        public bool IsInStorage { get; set; }

        [StringLength(30, ErrorMessage = "Responsibility Code cannot exceed 30 characters")]
        public string? ResponsibilityCode { get; set; }

        [StringLength(150, ErrorMessage = "Responsible Employee Name cannot exceed 150 characters")]
        public string? ResponsibleEmployeeName { get; set; }

        // Navigation property
        public ICollection<Trip> Trips { get; set; } = new List<Trip>();
    }
}
