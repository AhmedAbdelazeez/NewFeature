using System.ComponentModel.DataAnnotations;

namespace NewFeature.Models
{
    public class TimeLog
    {
        public int Id { get; set; }

        [Required]
        public int TaskId { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required(ErrorMessage = "English description is required")]
        [StringLength(500, ErrorMessage = "English description cannot exceed 500 characters")]
        public string ActivityDescriptionEn { get; set; } = string.Empty;

        [Required(ErrorMessage = "Arabic description is required")]
        [StringLength(500, ErrorMessage = "Arabic description cannot exceed 500 characters")]
        public string ActivityDescriptionAr { get; set; } = string.Empty;

        [Required(ErrorMessage = "Start time is required")]
        public DateTime StartTime { get; set; }

        [Required(ErrorMessage = "End time is required")]
        public DateTime EndTime { get; set; }

        [Required(ErrorMessage = "Hours Logged is required")]
        [Range(0.01, 24.0, ErrorMessage = "Hours logged must be between 0.01 and 24 hours")]
        public decimal HoursLogged { get; set; }

        // Navigation properties
        public Task? Task { get; set; }
        public ApplicationUser? User { get; set; }
    }
}
