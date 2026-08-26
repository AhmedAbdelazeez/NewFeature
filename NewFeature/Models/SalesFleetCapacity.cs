using System;
using System.ComponentModel.DataAnnotations;

namespace NewFeature.Models
{
    // One row per bus type/category/model combination from the fleet capacity workbook
    // (عدد وانواع الحافلات.xlsx). Fleet capacity is treated as a point-in-time snapshot rather than
    // a running ledger: each successful upload replaces the previous snapshot's rows (see
    // SalesService.BulkUploadFleetCapacityAsync), so KPIs always read the most recent SnapshotDate.
    public class SalesFleetCapacity
    {
        public int Id { get; set; }

        [StringLength(100)]
        public string? BusCode { get; set; } // "الحافلة" e.g. "2027-كوتش-K"

        [Required, StringLength(100)]
        public string BusType { get; set; } = string.Empty; // "نوع الحافلة" (manufacturer), e.g. كينج لونج

        [Required, StringLength(100)]
        public string Category { get; set; } = string.Empty; // "الفئة" e.g. كوتش / VIP / كوستر / سيتي

        public int? ModelYear { get; set; } // "الموديل"

        [Required]
        public int NumberOfBuses { get; set; } // "عدد الحافلات"

        [Required]
        public int SeatsPerBus { get; set; } // "عدد المقاعد"

        [Required]
        public int TotalSeats { get; set; } // "اجمالي المقاعد"

        public DateTime SnapshotDate { get; set; } = DateTime.UtcNow;

        public int SalesImportBatchId { get; set; }
        public SalesImportBatch? ImportBatch { get; set; }
    }
}
