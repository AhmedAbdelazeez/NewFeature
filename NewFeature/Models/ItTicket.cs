using System;
using System.ComponentModel.DataAnnotations;

namespace NewFeature.Models
{
    public class ItTicket
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "English Title is required")]
        [StringLength(200, ErrorMessage = "English Title cannot exceed 200 characters")]
        public string TitleEn { get; set; } = string.Empty;

        [Required(ErrorMessage = "Arabic Title is required")]
        [StringLength(200, ErrorMessage = "Arabic Title cannot exceed 200 characters")]
        public string TitleAr { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "English Description cannot exceed 1000 characters")]
        public string DescriptionEn { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "Arabic Description cannot exceed 1000 characters")]
        public string DescriptionAr { get; set; } = string.Empty;

        [Required(ErrorMessage = "Status is required")]
        [StringLength(50, ErrorMessage = "Status cannot exceed 50 characters")]
        public string Status { get; set; } = "Open"; // Open, In Progress, Resolved

        [Required(ErrorMessage = "Priority is required")]
        [StringLength(50, ErrorMessage = "Priority cannot exceed 50 characters")]
        public string Priority { get; set; } = "Medium"; // Low, Medium, High, Critical

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ResolvedAt { get; set; }

        [Range(1, 5, ErrorMessage = "Satisfaction rating must be between 1 and 5")]
        public int? UserSatisfaction { get; set; }
    }
}
