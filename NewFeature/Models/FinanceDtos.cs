using System;
using System.ComponentModel.DataAnnotations;

namespace NewFeature.Models
{
    public class FinanceTransactionDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "English Description is required")]
        public string DescriptionEn { get; set; } = string.Empty;

        [Required(ErrorMessage = "Arabic Description is required")]
        public string DescriptionAr { get; set; } = string.Empty;

        [Required(ErrorMessage = "Amount is required")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "Type is required")]
        public string Type { get; set; } = "Revenue";

        [Required(ErrorMessage = "Date is required")]
        public DateTime Date { get; set; }

        [Required(ErrorMessage = "English Category is required")]
        public string CategoryEn { get; set; } = string.Empty;

        [Required(ErrorMessage = "Arabic Category is required")]
        public string CategoryAr { get; set; } = string.Empty;
    }

    public class FinanceBudgetDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "English Department Name is required")]
        public string DepartmentNameEn { get; set; } = string.Empty;

        [Required(ErrorMessage = "Arabic Department Name is required")]
        public string DepartmentNameAr { get; set; } = string.Empty;

        [Required(ErrorMessage = "Allocated Amount is required")]
        public decimal AllocatedAmount { get; set; }

        [Required(ErrorMessage = "Spent Amount is required")]
        public decimal SpentAmount { get; set; }

        [Required(ErrorMessage = "Year is required")]
        public int Year { get; set; }
    }

    public class FinanceKpisDto
    {
        public decimal TotalRevenueActual { get; set; }
        public decimal TotalRevenueTarget { get; set; }

        public double EbitdaMarginActual { get; set; }
        public double EbitdaMarginTarget { get; set; }

        public double NetProfitMarginActual { get; set; }
        public double NetProfitMarginTarget { get; set; }

        public decimal OperatingCashFlowActual { get; set; }
        public decimal OperatingCashFlowTarget { get; set; }

        public double ReturnOnAssetsActual { get; set; }
        public double ReturnOnAssetsTarget { get; set; }

        public double BudgetVarianceRateActual { get; set; }
        public double BudgetVarianceRateTarget { get; set; }

        public decimal WorkingCapitalActual { get; set; }
        public decimal WorkingCapitalTarget { get; set; }
    }
}
