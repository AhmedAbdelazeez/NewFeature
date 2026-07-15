using System;
using System.ComponentModel.DataAnnotations;

namespace NewFeature.Models
{
    public class FinanceTransaction
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "English Description is required")]
        [StringLength(200, ErrorMessage = "English Description cannot exceed 200 characters")]
        public string DescriptionEn { get; set; } = string.Empty;

        [Required(ErrorMessage = "Arabic Description is required")]
        [StringLength(200, ErrorMessage = "Arabic Description cannot exceed 200 characters")]
        public string DescriptionAr { get; set; } = string.Empty;

        [Required(ErrorMessage = "Amount is required")]
        [Range(0.01, 1000000000.0, ErrorMessage = "Amount must be greater than 0")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "Type is required")]
        [StringLength(50, ErrorMessage = "Type cannot exceed 50 characters")]
        public string Type { get; set; } = "Revenue"; // Revenue, Expense, Asset, Liability, Receivables, Payables

        [Required(ErrorMessage = "Date is required")]
        public DateTime Date { get; set; } = DateTime.UtcNow;

        [Required(ErrorMessage = "English Category is required")]
        [StringLength(100, ErrorMessage = "English Category cannot exceed 100 characters")]
        public string CategoryEn { get; set; } = string.Empty;

        [Required(ErrorMessage = "Arabic Category is required")]
        [StringLength(100, ErrorMessage = "Arabic Category cannot exceed 100 characters")]
        public string CategoryAr { get; set; } = string.Empty;
    }
}
