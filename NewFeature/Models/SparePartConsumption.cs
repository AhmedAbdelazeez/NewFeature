using System.ComponentModel.DataAnnotations;

namespace NewFeature.Models
{
    public class SparePartConsumption
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Maintenance Work Order ID is required")]
        public int MaintenanceWorkOrderId { get; set; }

        [Required(ErrorMessage = "Part Name is required")]
        [StringLength(150)]
        public string PartName { get; set; } = string.Empty;

        [Range(1, 10000, ErrorMessage = "Quantity must be at least 1")]
        public int Quantity { get; set; }

        [Range(0.0, 1000000.0, ErrorMessage = "Unit Price must be positive")]
        public decimal UnitPrice { get; set; }

        public int? InventoryItemId { get; set; }

        // Navigation properties
        public MaintenanceWorkOrder? MaintenanceWorkOrder { get; set; }
        public InventoryItem? InventoryItem { get; set; }
    }
}
