using System;
using System.ComponentModel.DataAnnotations;

namespace NewFeature.Models
{
    public class InventoryItem
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "English Item Name is required")]
        [StringLength(200, ErrorMessage = "English Item Name cannot exceed 200 characters")]
        public string ItemNameEn { get; set; } = string.Empty;

        [Required(ErrorMessage = "Arabic Item Name is required")]
        [StringLength(200, ErrorMessage = "Arabic Item Name cannot exceed 200 characters")]
        public string ItemNameAr { get; set; } = string.Empty;

        [Required(ErrorMessage = "Category is required")]
        [StringLength(100, ErrorMessage = "Category cannot exceed 100 characters")]
        public string Category { get; set; } = "Spare Parts"; // Spare Parts, Office Supply, Tech Hardware

        [Range(0, 100000, ErrorMessage = "Quantity must be positive")]
        public int Quantity { get; set; }

        [Range(0, 100000, ErrorMessage = "Reorder Level must be positive")]
        public int ReorderLevel { get; set; }

        [Range(0, 1000000, ErrorMessage = "Unit Price must be positive")]
        public decimal UnitPrice { get; set; }

        [Required]
        public DateTime LastAuditDate { get; set; } = DateTime.UtcNow;

        [Range(0, 1000, ErrorMessage = "Discrepancy count must be positive")]
        public int DiscrepancyCount { get; set; } = 0;
    }
}
