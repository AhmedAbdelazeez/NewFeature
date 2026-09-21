using System.ComponentModel.DataAnnotations;

namespace NewFeature.Models
{
    // One account from the COA sheet of the approved Finance template. This is reference data, not
    // a transaction: it is what turns a bare balance into a revenue, a cost or a balance-sheet
    // figure. The account number is the key the balances sheet joins on, so it is unique.
    public class ChartOfAccount
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "رقم الحساب مطلوب")]
        [StringLength(50)]
        public string AccountNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "اسم الحساب مطلوب")]
        [StringLength(250)]
        public string AccountName { get; set; } = string.Empty;

        [StringLength(200)]
        public string? Mapping { get; set; }

        [StringLength(200)]
        public string? BsClassification { get; set; }

        [StringLength(200)]
        public string? IsClassification { get; set; }

        // The classification every KPI bucket is derived from - its numeric prefix (5xxx assets,
        // 6xxx liabilities and equity, 7000 revenue, 71xx cost of sales, 72xx operating costs)
        // is the one consistent machine-readable signal in the whole sheet.
        [Required]
        [StringLength(200)]
        public string RsmClassification { get; set; } = string.Empty;

        [StringLength(200)]
        public string? ManagementClassification { get; set; }

        [StringLength(200)]
        public string? RevenueMainClassification { get; set; }

        [StringLength(200)]
        public string? RevenueSubClassification { get; set; }
    }
}
