using System;
using System.ComponentModel.DataAnnotations;

namespace NewFeature.Models
{
    public class ItTicketDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "English Title is required")]
        public string TitleEn { get; set; } = string.Empty;

        [Required(ErrorMessage = "Arabic Title is required")]
        public string TitleAr { get; set; } = string.Empty;
        public string DescriptionEn { get; set; } = string.Empty;
        public string DescriptionAr { get; set; } = string.Empty;

        [Required(ErrorMessage = "Status is required")]
        public string Status { get; set; } = "Open";

        [Required(ErrorMessage = "Priority is required")]
        public string Priority { get; set; } = "Medium";
        public DateTime CreatedAt { get; set; }
        public DateTime? ResolvedAt { get; set; }
        public int? UserSatisfaction { get; set; }
    }

    public class ItSystemDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "English Name is required")]
        public string NameEn { get; set; } = string.Empty;

        [Required(ErrorMessage = "Arabic Name is required")]
        public string NameAr { get; set; } = string.Empty;
        public double UptimePercentage { get; set; } = 100.0;
        public bool LastBackupStatus { get; set; } = true;

        [Required(ErrorMessage = "Status is required")]
        public string Status { get; set; } = "Active";
        public bool IsAutomated { get; set; } = false;
    }

    public class ItKpisDto
    {
        public double DigitalTransformationRateActual { get; set; }
        public double DigitalTransformationRateTarget { get; set; }

        public double SystemUptimeActual { get; set; }
        public double SystemUptimeTarget { get; set; }

        public double AvgTicketResolutionTimeActual { get; set; }
        public double AvgTicketResolutionTimeTarget { get; set; }

        public int CybersecurityIncidentsActual { get; set; }
        public int CybersecurityIncidentsTarget { get; set; }

        public double UserSatisfactionActual { get; set; }
        public double UserSatisfactionTarget { get; set; }

        public double BackupSuccessRateActual { get; set; }
        public double BackupSuccessRateTarget { get; set; }

        public double ItProjectDeliveryActual { get; set; }
        public double ItProjectDeliveryTarget { get; set; }
    }
}
