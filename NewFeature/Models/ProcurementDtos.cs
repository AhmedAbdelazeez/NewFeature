using System;
using System.ComponentModel.DataAnnotations;

namespace NewFeature.Models
{
    public class ProcurementRequestDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "English Title is required")]
        public string TitleEn { get; set; } = string.Empty;

        [Required(ErrorMessage = "Arabic Title is required")]
        public string TitleAr { get; set; } = string.Empty;

        [Required(ErrorMessage = "Requester Name is required")]
        public string RequesterName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Supplier Name is required")]
        public string SupplierName { get; set; } = string.Empty;

        [Range(0, 10000000, ErrorMessage = "Amount must be positive")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "Status is required")]
        public string Status { get; set; } = "Requested";

        [Required]
        public DateTime RequestDate { get; set; }

        public DateTime? DeliveryDate { get; set; }

        public bool BudgetCompliant { get; set; }
    }

    public class InventoryItemDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "English Item Name is required")]
        public string ItemNameEn { get; set; } = string.Empty;

        [Required(ErrorMessage = "Arabic Item Name is required")]
        public string ItemNameAr { get; set; } = string.Empty;

        [Required(ErrorMessage = "Category is required")]
        public string Category { get; set; } = "Spare Parts";

        [Range(0, 100000, ErrorMessage = "Quantity must be positive")]
        public int Quantity { get; set; }

        [Range(0, 100000, ErrorMessage = "Reorder Level must be positive")]
        public int ReorderLevel { get; set; }

        [Range(0, 1000000, ErrorMessage = "Unit Price must be positive")]
        public decimal UnitPrice { get; set; }

        [Required]
        public DateTime LastAuditDate { get; set; }

        [Range(0, 1000, ErrorMessage = "Discrepancy count must be positive")]
        public int DiscrepancyCount { get; set; }
    }

    public class ProcurementKpisDto
    {
        public double AvgProcurementCycleTimeActual { get; set; }
        public double AvgProcurementCycleTimeTarget { get; set; }

        public double CostSavingsRateActual { get; set; }
        public double CostSavingsRateTarget { get; set; }

        public double SupplierPerformanceRatingActual { get; set; }
        public double SupplierPerformanceRatingTarget { get; set; }

        public double BudgetComplianceActual { get; set; }
        public double BudgetComplianceTarget { get; set; }

        public double CriticalSparePartsAvailabilityActual { get; set; }
        public double CriticalSparePartsAvailabilityTarget { get; set; }

        public double InventoryAccuracyRateActual { get; set; }
        public double InventoryAccuracyRateTarget { get; set; }

        public int ActiveSupplyContractsActual { get; set; }
        public int ActiveSupplyContractsTarget { get; set; }
    }
}
