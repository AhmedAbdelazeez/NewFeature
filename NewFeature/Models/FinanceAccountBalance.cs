using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NewFeature.Models
{
    // One account's figures for one reporting date in one branch, from the الحركات المالية
    // (trial balance) template. Date + Branch + AccountNumber is what identifies a row, so
    // re-uploading a corrected month overwrites that month's figures instead of double-counting
    // them, and the two branches never overwrite each other.
    public class FinanceAccountBalance
    {
        // What a row that arrives without a branch is filed under: the single-branch case, and
        // every balance uploaded before the branch column existed.
        public const string DefaultBranch = "فرع 1";

        public int Id { get; set; }

        [Required(ErrorMessage = "التاريخ مطلوب")]
        public DateTime Date { get; set; }

        // The branch (فرع) the figures belong to. Their own workbook keeps one sheet per branch;
        // here it is a column, so one upload can carry every branch and the KPIs can be read per
        // branch or for the company as a whole.
        [Required]
        [StringLength(100)]
        public string Branch { get; set; } = DefaultBranch;

        [Required(ErrorMessage = "رقم الحساب مطلوب")]
        [StringLength(50)]
        public string AccountNumber { get; set; } = string.Empty;

        [StringLength(250)]
        public string AccountName { get; set; } = string.Empty;

        // The opening balance the period starts from (BBF in their sheet). Zero for an income
        // statement account, which always starts a year at nil.
        public decimal OpeningBalance { get; set; }

        public decimal Debit { get; set; }

        public decimal Credit { get; set; }

        // Signed as the trial balance exports it: OpeningBalance + Debit - Credit, so a
        // credit-natured account (revenue, liability, equity) legitimately carries a negative
        // balance. The KPI layer flips the sign per the account's own classification rather than
        // assuming every figure is a positive magnitude.
        [Required(ErrorMessage = "الرصيد مطلوب")]
        public decimal Balance { get; set; }

        // The period's own activity, with the opening balance excluded. This is what the income
        // statement KPIs accumulate across months; the balance sheet KPIs read Balance instead.
        [NotMapped]
        public decimal Movement => Debit - Credit;
    }
}
