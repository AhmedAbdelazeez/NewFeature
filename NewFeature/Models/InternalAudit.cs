using System;
using System.ComponentModel.DataAnnotations;

namespace NewFeature.Models
{
    public class InternalAudit
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "English Title is required")]
        [StringLength(200, ErrorMessage = "English Title cannot exceed 200 characters")]
        public string TitleEn { get; set; } = string.Empty;

        [Required(ErrorMessage = "Arabic Title is required")]
        [StringLength(200, ErrorMessage = "Arabic Title cannot exceed 200 characters")]
        public string TitleAr { get; set; } = string.Empty;

        [Required(ErrorMessage = "Audit Date is required")]
        [DataType(DataType.Date)]
        public DateTime AuditDate { get; set; }

        [Required(ErrorMessage = "Department is required")]
        public int DepartmentId { get; set; }

        [Required(ErrorMessage = "Total Controls Audited count is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Total Controls must be at least 1")]
        public int TotalControlsAudited { get; set; }

        [Required(ErrorMessage = "Passed Controls count is required")]
        [Range(0, int.MaxValue, ErrorMessage = "Passed Controls must be non-negative")]
        public int PassedControlsCount { get; set; }

        [Required(ErrorMessage = "Critical Findings count is required")]
        [Range(0, int.MaxValue, ErrorMessage = "Critical Findings must be non-negative")]
        public int CriticalFindingsCount { get; set; }

        // Navigation property
        public Department? Department { get; set; }
    }
}
