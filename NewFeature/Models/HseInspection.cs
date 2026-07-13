using System;
using System.ComponentModel.DataAnnotations;

namespace NewFeature.Models
{
    public class HseInspection
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "English Title is required")]
        [StringLength(200, ErrorMessage = "English Title cannot exceed 200 characters")]
        public string TitleEn { get; set; } = string.Empty;

        [Required(ErrorMessage = "Arabic Title is required")]
        [StringLength(200, ErrorMessage = "Arabic Title cannot exceed 200 characters")]
        public string TitleAr { get; set; } = string.Empty;

        [Required]
        public DateTime InspectionDate { get; set; } = DateTime.UtcNow;

        [Required(ErrorMessage = "Inspector Name is required")]
        [StringLength(150, ErrorMessage = "Inspector Name cannot exceed 150 characters")]
        public string InspectorName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Status is required")]
        [StringLength(50, ErrorMessage = "Status cannot exceed 50 characters")]
        public string Status { get; set; } = "Planned"; // Planned, Completed, Cancelled

        [Range(0, 100, ErrorMessage = "Compliance Score must be between 0 and 100")]
        public double ComplianceScore { get; set; } = 100.0;

        [Range(0, 1000, ErrorMessage = "Training Hours must be positive")]
        public double TrainingHours { get; set; } = 0.0;
    }
}
