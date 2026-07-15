using System;
using System.ComponentModel.DataAnnotations;

namespace NewFeature.Models
{
    public class CommercialLead
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Lead Name is required")]
        [StringLength(100, ErrorMessage = "Lead Name cannot exceed 100 characters")]
        public string LeadName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Source is required")]
        [StringLength(100, ErrorMessage = "Source cannot exceed 100 characters")]
        public string Source { get; set; } = string.Empty; // Referral, Web, Campaign, Direct

        [Required(ErrorMessage = "Status is required")]
        [StringLength(50, ErrorMessage = "Status cannot exceed 50 characters")]
        public string Status { get; set; } = "New"; // New, InProgress, Won, Lost

        [Required(ErrorMessage = "Estimated Value is required")]
        [Range(0.0, 1000000000.0, ErrorMessage = "Estimated Value must be 0 or greater")]
        public decimal EstimatedValue { get; set; }

        [Required(ErrorMessage = "Acquisition Cost is required")]
        [Range(0.0, 10000000.0, ErrorMessage = "Acquisition Cost must be 0 or greater")]
        public decimal AcquisitionCost { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
