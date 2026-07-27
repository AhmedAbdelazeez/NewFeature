using System;
using System.ComponentModel.DataAnnotations;

namespace NewFeature.Models
{
    public class MohuPilgrimGroup
    {
        [Key]
        public int Id { get; set; }
        [Required, MaxLength(100)]
        public string GroupNumber { get; set; } = string.Empty;
        [Required, MaxLength(100)]
        public string Nationality { get; set; } = string.Empty;
        [Required, MaxLength(50)]
        public string AgeGroup { get; set; } = "Adults"; // Adults, Seniors, Children
        [Required, MaxLength(50)]
        public string PackageCategory { get; set; } = "Mid-range"; // VIP, Mid-range, Economy
        public decimal PackagePrice { get; set; }
        public bool IsNewPilgrim { get; set; }
        public DateTime ArrivalDate { get; set; }
        [MaxLength(100)]
        public string ArrivalPort { get; set; } = string.Empty;
        public int PilgrimCount { get; set; }
    }

    public class MohuFeedback
    {
        [Key]
        public int Id { get; set; }
        public int MohuPilgrimGroupId { get; set; } // Can link to group or be standalone
        public int Rating { get; set; } = 5; // 1 to 5
        [MaxLength(50)]
        public string ServiceType { get; set; } = "General"; // Housing, Transport, Catering
        public bool HasComplaint { get; set; }
        [MaxLength(500)]
        public string? ComplaintDetails { get; set; }
        public int WaitingTimeMinutes { get; set; }
        public DateTime FeedbackDate { get; set; } = DateTime.UtcNow;
    }

    public class MohuViolationRecord
    {
        [Key]
        public int Id { get; set; }
        [Required, MaxLength(150)]
        public string ViolationType { get; set; } = string.Empty; // Health, Housing, Transport
        public decimal PenaltyAmount { get; set; }
        public bool IsClosed { get; set; }
        public int AffectedPilgrimsCount { get; set; }
        public int CommitteeEvaluationScore { get; set; } = 100; // 0 to 100
        public DateTime InspectionDate { get; set; } = DateTime.UtcNow;
    }

    public class MohuPermitLog
    {
        [Key]
        public int Id { get; set; }
        [Required, MaxLength(100)]
        public string PermitNumber { get; set; } = string.Empty;
        public bool IsEntryPortMatched { get; set; }
        public bool IsEntryDateMatched { get; set; }
        public bool IsHousingMatched { get; set; }
        public DateTime VerificationDate { get; set; } = DateTime.UtcNow;
    }
}
