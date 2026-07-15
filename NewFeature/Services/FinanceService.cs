using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NewFeature.Models;
using NewFeature.Services.Repositories;

namespace NewFeature.Services
{
    public class FinanceService : IFinanceService
    {
        private readonly ApplicationDbContext _context;

        public FinanceService(ApplicationDbContext context)
        {
            _context = context;
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
                WorkingCapitalTarget = 3000000m
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
    }
}
