using System;
using System.ComponentModel.DataAnnotations;

namespace NewFeature.Models
{
    public class MohuKpiDto
    {
        public int Id { get; set; }
        public string CategoryCode { get; set; } = string.Empty;
        public string KpiCode { get; set; } = string.Empty;
        public string NameAr { get; set; } = string.Empty;
        public string NameEn { get; set; } = string.Empty;
        public double ActualValue { get; set; }
        public double TargetValue { get; set; }
        public DateTime LastUpdated { get; set; }
    }

    public class UpdateMohuKpiDto
    {
        [Required]
        public double ActualValue { get; set; }
        [Required]
        public double TargetValue { get; set; }
    }
}
