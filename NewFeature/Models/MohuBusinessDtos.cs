using System;

namespace NewFeature.Models
{
    public class MohuPilgrimGroupDto
    {
        public int Id { get; set; }
        public string GroupNumber { get; set; } = string.Empty;
        public string Nationality { get; set; } = string.Empty;
        public string AgeGroup { get; set; } = string.Empty;
        public string PackageCategory { get; set; } = string.Empty;
        public decimal PackagePrice { get; set; }
        public bool IsNewPilgrim { get; set; }
        public DateTime ArrivalDate { get; set; }
        public string ArrivalPort { get; set; } = string.Empty;
        public int PilgrimCount { get; set; }
    }

    public class MohuFeedbackDto
    {
        public int Id { get; set; }
        public int MohuPilgrimGroupId { get; set; }
        public int Rating { get; set; }
        public string ServiceType { get; set; } = string.Empty;
        public bool HasComplaint { get; set; }
        public string? ComplaintDetails { get; set; }
        public int WaitingTimeMinutes { get; set; }
        public DateTime FeedbackDate { get; set; }
    }

    public class MohuViolationRecordDto
    {
        public int Id { get; set; }
        public string ViolationType { get; set; } = string.Empty;
        public decimal PenaltyAmount { get; set; }
        public bool IsClosed { get; set; }
        public int AffectedPilgrimsCount { get; set; }
        public int CommitteeEvaluationScore { get; set; }
        public DateTime InspectionDate { get; set; }
    }

    public class MohuPermitLogDto
    {
        public int Id { get; set; }
        public string PermitNumber { get; set; } = string.Empty;
        public bool IsEntryPortMatched { get; set; }
        public bool IsEntryDateMatched { get; set; }
        public bool IsHousingMatched { get; set; }
        public DateTime VerificationDate { get; set; }
    }
}
