using System;
using System.ComponentModel.DataAnnotations;

namespace NewFeature.Models
{
    public class ImprovementAction
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

        [Required(ErrorMessage = "Department is required")]
        public int DepartmentId { get; set; }

        [Required(ErrorMessage = "Status is required")]
        public ImprovementStatus Status { get; set; } = ImprovementStatus.Proposed;

        [Required(ErrorMessage = "Date Proposed is required")]
        [DataType(DataType.Date)]
        public DateTime DateProposed { get; set; }

        [DataType(DataType.Date)]
        public DateTime? DateImplemented { get; set; }

        // Navigation property
        public Department? Department { get; set; }
    }
}
