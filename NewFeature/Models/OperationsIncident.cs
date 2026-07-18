using System;
using System.ComponentModel.DataAnnotations;

namespace NewFeature.Models
{
    public class OperationsIncident
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string DescriptionAr { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string DescriptionEn { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Severity { get; set; } = "Medium"; // Low, Medium, High

        [Range(0, 1440)]
        public double ResponseTimeMinutes { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Open"; // Open, Resolved
    }
}
