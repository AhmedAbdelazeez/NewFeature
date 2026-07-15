using System;
using System.ComponentModel.DataAnnotations;

namespace NewFeature.Models
{
    public class CommercialContract
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "English Client Name is required")]
        [StringLength(100, ErrorMessage = "English Client Name cannot exceed 100 characters")]
        public string ClientNameEn { get; set; } = string.Empty;

        [Required(ErrorMessage = "Arabic Client Name is required")]
        [StringLength(100, ErrorMessage = "Arabic Client Name cannot exceed 100 characters")]
        public string ClientNameAr { get; set; } = string.Empty;

        [Required(ErrorMessage = "Contract Number is required")]
        [StringLength(50, ErrorMessage = "Contract Number cannot exceed 50 characters")]
        public string ContractNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Contract Value is required")]
        [Range(0.01, 1000000000.0, ErrorMessage = "Contract Value must be greater than 0")]
        public decimal Value { get; set; }

        [Required(ErrorMessage = "Start Date is required")]
        public DateTime StartDate { get; set; } = DateTime.UtcNow;

        [Required(ErrorMessage = "End Date is required")]
        public DateTime EndDate { get; set; } = DateTime.UtcNow.AddYears(1);

        [Required(ErrorMessage = "Preparation Date is required")]
        public DateTime PreparationDate { get; set; } = DateTime.UtcNow;

        public DateTime? ActiveDate { get; set; }

        [Required(ErrorMessage = "Status is required")]
        [StringLength(50, ErrorMessage = "Status cannot exceed 50 characters")]
        public string Status { get; set; } = "Active"; // Active, Pending, Expired, Renewed

        public bool IsDisputed { get; set; } = false;
    }
}
