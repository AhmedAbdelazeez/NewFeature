using System;
using System.ComponentModel.DataAnnotations;

namespace NewFeature.Models
{
    // One row per route/service scheduling request from the real dispatch-planning sheet
    // (جدولة الخطوط.xlsx). Each request is marked scheduled ("1"/a bus-count number) or unscheduled
    // ("غير مجدول") in the "الجدولة" column - this is what SchedulingSuccessRatePercent is computed
    // from. Point-in-time snapshot, replaced entirely on each successful upload.
    public class RouteScheduleRequest
    {
        public int Id { get; set; }

        public DateTime? ExecutionDate { get; set; } // "تاريخ التنفيذ"

        [StringLength(50)]
        public string? RentalOrderNumber { get; set; } // "رقم أمر الايجار"

        [StringLength(200)]
        public string? ClientName { get; set; } // "أسم العميل"

        [StringLength(200)]
        public string? ServiceName { get; set; } // "اسم الصنف"

        [StringLength(100)]
        public string? Location { get; set; } // "مكان التشغيل"

        // Raw text of "الجدولة" - a number when scheduled, "غير مجدول" when not.
        [StringLength(50)]
        public string? ScheduleStatusRaw { get; set; }

        public bool IsScheduled { get; set; }

        public DateTime SnapshotDate { get; set; } = DateTime.UtcNow;
    }
}
