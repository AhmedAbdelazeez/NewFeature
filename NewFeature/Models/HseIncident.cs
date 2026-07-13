using System;
using System.ComponentModel.DataAnnotations;

namespace NewFeature.Models
{
    public class HseIncident
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "English Title is required")]
        [StringLength(200, ErrorMessage = "English Title cannot exceed 200 characters")]
        public string TitleEn { get; set; } = string.Empty;

        [Required(ErrorMessage = "Arabic Title is required")]
        [StringLength(200, ErrorMessage = "Arabic Title cannot exceed 200 characters")]
        public string TitleAr { get; set; } = string.Empty;

        [Required(ErrorMessage = "Incident Type is required")]
        [StringLength(100, ErrorMessage = "Incident Type cannot exceed 100 characters")]
        public string Type { get; set; } = "NearMiss"; // Injury, SeriousAccident, NearMiss, Environmental

        [Required]
        public DateTime Date { get; set; } = DateTime.UtcNow;

        [StringLength(1000, ErrorMessage = "English Description cannot exceed 1000 characters")]
        public string DescriptionEn { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "Arabic Description cannot exceed 1000 characters")]
        public string DescriptionAr { get; set; } = string.Empty;

        [Required(ErrorMessage = "Severity is required")]
        [StringLength(50, ErrorMessage = "Severity cannot exceed 50 characters")]
        public string Severity { get; set; } = "Low"; // Low, Medium, High, Critical

        [StringLength(200, ErrorMessage = "Location cannot exceed 200 characters")]
        public string Location { get; set; } = string.Empty;
    }
}
