using System;
using System.ComponentModel.DataAnnotations;

namespace NewFeature.Models
{
    public class ClientDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "English Name is required")]
        [StringLength(150)]
        public string NameEn { get; set; } = string.Empty;

        [Required(ErrorMessage = "Arabic Name is required")]
        [StringLength(150)]
        public string NameAr { get; set; } = string.Empty;

        [Required(ErrorMessage = "Client Code is required")]
        [StringLength(20)]
        public string Code { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone is required")]
        [Phone]
        [StringLength(20)]
        public string Phone { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;
    }

    public class ProjectDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Client is required")]
        public int ClientId { get; set; }
        public string ClientName { get; set; } = string.Empty;

        [Required(ErrorMessage = "English Name is required")]
        [StringLength(150)]
        public string NameEn { get; set; } = string.Empty;

        [Required(ErrorMessage = "Arabic Name is required")]
        [StringLength(150)]
        public string NameAr { get; set; } = string.Empty;

        [Required(ErrorMessage = "English Description is required")]
        [StringLength(1000)]
        public string DescriptionEn { get; set; } = string.Empty;

        [Required(ErrorMessage = "Arabic Description is required")]
        [StringLength(1000)]
        public string DescriptionAr { get; set; } = string.Empty;

        [Required(ErrorMessage = "Start Date is required")]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "End Date is required")]
        public DateTime EndDate { get; set; }

        [Required]
        public ProjectStatus Status { get; set; }

        public decimal ContractValue { get; set; }
        public int RequiredVehiclesCount { get; set; }
        public int EstimatedTripsCount { get; set; }

        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    public class VehicleDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "License Plate is required")]
        [StringLength(20)]
        public string LicensePlate { get; set; } = string.Empty;

        [Required(ErrorMessage = "Make is required")]
        [StringLength(50)]
        public string Make { get; set; } = string.Empty;

        [Required(ErrorMessage = "Model is required")]
        [StringLength(50)]
        public string Model { get; set; } = string.Empty;

        [Required(ErrorMessage = "Year is required")]
        [Range(1900, 2100)]
        public int Year { get; set; }

        [Required(ErrorMessage = "Capacity is required")]
        [Range(0.1, 1000.0)]
        public decimal Capacity { get; set; }

        [Required]
        public VehicleStatus Status { get; set; }
    }

    public class RouteDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "English Name is required")]
        [StringLength(150)]
        public string NameEn { get; set; } = string.Empty;

        [Required(ErrorMessage = "Arabic Name is required")]
        [StringLength(150)]
        public string NameAr { get; set; } = string.Empty;

        [Required(ErrorMessage = "English Start Location is required")]
        [StringLength(200)]
        public string StartLocationEn { get; set; } = string.Empty;

        [Required(ErrorMessage = "Arabic Start Location is required")]
        [StringLength(200)]
        public string StartLocationAr { get; set; } = string.Empty;

        [Required(ErrorMessage = "English End Location is required")]
        [StringLength(200)]
        public string EndLocationEn { get; set; } = string.Empty;

        [Required(ErrorMessage = "Arabic End Location is required")]
        [StringLength(200)]
        public string EndLocationAr { get; set; } = string.Empty;

        [Required(ErrorMessage = "Distance in KM is required")]
        [Range(0.1, 100000.0)]
        public decimal DistanceKm { get; set; }

        public string Name { get; set; } = string.Empty;
        public string StartLocation { get; set; } = string.Empty;
        public string EndLocation { get; set; } = string.Empty;
    }

    public class TripDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Vehicle is required")]
        public int VehicleId { get; set; }
        public string VehiclePlate { get; set; } = string.Empty;

        [Required(ErrorMessage = "Route is required")]
        public int RouteId { get; set; }
        public string RouteName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Driver is required")]
        public string DriverId { get; set; } = string.Empty;
        public string DriverName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Scheduled Departure is required")]
        public DateTime ScheduledDeparture { get; set; }

        [Required(ErrorMessage = "Scheduled Arrival is required")]
        public DateTime ScheduledArrival { get; set; }

        public DateTime? ActualDeparture { get; set; }
        public DateTime? ActualArrival { get; set; }

        [Required]
        public TripStatus Status { get; set; }

        public int? ProjectId { get; set; }
        public string ProjectName { get; set; } = string.Empty;
    }

    public class TaskDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Project is required")]
        public int ProjectId { get; set; }
        public string ProjectName { get; set; } = string.Empty;

        [Required(ErrorMessage = "English Title is required")]
        [StringLength(150)]
        public string TitleEn { get; set; } = string.Empty;

        [Required(ErrorMessage = "Arabic Title is required")]
        [StringLength(150)]
        public string TitleAr { get; set; } = string.Empty;

        [Required(ErrorMessage = "English Description is required")]
        [StringLength(1000)]
        public string DescriptionEn { get; set; } = string.Empty;

        [Required(ErrorMessage = "Arabic Description is required")]
        [StringLength(1000)]
        public string DescriptionAr { get; set; } = string.Empty;

        public DateTime? StartDate { get; set; }

        [Required(ErrorMessage = "Due Date is required")]
        public DateTime DueDate { get; set; }

        [Required(ErrorMessage = "Estimated Hours is required")]
        [Range(0.0, 1000.0)]
        public decimal EstimatedHours { get; set; }

        [Required]
        public TaskStatus Status { get; set; }

        public string? AssignedToUserId { get; set; }
        public string AssignedToUserName { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    public class UserDto
    {
        public string Id { get; set; } = string.Empty;

        [Required(ErrorMessage = "Username is required")]
        [StringLength(100)]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "English Full Name is required")]
        [StringLength(150)]
        public string FullNameEn { get; set; } = string.Empty;

        [Required(ErrorMessage = "Arabic Full Name is required")]
        [StringLength(150)]
        public string FullNameAr { get; set; } = string.Empty;

        [Required]
        public bool IsActive { get; set; } = true;

        [Required(ErrorMessage = "Role assignment is required")]
        public string Role { get; set; } = string.Empty;

        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters")]
        [DataType(DataType.Password)]
        public string? Password { get; set; }

        public string FullName { get; set; } = string.Empty;
    }

    public class ProjectDetailsDto
    {
        public ProjectDto Project { get; set; } = null!;
        public int TotalTrips { get; set; }
        public int CompletedTrips { get; set; }
        public int ActiveTrips { get; set; }
        public int CancelledTrips { get; set; }
        public double CompletionPercentage { get; set; }
        public List<TripDto> Trips { get; set; } = new List<TripDto>();
    }
}
