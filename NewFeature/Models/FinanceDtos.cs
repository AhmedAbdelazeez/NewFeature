using System;
using System.Collections.Generic;
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

    // One account's figures for one reporting date in one branch (الحركات المالية sheet), carrying
    // the account's own classifications alongside them so the page's table can be read, filtered
    // and understood without a second lookup.
    public class FinanceAccountBalanceDto
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public string? Branch { get; set; }
        public string AccountNumber { get; set; } = string.Empty;
        public string AccountName { get; set; } = string.Empty;
        public decimal OpeningBalance { get; set; }
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
        // Nullable so a missing figure can be told apart from a genuine zero balance. Left blank
        // on the way in, it is computed as OpeningBalance + Debit - Credit.
        public decimal? Balance { get; set; }
        // Read-only, computed: the period's own activity with the opening balance excluded.
        public decimal Movement { get; set; }
        public string? Classification { get; set; }
        public string? Mapping { get; set; }
        public string? ManagementClassification { get; set; }
    }

    // What the شجرة الحسابات page's dropdowns narrow the tree by. Every unset property means "any".
    public class ChartOfAccountFilter
    {
        public string? Mapping { get; set; }
        public string? BsClassification { get; set; }
        public string? IsClassification { get; set; }
        public string? RsmClassification { get; set; }
        public string? ManagementClassification { get; set; }
        public string? RevenueMainClassification { get; set; }
        public string? RevenueSubClassification { get; set; }
    }

    // What the الأرصدة page's dropdowns narrow the monthly figures by. The classification filters
    // are applied through the account tree, so "show me every cash balance in فرع 1 in September"
    // is one request rather than a manual cross-reference.
    public class FinanceBalanceFilter
    {
        public string? Branch { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string? Mapping { get; set; }
        public string? RsmClassification { get; set; }
        public string? ManagementClassification { get; set; }
    }

    // The distinct values actually present in the uploaded data, so a page's filter dropdowns list
    // what can be found rather than a hard-coded guess that drifts from the file.
    public class FinanceAccountFilterOptionsDto
    {
        public List<string> Mappings { get; set; } = new();
        public List<string> BsClassifications { get; set; } = new();
        public List<string> IsClassifications { get; set; } = new();
        public List<string> RsmClassifications { get; set; } = new();
        public List<string> ManagementClassifications { get; set; } = new();
        public List<string> RevenueMainClassifications { get; set; } = new();
        public List<string> RevenueSubClassifications { get; set; } = new();
    }

    public class FinanceBalanceFilterOptionsDto
    {
        public List<string> Branches { get; set; } = new();
        public List<string> Mappings { get; set; } = new();
        public List<string> RsmClassifications { get; set; } = new();
        public List<string> ManagementClassifications { get; set; } = new();
        // Newest first: the reporting dates on file, for "show me September".
        public List<DateTime> Dates { get; set; } = new();
    }

    // The ten indicators the executive dashboard shows, every one of them a sum over the uploaded
    // figures grouped by the account's own COA classification. Two different periods are reported
    // side by side on purpose, because an income statement and a balance sheet do not describe the
    // same thing: the profitability figures accumulate every month uploaded for the latest year
    // (year to date), while the cash, receivables and payables figures are the closing position at
    // the latest reporting date. Nothing here assumes an asset base, a depreciation figure or a
    // target the uploaded file doesn't carry.
    public class FinanceKpisDto
    {
        // The latest reporting date on file; the balance-sheet figures describe this date. Null
        // when nothing has been uploaded yet.
        public DateTime? AsOfDate { get; set; }

        // The financial year the profitability figures accumulate over, and the first month of it
        // that was actually uploaded - so "year to date" can be read honestly even when only part
        // of the year is in.
        public int? FiscalYear { get; set; }
        public DateTime? PeriodFrom { get; set; }

        // ── Income statement, year to date ──
        public decimal TotalRevenue { get; set; }
        public decimal CostOfSales { get; set; }
        public decimal GrossProfit { get; set; }
        public double GrossProfitMarginPercent { get; set; }
        public decimal OperatingExpenses { get; set; }
        public decimal TotalExpenses { get; set; }
        public decimal NetProfit { get; set; }
        public double NetProfitMarginPercent { get; set; }
        public double ExpenseToRevenueRatioPercent { get; set; }

        // ── Balance sheet, at AsOfDate ──
        public decimal CashAndEquivalents { get; set; }
        public decimal TradeReceivables { get; set; }
        // Trade payables plus accrued expenses and other liabilities: what the company owes on the
        // two Mapping groups a finance manager watches week to week.
        public decimal PayablesAndAccruals { get; set; }
        public decimal TotalAssets { get; set; }
        public decimal TotalLiabilities { get; set; }

        // ── Where the money comes from and goes to ──
        // Biggest revenue stream by Revenues Main Classification (الحج، العمرة، الباطن، العقود).
        public string TopRevenueStreamName { get; set; } = "--";
        public decimal TopRevenueStreamAmount { get; set; }
        // Biggest cost block by Management Classification (تشغيلية، موارد بشرية، استهلاك...).
        public string TopExpenseCategoryName { get; set; } = "--";
        public decimal TopExpenseCategoryAmount { get; set; }

        // ── Coverage and data quality, so an empty dashboard can be told apart from one whose
        // upload simply hasn't happened yet, and so miscoded accounts surface instead of quietly
        // shrinking a KPI ──
        public int AccountsInChart { get; set; }
        public int BalancesLoaded { get; set; }
        // Rows whose account number is not in the chart of accounts at all.
        public int UnclassifiedBalances { get; set; }
        // Expense value sitting on accounts with no Management Classification: real money that
        // cannot be attributed to a cost block.
        public decimal UnclassifiedExpenseAmount { get; set; }
        // Which branches the figures above cover.
        public List<string> Branches { get; set; } = new();
    }
}
