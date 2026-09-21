using System;
using System.ComponentModel.DataAnnotations;

namespace NewFeature.Models
{
    // One row of the approved Operations dispatch log: one bus assigned to one rental order on one
    // day. Stored as its own flat record rather than being forced into Trip, because Trip requires
    // a Vehicle, Route and Identity-user foreign key for every row, and a monthly dispatch sheet
    // would manufacture thousands of placeholder vehicles, routes and user accounts that nobody
    // ever asked for. Every Operations KPI is a count, distinct-count or sum over these columns.
    public class OperationsDispatchRecord
    {
        public int Id { get; set; }

        // Direction + Rent Order + Bus number + delivery date is what makes a dispatch line unique:
        // the same rental order legitimately appears once per bus, and once per direction (a
        // morning and an evening leg), so all four are needed to re-import a corrected month
        // without duplicating rows.
        [Required(ErrorMessage = "كود الخط (Direction) مطلوب")]
        [StringLength(50)]
        public string Direction { get; set; } = string.Empty;

        [StringLength(200)]
        public string? DirectionName { get; set; }

        [Required(ErrorMessage = "أمر الإيجار (Rent Order) مطلوب")]
        [StringLength(50)]
        public string RentOrder { get; set; } = string.Empty;

        [StringLength(50)]
        public string? CustomerAccount { get; set; }

        [Required(ErrorMessage = "اسم العميل مطلوب")]
        [StringLength(250)]
        public string CustomerName { get; set; } = string.Empty;

        [StringLength(100)]
        public string? BusType { get; set; }

        [Required(ErrorMessage = "رقم الحافلة مطلوب")]
        [StringLength(50)]
        public string BusNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "تاريخ التنفيذ مطلوب")]
        public DateTime DeliveryDate { get; set; }

        public DateTime? PlannedStart { get; set; }
        public DateTime? PlannedEnd { get; set; }

        [StringLength(50)]
        public string? DriverNumber { get; set; }

        [Required(ErrorMessage = "اسم السائق مطلوب")]
        [StringLength(250)]
        public string DriverName { get; set; } = string.Empty;

        [StringLength(50)]
        public string? AdditionalDriverNumber { get; set; }

        [StringLength(250)]
        public string? AdditionalDriverName { get; set; }

        [StringLength(200)]
        public string? Location { get; set; }

        [StringLength(200)]
        public string? FromLocation { get; set; }

        [StringLength(200)]
        public string? ToLocation { get; set; }

        [Range(0, 1000000)]
        public double? ActualKm { get; set; }

        [Range(0, 1000000)]
        public double? PlannedKm { get; set; }

        [Range(0, 1000000)]
        public double? DieselLiters { get; set; }

        [Required(ErrorMessage = "حالة التنفيذ مطلوبة")]
        [StringLength(50)]
        public string Completion { get; set; } = string.Empty;

        // True when the Completion cell says the order was actually executed. Kept as a stored flag
        // so the completion-rate KPI never has to re-parse free text over the whole table.
        public bool IsCompleted { get; set; }
    }
}
