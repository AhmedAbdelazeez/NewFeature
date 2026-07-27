using System;
using System.ComponentModel.DataAnnotations;

namespace NewFeature.Models
{
    public class MohuStandardKpi
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [MaxLength(50)]
        public string CategoryCode { get; set; } = string.Empty; // Diversity, Experience, Compliance, DataQuality
        
        [Required]
        [MaxLength(20)]
        public string KpiCode { get; set; } = string.Empty; // e.g. M-DIV-01
        
        [Required]
        [MaxLength(200)]
        public string NameAr { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(200)]
        public string NameEn { get; set; } = string.Empty;
        
        public double ActualValue { get; set; }
        public double TargetValue { get; set; }
        
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    }
}
