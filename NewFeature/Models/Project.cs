using System.ComponentModel.DataAnnotations;

namespace NewFeature.Models
{
    public class Project
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Client selection is required")]
        public int ClientId { get; set; }

        [Required(ErrorMessage = "English Name is required")]
        [StringLength(150, ErrorMessage = "English Name cannot exceed 150 characters")]
        public string NameEn { get; set; } = string.Empty;

        [Required(ErrorMessage = "Arabic Name is required")]
        [StringLength(150, ErrorMessage = "Arabic Name cannot exceed 150 characters")]
        public string NameAr { get; set; } = string.Empty;

        [Required(ErrorMessage = "English Description is required")]
        [StringLength(1000, ErrorMessage = "English Description cannot exceed 1000 characters")]
        public string DescriptionEn { get; set; } = string.Empty;

        [Required(ErrorMessage = "Arabic Description is required")]
        [StringLength(1000, ErrorMessage = "Arabic Description cannot exceed 1000 characters")]
        public string DescriptionAr { get; set; } = string.Empty;

        [Required(ErrorMessage = "Start Date is required")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "End Date is required")]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }

        [Required(ErrorMessage = "Project Status is required")]
        public ProjectStatus Status { get; set; } = ProjectStatus.Planning;

        [Required(ErrorMessage = "Contract Value / Price is required")]
        public decimal ContractValue { get; set; }

        [Required(ErrorMessage = "Required Vehicles count is required")]
        public int RequiredVehiclesCount { get; set; }

        [Required(ErrorMessage = "Estimated Trips count is required")]
        public int EstimatedTripsCount { get; set; }

        // Navigation properties
        public Client? Client { get; set; }
        public ICollection<ProjectMember> Members { get; set; } = new List<ProjectMember>();
        public ICollection<ProjectMilestone> Milestones { get; set; } = new List<ProjectMilestone>();
        public ICollection<Task> Tasks { get; set; } = new List<Task>();
        public ICollection<Trip> Trips { get; set; } = new List<Trip>();
    }
}
