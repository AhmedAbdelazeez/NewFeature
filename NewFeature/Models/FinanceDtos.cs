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

    // One account of the uploaded chart of accounts (COA sheet).
    public class ChartOfAccountDto
    {
        public int Id { get; set; }
        public string AccountNumber { get; set; } = string.Empty;
        public string AccountName { get; set; } = string.Empty;
        public string? Mapping { get; set; }
        public string? BsClassification { get; set; }
        public string? IsClassification { get; set; }
        public string RsmClassification { get; set; } = string.Empty;
        public string? ManagementClassification { get; set; }
        public string? RevenueMainClassification { get; set; }
        public string? RevenueSubClassification { get; set; }
    }

    // One account balance as of one reporting date (الحركات المالية sheet), carrying the account's
    // classification alongside it so the page's table can be read without a second lookup.
    public class FinanceAccountBalanceDto
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public string AccountNumber { get; set; } = string.Empty;
        public string AccountName { get; set; } = string.Empty;
        // Nullable so a missing figure can be told apart from a genuine zero balance.
        public decimal? Balance { get; set; }
        public string? Classification { get; set; }
    }

    // Ten indicators, every one of them a sum over the uploaded balances grouped by the account's
    // own COA classification, reported for the latest reporting date on file. EBITDA, return on
    // assets, operating cash flow, working capital and budget variance were removed: they need
    // depreciation, cash-flow and budget data the approved template does not carry, and were
    // previously being derived from a hard-coded assumed asset base.
    public class FinanceKpisDto
    {
        // Which reporting date these figures describe. Null when nothing has been uploaded yet -
        // a trial balance is a snapshot, so the KPIs always describe one date rather than the sum
        // of every month ever loaded.
        public DateTime? AsOfDate { get; set; }

        public decimal TotalRevenue { get; set; }
        public decimal CostOfSales { get; set; }
        public decimal GrossProfit { get; set; }
        public double GrossProfitMarginPercent { get; set; }
        public decimal OperatingExpenses { get; set; }
        public decimal TotalExpenses { get; set; }
        public decimal NetProfit { get; set; }
        public double NetProfitMarginPercent { get; set; }
        public double ExpenseToRevenueRatioPercent { get; set; }
        public decimal CashAndEquivalents { get; set; }
        public decimal TradeReceivables { get; set; }
        public decimal TotalAssets { get; set; }
        public decimal TotalLiabilities { get; set; }

        public string TopExpenseCategoryName { get; set; } = "--";
        public decimal TopExpenseCategoryAmount { get; set; }

        // Coverage figures, so an empty dashboard can be told apart from a dashboard whose upload
        // simply hasn't happened yet.
        public int AccountsInChart { get; set; }
        public int BalancesLoaded { get; set; }
        public int UnclassifiedBalances { get; set; }
    }
}
