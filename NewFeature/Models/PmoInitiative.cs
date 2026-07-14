using System;
using System.ComponentModel.DataAnnotations;

namespace NewFeature.Models
{
    public class PmoInitiative
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "English Title is required")]
        [StringLength(200, ErrorMessage = "English Title cannot exceed 200 characters")]
        public string TitleEn { get; set; } = string.Empty;

        [Required(ErrorMessage = "Arabic Title is required")]
        [StringLength(200, ErrorMessage = "Arabic Title cannot exceed 200 characters")]
        public string TitleAr { get; set; } = string.Empty;

        [Required(ErrorMessage = "Manager Name is required")]
        [StringLength(150, ErrorMessage = "Manager Name cannot exceed 150 characters")]
        public string ManagerName { get; set; } = string.Empty;

        [Range(0, 100, ErrorMessage = "Progress must be between 0 and 100")]
        public double Progress { get; set; } = 0.0;

        [Range(0, 100000000, ErrorMessage = "Budget must be positive")]
        public decimal Budget { get; set; }

        [Required(ErrorMessage = "Status is required")]
        [StringLength(50, ErrorMessage = "Status cannot exceed 50 characters")]
        public string Status { get; set; } = "InProgress"; // InProgress, Delayed, Completed

        [Required]
        public DateTime StartDate { get; set; } = DateTime.UtcNow;

        [Required]
        public DateTime EndDate { get; set; } = DateTime.UtcNow.AddMonths(6);

        [Range(1, 5, ErrorMessage = "Maturity score must be between 1 and 5")]
        public double GovernanceMaturityScore { get; set; } = 3.0;

        public bool MilestoneOnTime { get; set; } = true;
    }
}
