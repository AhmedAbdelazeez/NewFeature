using System;
using System.ComponentModel.DataAnnotations;

namespace NewFeature.Models
{
    public class StrategicGoalDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "English Title is required")]
        public string TitleEn { get; set; } = string.Empty;

        [Required(ErrorMessage = "Arabic Title is required")]
        public string TitleAr { get; set; } = string.Empty;

        [Range(0, 100, ErrorMessage = "Weight must be between 0 and 100")]
        public double Weight { get; set; }

        [Range(0, 100, ErrorMessage = "Progress must be between 0 and 100")]
        public double Progress { get; set; }

        [Required(ErrorMessage = "Status is required")]
        public string Status { get; set; } = "NotStarted";

        [Required]
        public DateTime TargetDate { get; set; }
    }

    public class PmoInitiativeDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "English Title is required")]
        public string TitleEn { get; set; } = string.Empty;

        [Required(ErrorMessage = "Arabic Title is required")]
        public string TitleAr { get; set; } = string.Empty;

        [Required(ErrorMessage = "Manager Name is required")]
        public string ManagerName { get; set; } = string.Empty;

        [Range(0, 100, ErrorMessage = "Progress must be between 0 and 100")]
        public double Progress { get; set; }

        [Range(0, 100000000, ErrorMessage = "Budget must be positive")]
        public decimal Budget { get; set; }

        [Required(ErrorMessage = "Status is required")]
        public string Status { get; set; } = "InProgress";

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Range(1, 5, ErrorMessage = "Maturity score must be between 1 and 5")]
        public double GovernanceMaturityScore { get; set; }

        public bool MilestoneOnTime { get; set; }
    }

    public class StrategyKpisDto
    {
        public double StrategicGoalsAchievementActual { get; set; }
        public double StrategicGoalsAchievementTarget { get; set; }

        public double PmoInitiativeDeliveryActual { get; set; }
        public double PmoInitiativeDeliveryTarget { get; set; }

        public double RiskHandlingActual { get; set; }
        public double RiskHandlingTarget { get; set; }

        public double GovMaturityActual { get; set; }
        public double GovMaturityTarget { get; set; }

        public double StrategicGoalsAchieveMinedActual { get; set; }
        public double StrategicGoalsAchieveMinedTarget { get; set; }

        public double OnTimeMilestonesDeliveryActual { get; set; }
        public double OnTimeMilestonesDeliveryTarget { get; set; }

        public double StrategicBudgetEfficiencyActual { get; set; }
        public double StrategicBudgetEfficiencyTarget { get; set; }
    }
}
