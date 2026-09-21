using System;
using System.ComponentModel.DataAnnotations;

namespace NewFeature.Models
{
    // One account balance as of one reporting date, from the الحركات المالية sheet of the approved
    // Finance template. Date + AccountNumber is what identifies a balance, so re-uploading a
    // corrected month overwrites that month's figures instead of double-counting them.
    public class FinanceAccountBalance
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "التاريخ مطلوب")]
        public DateTime Date { get; set; }

        [Required(ErrorMessage = "رقم الحساب مطلوب")]
        [StringLength(50)]
        public string AccountNumber { get; set; } = string.Empty;

        [StringLength(250)]
        public string AccountName { get; set; } = string.Empty;

        // Signed as the trial balance exports it: a credit-natured account (revenue, liability,
        // equity) can legitimately arrive negative. The KPI layer takes magnitudes where a
        // direction is already implied by the account's own classification.
        [Required(ErrorMessage = "الرصيد مطلوب")]
        public decimal Balance { get; set; }
    }
}
