using System;
using System.ComponentModel.DataAnnotations;

namespace NewFeature.Models
{
    public class ProcurementRequest
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "English Title is required")]
        [StringLength(200, ErrorMessage = "English Title cannot exceed 200 characters")]
        public string TitleEn { get; set; } = string.Empty;

        [Required(ErrorMessage = "Arabic Title is required")]
        [StringLength(200, ErrorMessage = "Arabic Title cannot exceed 200 characters")]
        public string TitleAr { get; set; } = string.Empty;

        [Required(ErrorMessage = "Requester Name is required")]
        [StringLength(150, ErrorMessage = "Requester Name cannot exceed 150 characters")]
        public string RequesterName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Supplier Name is required")]
        [StringLength(150, ErrorMessage = "Supplier Name cannot exceed 150 characters")]
        public string SupplierName { get; set; } = string.Empty;

        [Range(0, 10000000, ErrorMessage = "Amount must be positive")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "Status is required")]
        [StringLength(50, ErrorMessage = "Status cannot exceed 50 characters")]
        public string Status { get; set; } = "Requested"; // Requested, Approved, Ordered, Received, Cancelled

        [Required]
        public DateTime RequestDate { get; set; } = DateTime.UtcNow;

        public DateTime? DeliveryDate { get; set; }

        public bool BudgetCompliant { get; set; } = true;
    }
}
