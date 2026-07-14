using System;
using System.ComponentModel.DataAnnotations;

namespace NewFeature.Models
{
    public class StrategicGoal
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "English Title is required")]
        [StringLength(200, ErrorMessage = "English Title cannot exceed 200 characters")]
        public string TitleEn { get; set; } = string.Empty;

        [Required(ErrorMessage = "Arabic Title is required")]
        [StringLength(200, ErrorMessage = "Arabic Title cannot exceed 200 characters")]
        public string TitleAr { get; set; } = string.Empty;

        [Range(0, 100, ErrorMessage = "Weight must be between 0 and 100")]
        public double Weight { get; set; } = 10.0;

        [Range(0, 100, ErrorMessage = "Progress must be between 0 and 100")]
        public double Progress { get; set; } = 0.0;

        [Required(ErrorMessage = "Status is required")]
        [StringLength(50, ErrorMessage = "Status cannot exceed 50 characters")]
        public string Status { get; set; } = "NotStarted"; // NotStarted, OnTrack, Delayed, Completed

        [Required]
        public DateTime TargetDate { get; set; } = DateTime.UtcNow.AddYears(1);
    }
}
