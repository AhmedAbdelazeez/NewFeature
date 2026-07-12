using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NewFeature.Models
{
    public class OperationalAuditDto
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

        public string Title { get; set; } = string.Empty;
    }

    public class OperationalAuditKpisDto
    {
        // 1. Audit Execution Rate
        public double AuditExecutionRateActual { get; set; }
        public double AuditExecutionRateTarget { get; set; } = 90.0; // 90%

        // 2. Operational Compliance Rate
        public double OperationalComplianceRateActual { get; set; }
        public double OperationalComplianceRateTarget { get; set; } = 95.0; // 95%

        // 3. Total Audited Processes
        public int TotalAuditedProcessesActual { get; set; }
        public int TotalAuditedProcessesTarget { get; set; } = 100; // 100 processes

        // 4. Passed Processes Count
        public int PassedProcessesCountActual { get; set; }
        public int PassedProcessesCountTarget { get; set; } = 95;

        // 5. Critical Findings Count
        public int CriticalFindingsCountActual { get; set; }
        public int CriticalFindingsCountTarget { get; set; } = 0; // Target is 0 critical findings

        // 6. Recommendations Count
        public int RecommendationsCountActual { get; set; }
        public int RecommendationsCountTarget { get; set; } = 50;

        // 7. Average Risk Mitigation Rate
        public double RiskMitigationRateActual { get; set; }
        public double RiskMitigationRateTarget { get; set; } = 90.0; // 90%

        // Chart Data
        public List<ChartDataPoint> AuditsByStatus { get; set; } = new();
        public List<ChartDataPoint> ComplianceByDepartment { get; set; } = new();
    }
}
