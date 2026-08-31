using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NewFeature.Models;
using Microsoft.Extensions.Logging;
using NewFeature.Services.ExcelImport;
using NewFeature.Services.Repositories;

namespace NewFeature.Services
{
    public class FinanceService : IFinanceService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<FinanceService> _logger;

        public FinanceService(ApplicationDbContext context, ILogger<FinanceService> logger)
        {
            _context = context;
            _logger = logger;
        }

        #region Transactions CRUD
        public async Task<IEnumerable<FinanceTransactionDto>> GetAllTransactionsAsync()
        {
            var items = await _context.FinanceTransactions.OrderByDescending(t => t.Date).ToListAsync();
            return items.Select(MapToTransactionDto);
        }

        public async Task<FinanceTransactionDto?> GetTransactionByIdAsync(int id)
        {
            var item = await _context.FinanceTransactions.FindAsync(id);
            if (item == null) return null;
            return MapToTransactionDto(item);
        }

        public async Task<FinanceTransactionDto> CreateTransactionAsync(FinanceTransactionDto dto)
        {
            var item = new FinanceTransaction
            {
                DescriptionEn = dto.DescriptionEn,
                DescriptionAr = dto.DescriptionAr,
                Amount = dto.Amount,
                Type = dto.Type,
                Date = dto.Date == default ? DateTime.UtcNow : dto.Date,
                CategoryEn = dto.CategoryEn,
                CategoryAr = dto.CategoryAr
            };

            _context.FinanceTransactions.Add(item);
            await _context.SaveChangesAsync();
            return MapToTransactionDto(item);
        }

        public async Task<bool> UpdateTransactionAsync(FinanceTransactionDto dto)
        {
            var item = await _context.FinanceTransactions.FindAsync(dto.Id);
            if (item == null) return false;

            item.DescriptionEn = dto.DescriptionEn;
            item.DescriptionAr = dto.DescriptionAr;
            item.Amount = dto.Amount;
            item.Type = dto.Type;
            item.Date = dto.Date;
            item.CategoryEn = dto.CategoryEn;
            item.CategoryAr = dto.CategoryAr;

            _context.Entry(item).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteTransactionAsync(int id)
        {
            var item = await _context.FinanceTransactions.FindAsync(id);
            if (item == null) return false;

            _context.FinanceTransactions.Remove(item);
            await _context.SaveChangesAsync();
            return true;
        }
        #endregion

        #region Budgets CRUD
        public async Task<IEnumerable<FinanceBudgetDto>> GetAllBudgetsAsync()
        {
            var items = await _context.FinanceBudgets.OrderBy(b => b.DepartmentNameEn).ToListAsync();
            return items.Select(MapToBudgetDto);
        }

        public async Task<FinanceBudgetDto?> GetBudgetByIdAsync(int id)
        {
            var item = await _context.FinanceBudgets.FindAsync(id);
            if (item == null) return null;
            return MapToBudgetDto(item);
        }

        public async Task<FinanceBudgetDto> CreateBudgetAsync(FinanceBudgetDto dto)
        {
            var item = new FinanceBudget
            {
                DepartmentNameEn = dto.DepartmentNameEn,
                DepartmentNameAr = dto.DepartmentNameAr,
                AllocatedAmount = dto.AllocatedAmount,
                SpentAmount = dto.SpentAmount,
                Year = dto.Year == 0 ? DateTime.UtcNow.Year : dto.Year
            };

            _context.FinanceBudgets.Add(item);
            await _context.SaveChangesAsync();
            return MapToBudgetDto(item);
        }

        public async Task<bool> UpdateBudgetAsync(FinanceBudgetDto dto)
        {
            var item = await _context.FinanceBudgets.FindAsync(dto.Id);
            if (item == null) return false;

            item.DepartmentNameEn = dto.DepartmentNameEn;
            item.DepartmentNameAr = dto.DepartmentNameAr;
            item.AllocatedAmount = dto.AllocatedAmount;
            item.SpentAmount = dto.SpentAmount;
            item.Year = dto.Year;

            _context.Entry(item).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteBudgetAsync(int id)
        {
            var item = await _context.FinanceBudgets.FindAsync(id);
            if (item == null) return false;

            _context.FinanceBudgets.Remove(item);
            await _context.SaveChangesAsync();
            return true;
        }
        #endregion

        #region KPIs
        public async Task<FinanceKpisDto> GetFinanceKpisAsync()
        {
            var transactions = await _context.FinanceTransactions.ToListAsync();
            var budgets = await _context.FinanceBudgets.ToListAsync();

            decimal totalRevenue = transactions.Where(t => t.Type == "Revenue").Sum(t => t.Amount);
            decimal totalExpense = transactions.Where(t => t.Type == "Expense").Sum(t => t.Amount);
            decimal operatingExpense = transactions.Where(t => t.Type == "Expense" && t.CategoryEn != "Depreciation").Sum(t => t.Amount);

            decimal ebitda = totalRevenue - operatingExpense;
            decimal netProfit = totalRevenue - totalExpense;

            double ebitdaMargin = totalRevenue > 0 ? (double)(ebitda / totalRevenue) * 100.0 : 0.0;
            double netProfitMargin = totalRevenue > 0 ? (double)(netProfit / totalRevenue) * 100.0 : 0.0;

            decimal operatingCashFlow = transactions.Where(t => t.CategoryEn == "Operations").Sum(t => t.Type == "Revenue" ? t.Amount : -t.Amount);

            decimal totalAssets = transactions.Where(t => t.Type == "Asset").Sum(t => t.Amount);
            if (totalAssets == 0) totalAssets = 5000000m; // Avoid divide by zero, assume base assets
            double roa = (double)(netProfit / totalAssets) * 100.0;

            // Budget Variance Calculation
            double budgetVariance = 0.0;
            if (budgets.Any())
            {
                double totalVar = 0.0;
                foreach (var b in budgets)
                {
                    if (b.AllocatedAmount > 0)
                    {
                        totalVar += (double)Math.Abs(b.AllocatedAmount - b.SpentAmount) / (double)b.AllocatedAmount * 100.0;
                    }
                }
                budgetVariance = totalVar / budgets.Count;
            }

            decimal currentAssets = totalAssets + transactions.Where(t => t.Type == "Receivables").Sum(t => t.Amount);
            decimal currentLiabilities = transactions.Where(t => t.Type == "Liability").Sum(t => t.Amount);
            decimal workingCapital = currentAssets - currentLiabilities;

            // ─── Simple indicators, computed only from what the uploaded ledger carries ───
            // Revenue, expenses and the gap between them; no balance-sheet assumptions.
            int transactionsCount = transactions.Count;

            // Average monthly revenue is spread over the months the ledger actually covers, not
            // over a fixed twelve, so a sheet holding one quarter is not reported as a third of
            // its real monthly run rate.
            var revenueTransactions = transactions.Where(t => t.Type == "Revenue").ToList();
            int revenueMonthsCovered = revenueTransactions
                .Select(t => new { t.Date.Year, t.Date.Month })
                .Distinct()
                .Count();
            decimal averageMonthlyRevenue = revenueMonthsCovered > 0
                ? totalRevenue / revenueMonthsCovered
                : 0m;

            double expenseToRevenueRatio = totalRevenue > 0
                ? (double)(totalExpense / totalRevenue) * 100.0
                : 0.0;

            var topExpense = transactions
                .Where(t => t.Type == "Expense")
                .GroupBy(t => t.CategoryAr)
                .Select(g => new { Category = g.Key, Amount = g.Sum(t => t.Amount) })
                .OrderByDescending(g => g.Amount)
                .FirstOrDefault();

            return new FinanceKpisDto
            {
                TotalRevenueActual = totalRevenue,
                TotalRevenueTarget = 2000000m,

                EbitdaMarginActual = ebitdaMargin,
                EbitdaMarginTarget = 25.0,

                NetProfitMarginActual = netProfitMargin,
                NetProfitMarginTarget = 15.0,

                OperatingCashFlowActual = operatingCashFlow,
                OperatingCashFlowTarget = 1500000m,

                ReturnOnAssetsActual = roa,
                ReturnOnAssetsTarget = 8.0,

                BudgetVarianceRateActual = budgetVariance,
                BudgetVarianceRateTarget = 5.0, // We want low variance, e.g. target is under 5%

                WorkingCapitalActual = workingCapital,
                WorkingCapitalTarget = 3000000m,

                TotalExpensesActual = totalExpense,
                NetProfitActual = netProfit,
                ExpenseToRevenueRatioActual = Math.Round(expenseToRevenueRatio, 1),
                TransactionsCountActual = transactionsCount,
                AverageMonthlyRevenueActual = Math.Round(averageMonthlyRevenue, 2),
                TopExpenseCategoryName = topExpense?.Category ?? "--",
                TopExpenseCategoryAmount = topExpense?.Amount ?? 0m
            };
        }
        #endregion

        #region Mappers
        private FinanceTransactionDto MapToTransactionDto(FinanceTransaction ft) => new FinanceTransactionDto
        {
            Id = ft.Id,
            DescriptionEn = ft.DescriptionEn,
            DescriptionAr = ft.DescriptionAr,
            Amount = ft.Amount,
            Type = ft.Type,
            Date = ft.Date,
            CategoryEn = ft.CategoryEn,
            CategoryAr = ft.CategoryAr
        };

        private FinanceBudgetDto MapToBudgetDto(FinanceBudget fb) => new FinanceBudgetDto
        {
            Id = fb.Id,
            DepartmentNameEn = fb.DepartmentNameEn,
            DepartmentNameAr = fb.DepartmentNameAr,
            AllocatedAmount = fb.AllocatedAmount,
            SpentAmount = fb.SpentAmount,
            Year = fb.Year
        };
        #endregion
        #region Bulk upload (approved single-sheet finance ledger)
        // Imports the approved Finance template: one row per revenue or expense entry. A ledger
        // line is identified by date + statement text, so re-uploading a corrected month updates
        // the same entries instead of double-counting revenue.
        public async Task<ExcelImportResultDto> BulkUploadTransactionsAsync(System.IO.Stream excelStream)
        {
            var transactions = await _context.FinanceTransactions.ToListAsync();

            return await ExcelImportEngine.RunAsync(
                excelStream,
                DepartmentTemplates.Finance,
                async row =>
                {
                    var statement = row.GetString(DepartmentTemplates.FinanceStatement);
                    if (string.IsNullOrWhiteSpace(statement))
                        return ExcelRowOutcomeResult.Skipped("البيان مطلوب.", "البيان");
                    if (statement.Length > 200) statement = statement.Substring(0, 200);

                    var date = row.GetDate(DepartmentTemplates.FinanceDate);
                    if (date == null)
                        return ExcelRowOutcomeResult.Skipped("التاريخ غير مقروء. استخدم الصيغة يوم/شهر/سنة.", "التاريخ");

                    var amount = row.GetDecimal(DepartmentTemplates.FinanceAmount);
                    // The entity requires a strictly positive amount; direction is carried by the
                    // type column, never by a negative figure.
                    if (amount == null || amount <= 0)
                        return ExcelRowOutcomeResult.Skipped("المبلغ يجب أن يكون رقماً أكبر من صفر.", "المبلغ");

                    var type = ParseTransactionType(row.GetString(DepartmentTemplates.FinanceType));
                    if (type == null)
                        return ExcelRowOutcomeResult.Skipped("النوع يجب أن يكون \"إيراد\" أو \"مصروف\".", "النوع");

                    var category = row.GetString(DepartmentTemplates.FinanceCategory);
                    if (string.IsNullOrWhiteSpace(category)) category = "عام";
                    if (category.Length > 100) category = category.Substring(0, 100);

                    var existing = transactions.FirstOrDefault(t =>
                        t.Date.Date == date.Value.Date &&
                        string.Equals(t.DescriptionAr, statement, StringComparison.OrdinalIgnoreCase));

                    if (existing != null)
                    {
                        existing.DescriptionEn = statement;
                        existing.Amount = amount.Value;
                        existing.Type = type;
                        existing.CategoryAr = category;
                        existing.CategoryEn = category;
                        return ExcelRowOutcomeResult.Updated();
                    }

                    var transaction = new FinanceTransaction
                    {
                        DescriptionAr = statement,
                        DescriptionEn = statement,
                        Amount = amount.Value,
                        Type = type,
                        Date = date.Value.Date,
                        CategoryAr = category,
                        CategoryEn = category
                    };
                    _context.FinanceTransactions.Add(transaction);
                    transactions.Add(transaction);
                    await System.Threading.Tasks.Task.CompletedTask;
                    return ExcelRowOutcomeResult.Inserted();
                },
                () => _context.SaveChangesAsync(),
                _logger);
        }

        // Returns the entity's stored type string, or null when the cell says neither.
        private static string? ParseTransactionType(string? raw)
        {
            var value = (raw ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(value)) return null;

            if (value.Contains("إيراد") || value.Contains("ايراد") || value.Contains("دخل") ||
                value.IndexOf("revenue", StringComparison.OrdinalIgnoreCase) >= 0 ||
                value.IndexOf("income", StringComparison.OrdinalIgnoreCase) >= 0)
                return "Revenue";

            if (value.Contains("مصروف") || value.Contains("مصاريف") || value.Contains("تكلفة") ||
                value.IndexOf("expense", StringComparison.OrdinalIgnoreCase) >= 0 ||
                value.IndexOf("cost", StringComparison.OrdinalIgnoreCase) >= 0)
                return "Expense";

            return null;
        }
        #endregion
    }
}
