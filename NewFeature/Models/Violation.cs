using System;
using System.ComponentModel.DataAnnotations;

namespace NewFeature.Models
{
    public class Violation
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "English Title is required")]
        [StringLength(200, ErrorMessage = "English Title cannot exceed 200 characters")]
        public string TitleEn { get; set; } = string.Empty;

        [Required(ErrorMessage = "Arabic Title is required")]
        [StringLength(200, ErrorMessage = "Arabic Title cannot exceed 200 characters")]
        public string TitleAr { get; set; } = string.Empty;

        [Required(ErrorMessage = "English Description is required")]
        [StringLength(1000, ErrorMessage = "English Description cannot exceed 1000 characters")]
        public string DescriptionEn { get; set; } = string.Empty;

        [Required(ErrorMessage = "Arabic Description is required")]
        [StringLength(1000, ErrorMessage = "Arabic Description cannot exceed 1000 characters")]
        public string DescriptionAr { get; set; } = string.Empty;

        [Required(ErrorMessage = "Detection Date is required")]
        [DataType(DataType.Date)]
        public DateTime DetectionDate { get; set; }

        [DataType(DataType.Date)]
        public DateTime? ResolutionDate { get; set; }

        [Required(ErrorMessage = "Violation Status is required")]
        public ViolationStatus Status { get; set; } = ViolationStatus.Open;

        [Required(ErrorMessage = "Violation Severity is required")]
        public ViolationSeverity Severity { get; set; } = ViolationSeverity.Minor;

        [Required(ErrorMessage = "Violation Classification is required")]
        public int ClassificationId { get; set; }

        [Required(ErrorMessage = "Department is required")]
        public int DepartmentId { get; set; }

        [Required(ErrorMessage = "Fine Amount is required")]
        [Range(0.0, double.MaxValue, ErrorMessage = "Fine Amount must be non-negative")]
        public decimal FineAmount { get; set; }

        // Navigation properties
        public ViolationClassification? Classification { get; set; }
        public Department? Department { get; set; }
    }
}
