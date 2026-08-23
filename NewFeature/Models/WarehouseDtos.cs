namespace NewFeature.Models
{
    // KPIs for the Storage / Warehouse department (إدارة التخزين), computed from InventoryItem records.
    public class WarehouseKpisDto
    {
        public int TotalItemsActual { get; set; }
        public int TotalItemsTarget { get; set; }

        public decimal TotalStockValueActual { get; set; }
        public decimal TotalStockValueTarget { get; set; }

        public int LowStockItemsActual { get; set; } // Quantity <= ReorderLevel
        public int LowStockItemsTarget { get; set; }

        public double InventoryAccuracyRateActual { get; set; } // % items with zero discrepancy
        public double InventoryAccuracyRateTarget { get; set; }

        public decimal AverageUnitPriceActual { get; set; }
        public decimal AverageUnitPriceTarget { get; set; }
    }
}
