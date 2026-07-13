using System;
using System.ComponentModel.DataAnnotations;

namespace NewFeature.Models
{
    public class HseIncidentDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "English Title is required")]
        public string TitleEn { get; set; } = string.Empty;

        [Required(ErrorMessage = "Arabic Title is required")]
        public string TitleAr { get; set; } = string.Empty;

        [Required(ErrorMessage = "Type is required")]
        public string Type { get; set; } = "NearMiss";
        public DateTime Date { get; set; }
        public string DescriptionEn { get; set; } = string.Empty;
        public string DescriptionAr { get; set; } = string.Empty;

        [Required(ErrorMessage = "Severity is required")]
        public string Severity { get; set; } = "Low";
        public string Location { get; set; } = string.Empty;
    }

    public class HseInspectionDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "English Title is required")]
        public string TitleEn { get; set; } = string.Empty;

        [Required(ErrorMessage = "Arabic Title is required")]
        public string TitleAr { get; set; } = string.Empty;
        public DateTime InspectionDate { get; set; }

        [Required(ErrorMessage = "Inspector Name is required")]
        public string InspectorName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Status is required")]
        public string Status { get; set; } = "Planned";
        public double ComplianceScore { get; set; } = 100.0;
        public double TrainingHours { get; set; } = 0.0;
    }

    public class HseKpisDto
    {
        public double LtifrActual { get; set; }
        public double LtifrTarget { get; set; }

        public int SeriousRoadAccidentsActual { get; set; }
        public int SeriousRoadAccidentsTarget { get; set; }

        public double RegulatoryComplianceRateActual { get; set; }
        public double RegulatoryComplianceRateTarget { get; set; }

        public double HseTrainingHoursActual { get; set; }
        public double HseTrainingHoursTarget { get; set; }

        public double SafetyInspectionsCompletionActual { get; set; }
        public double SafetyInspectionsCompletionTarget { get; set; }

        public int NearMissReportingActual { get; set; }
        public int NearMissReportingTarget { get; set; }

        public double WasteRecyclingRateActual { get; set; }
        public double WasteRecyclingRateTarget { get; set; }
    }
}
