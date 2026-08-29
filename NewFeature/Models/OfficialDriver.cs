using System;
using System.ComponentModel.DataAnnotations;

namespace NewFeature.Models
{
    // One row per officially registered driver from the company's compliance roster
    // (اسطول الحافلات - السائقين الرسميين.xlsx, sheet "السائقين الرسميين"). This is a point-in-time
    // snapshot of who is registered to drive, distinct from the ApplicationUser "driver" records
    // created on the fly from trip dispatch sheets - a new upload replaces the previous snapshot.
    public class OfficialDriver
    {
        public int Id { get; set; }

        [StringLength(50)]
        public string? EmployeeCode { get; set; } // "Employee" e.g. R100090

        [StringLength(50)]
        public string? IqamaNumber { get; set; } // "IqamaNoForBank"

        [Required, StringLength(200)]
        public string ArabicName { get; set; } = string.Empty;

        [StringLength(200)]
        public string? EnglishName { get; set; }

        [StringLength(100)]
        public string? Nationality { get; set; } // "Nationality Id" e.g. مصري / باكستانى

        public DateTime? LicenseExpiryDate { get; set; } // "تاريخ انتهاء الرخصة"

        [StringLength(300)]
        public string? Notes { get; set; } // free-text compliance notes e.g. "مشارك عمرة 2027"

        public DateTime SnapshotDate { get; set; } = DateTime.UtcNow;
    }
}
