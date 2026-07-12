using System;
using System.ComponentModel.DataAnnotations;

namespace NewFeature.Models
{
    public enum OperationalAuditStatus
    {
        Planned = 0,
        InProgress = 1,
        Completed = 2,
        FollowUp = 3
    }

    public class OperationalAudit
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

        [Required(ErrorMessage = "Audited Process Count is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Audited Process Count must be at least 1")]
        public int AuditedProcessCount { get; set; }

        [Required(ErrorMessage = "Passed Process Count is required")]
        [Range(0, int.MaxValue, ErrorMessage = "Passed Process Count must be non-negative")]
        public int PassedProcessCount { get; set; }

        [Required(ErrorMessage = "Critical Findings Count is required")]
        [Range(0, int.MaxValue, ErrorMessage = "Critical Findings Count must be non-negative")]
        public int CriticalFindingsCount { get; set; }

        [Required(ErrorMessage = "Recommendations Count is required")]
        [Range(0, int.MaxValue, ErrorMessage = "Recommendations Count must be non-negative")]
        public int RecommendationsCount { get; set; }

        [Required(ErrorMessage = "Risk Mitigation Rate is required")]
        [Range(0.0, 100.0, ErrorMessage = "Risk Mitigation Rate must be between 0 and 100")]
        public double RiskMitigationRate { get; set; }

        [Required(ErrorMessage = "Status is required")]
        public OperationalAuditStatus Status { get; set; }

        // Navigation property
        public Department? Department { get; set; }
    }
}
