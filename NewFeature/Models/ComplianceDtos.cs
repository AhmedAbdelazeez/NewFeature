using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NewFeature.Models
{
    public class DepartmentDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "English Name is required")]
        [StringLength(150)]
        public string NameEn { get; set; } = string.Empty;

        [Required(ErrorMessage = "Arabic Name is required")]
        [StringLength(150)]
        public string NameAr { get; set; } = string.Empty;

        [Required(ErrorMessage = "Department Code is required")]
        [StringLength(20)]
        public string Code { get; set; } = string.Empty;

        [Required]
        public bool IsCompliant { get; set; } = true;

        public string Name { get; set; } = string.Empty;
    }

    public class ViolationClassificationDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "English Name is required")]
        [StringLength(150)]
        public string NameEn { get; set; } = string.Empty;

        [Required(ErrorMessage = "Arabic Name is required")]
        [StringLength(150)]
        public string NameAr { get; set; } = string.Empty;

        [Required(ErrorMessage = "Classification Code is required")]
        [StringLength(20)]
        public string Code { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;
    }

    public class ViolationDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "English Title is required")]
        [StringLength(200)]
        public string TitleEn { get; set; } = string.Empty;

        [Required(ErrorMessage = "Arabic Title is required")]
        [StringLength(200)]
        public string TitleAr { get; set; } = string.Empty;

        [Required(ErrorMessage = "English Description is required")]
        [StringLength(1000)]
        public string DescriptionEn { get; set; } = string.Empty;

        [Required(ErrorMessage = "Arabic Description is required")]
        [StringLength(1000)]
        public string DescriptionAr { get; set; } = string.Empty;

        [Required(ErrorMessage = "Detection Date is required")]
        public DateTime DetectionDate { get; set; }

        public DateTime? ResolutionDate { get; set; }

        [Required(ErrorMessage = "Violation Status is required")]
        public ViolationStatus Status { get; set; }

        [Required(ErrorMessage = "Violation Severity is required")]
        public ViolationSeverity Severity { get; set; }

        [Required(ErrorMessage = "Classification is required")]
        public int ClassificationId { get; set; }
        public string ClassificationName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Department is required")]
        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Fine Amount is required")]
        [Range(0.0, double.MaxValue, ErrorMessage = "Fine Amount must be non-negative")]
        public decimal FineAmount { get; set; }

        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    public class InternalAuditDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "English Title is required")]
        [StringLength(200)]
        public string TitleEn { get; set; } = string.Empty;

        [Required(ErrorMessage = "Arabic Title is required")]
        [StringLength(200)]
        public string TitleAr { get; set; } = string.Empty;

        [Required(ErrorMessage = "Audit Date is required")]
        public DateTime AuditDate { get; set; }

        [Required(ErrorMessage = "Department is required")]
        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Total Controls Audited count is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Total Controls must be at least 1")]
        public int TotalControlsAudited { get; set; }

        [Required(ErrorMessage = "Passed Controls count is required")]
        [Range(0, int.MaxValue, ErrorMessage = "Passed Controls must be non-negative")]
        public int PassedControlsCount { get; set; }

        [Required(ErrorMessage = "Critical Findings count is required")]
        [Range(0, int.MaxValue, ErrorMessage = "Critical Findings must be non-negative")]
        public int CriticalFindingsCount { get; set; }

        public string Title { get; set; } = string.Empty;
    }

    public class ImprovementActionDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "English Title is required")]
        [StringLength(200)]
        public string TitleEn { get; set; } = string.Empty;

        [Required(ErrorMessage = "Arabic Title is required")]
        [StringLength(200)]
        public string TitleAr { get; set; } = string.Empty;

        [Required(ErrorMessage = "English Description is required")]
        [StringLength(1000)]
        public string DescriptionEn { get; set; } = string.Empty;

        [Required(ErrorMessage = "Arabic Description is required")]
        [StringLength(1000)]
        public string DescriptionAr { get; set; } = string.Empty;

        [Required(ErrorMessage = "Department is required")]
        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Status is required")]
        public ImprovementStatus Status { get; set; }

        [Required(ErrorMessage = "Date Proposed is required")]
        public DateTime DateProposed { get; set; }

        public DateTime? DateImplemented { get; set; }

        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    public class ComplianceKpisDto
    {
        // 2. Registered Violations Count
        public int RegisteredViolationsCountActual { get; set; }
        public int RegisteredViolationsCountTarget { get; set; } = 0;

        // 3. Violations Closure Rate
        public double ViolationsClosureRateActual { get; set; }
        public double ViolationsClosureRateTarget { get; set; } = 95.0; // 95%

        // 4. Average Violation Resolution Time
        public double AverageViolationResolutionTimeActual { get; set; }
        public double AverageViolationResolutionTimeTarget { get; set; } = 5.0; // Less than 5 days

        // 5. Contractual Compliance Rate
        public double ContractualComplianceRateActual { get; set; }
        public double ContractualComplianceRateTarget { get; set; } = 95.0; // 95%

        // 6. Policy & Procedure Adherence Rate
        public double PolicyAdherenceRateActual { get; set; }
        public double PolicyAdherenceRateTarget { get; set; } = 95.0; // 95%

        // 7. Internal Audit Passing Rate
        public double InternalAuditPassingRateActual { get; set; }
        public double InternalAuditPassingRateTarget { get; set; } = 90.0; // 90% or more

        // 8. Critical Audit Findings
        public int CriticalAuditFindingsActual { get; set; }
        public int CriticalAuditFindingsTarget { get; set; } = 0;

        // 9. Monthly Overall Compliance Index
        public double MonthlyOverallComplianceIndexActual { get; set; }
        public double MonthlyOverallComplianceIndexTarget { get; set; } = 95.0; // 95%

        // 10. Continuous Improvement Rate
        public double ContinuousImprovementRateActual { get; set; }
        public double ContinuousImprovementRateTarget { get; set; } = 90.0; // 90%

        // Chart Data
        public List<ChartDataPoint> ViolationsBySeverity { get; set; } = new();
        public List<ChartDataPoint> ViolationsByStatus { get; set; } = new();
        public List<ChartDataPoint> AuditsPassingByDepartment { get; set; } = new();
        public List<ChartDataPoint> ImprovementsByStatus { get; set; } = new();
    }
}
