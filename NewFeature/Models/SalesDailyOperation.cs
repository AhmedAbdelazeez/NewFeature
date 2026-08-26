using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace NewFeature.Models
{
    // One row per rental/trip order from the daily operations log (يومية التشغيل.xlsx). Re-uploading
    // a day's log replaces that ExecutionDate's rows (see SalesService.BulkUploadDailyOperationsAsync)
    // so the same day can be corrected without double counting, while different days accumulate into
    // a genuine history as the Sales user uploads more logs over time.
    [Index(nameof(ExecutionDate))]
    public class SalesDailyOperation
    {
        public int Id { get; set; }

        [StringLength(50)]
        public string? RentalOrderNumber { get; set; } // رقم أمر الايجار

        [StringLength(50)]
        public string? ConfirmationNumber { get; set; } // رقم التعميد

        [StringLength(100)]
        public string? RequestType { get; set; } // طلب العميل (خارجي/داخلي)

        [Required, StringLength(300)]
        public string CustomerName { get; set; } = string.Empty; // أسم العميل

        [StringLength(200)]
        public string? ExecutionPoint { get; set; } // المنفذ

        [StringLength(300)]
        public string? Direction { get; set; } // الاتجاه (route description)

        [StringLength(500)]
        public string? Notes { get; set; } // ملاحظات

        [StringLength(50)]
        public string? BusTypeCode { get; set; } // نوع الحافلة

        public int OperationalCount { get; set; } // العدد التشغيلى
        public int ScheduledBuses { get; set; } // الحافلات المجدولة

        [Required]
        public DateTime ExecutionDate { get; set; } // تاريخ التنفيذ
        public TimeSpan? ExecutionTime { get; set; } // وقت التنفيذ

        public int SalesImportBatchId { get; set; }
        public SalesImportBatch? ImportBatch { get; set; }
    }
}
