using System.ComponentModel.DataAnnotations;

namespace NewFeature.Models
{
    public class ItSystem
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "English Name is required")]
        [StringLength(150, ErrorMessage = "English Name cannot exceed 150 characters")]
        public string NameEn { get; set; } = string.Empty;

        [Required(ErrorMessage = "Arabic Name is required")]
        [StringLength(150, ErrorMessage = "Arabic Name cannot exceed 150 characters")]
        public string NameAr { get; set; } = string.Empty;

        [Range(0, 100, ErrorMessage = "Uptime must be between 0 and 100")]
        public double UptimePercentage { get; set; } = 100.0;

        public bool LastBackupStatus { get; set; } = true;

        [Required(ErrorMessage = "Status is required")]
        [StringLength(50, ErrorMessage = "Status cannot exceed 50 characters")]
        public string Status { get; set; } = "Active"; // Active, Maintenance, Inactive

        public bool IsAutomated { get; set; } = false;
    }
}
