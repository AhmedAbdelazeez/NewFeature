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

        // Paginated + searchable listing used by the Finance page's Transactions table.
        public async Task<PagedResultDto<FinanceTransactionDto>> GetTransactionsPagedAsync(int page, int pageSize, string? search, string? type)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 20;
            if (pageSize > 200) pageSize = 200;

            var transactions = (await _context.FinanceTransactions.ToListAsync()).AsEnumerable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                transactions = transactions.Where(t =>
                    Contains(t.DescriptionEn, term) ||
                    Contains(t.DescriptionAr, term) ||
                    Contains(t.CategoryEn, term) ||
                    Contains(t.CategoryAr, term));
            }

            if (!string.IsNullOrWhiteSpace(type))
            {
                transactions = transactions.Where(t => string.Equals(t.Type, type, StringComparison.OrdinalIgnoreCase));
            }

            var ordered = transactions.OrderByDescending(t => t.Date).ToList();
            var totalCount = ordered.Count;

            var items = ordered
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(MapToTransactionDto)
                .ToList();

            return new PagedResultDto<FinanceTransactionDto>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
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

        // Paginated + searchable listing used by the Finance page's Budgets table.
        public async Task<PagedResultDto<FinanceBudgetDto>> GetBudgetsPagedAsync(int page, int pageSize, string? search)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 20;
            if (pageSize > 200) pageSize = 200;

            var budgets = (await _context.FinanceBudgets.ToListAsync()).AsEnumerable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                budgets = budgets.Where(b =>
                    Contains(b.DepartmentNameEn, term) ||
                    Contains(b.DepartmentNameAr, term));
            }

            var ordered = budgets.OrderBy(b => b.DepartmentNameEn).ToList();
            var totalCount = ordered.Count;

            var items = ordered
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(MapToBudgetDto)
                .ToList();

            return new PagedResultDto<FinanceBudgetDto>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }
        #endregion


        private static bool Contains(string? haystack, string term) =>
            !string.IsNullOrEmpty(haystack) && haystack.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0;

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

        #region Chart of accounts + balances (the approved Finance template)

        // Which side of the financial statements an account belongs to. Derived from the account's
        // "RSM Classification" in the COA sheet, whose numeric prefix is the one machine-readable
        // signal the sheet carries consistently across all ~500 accounts:
        //   5xxx  assets          6xxx  liabilities and equity
        //   7000  revenue         71xx  cost of sales          72xx  operating/G&A/selling costs
        //   74xx  other gains and losses, finance cost         75xx  income tax / zakat
        private enum AccountBucket
        {
            Unclassified,
            Revenue,
            CostOfSales,
            OperatingExpense,
            OtherExpense,
            Asset,
            Liability,
            Equity
        }

        private static AccountBucket ClassifyAccount(ChartOfAccount account)
        {
            var code = (account.RsmClassification ?? string.Empty).Trim();

            // "7100   Cost of sales" - the leading digits are what matter, the label is free text.
            var digits = new string(code.TakeWhile(char.IsDigit).ToArray());
            if (digits.Length >= 4)
            {
                if (digits.StartsWith("7000")) return AccountBucket.Revenue;
                if (digits.StartsWith("71")) return AccountBucket.CostOfSales;
                if (digits.StartsWith("72")) return AccountBucket.OperatingExpense;
                if (digits.StartsWith("74") || digits.StartsWith("75")) return AccountBucket.OtherExpense;
                if (digits.StartsWith("69")) return AccountBucket.Equity;
                if (digits.StartsWith("6")) return AccountBucket.Liability;
                if (digits.StartsWith("5")) return AccountBucket.Asset;
            }

            // A handful of accounts carry a label instead of a code ("Investments", "Standard
            // Cost", "OCI"); fall back to the balance-sheet/income-statement classifications the
            // same row already carries rather than dropping the account out of every KPI.
            if (!string.IsNullOrWhiteSpace(account.IsClassification))
            {
                var isClass = account.IsClassification;
                if (isClass.IndexOf("revenue", StringComparison.OrdinalIgnoreCase) >= 0)
                    return AccountBucket.Revenue;
                return AccountBucket.OperatingExpense;
            }

            if (!string.IsNullOrWhiteSpace(account.BsClassification))
            {
                var bsClass = account.BsClassification;
                if (bsClass.IndexOf("equity", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    bsClass.IndexOf("capital", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    bsClass.IndexOf("earnings", StringComparison.OrdinalIgnoreCase) >= 0)
                    return AccountBucket.Equity;
                if (bsClass.IndexOf("payable", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    bsClass.IndexOf("liabilit", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    bsClass.IndexOf("obligation", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    bsClass.IndexOf("provision", StringComparison.OrdinalIgnoreCase) >= 0)
                    return AccountBucket.Liability;
                return AccountBucket.Asset;
            }

            return AccountBucket.Unclassified;
        }

        public async Task<IEnumerable<ChartOfAccountDto>> GetChartOfAccountsAsync()
        {
            var accounts = await _context.ChartOfAccounts.AsNoTracking()
                .OrderBy(a => a.AccountNumber)
                .ToListAsync();
            return accounts.Select(MapToAccountDto).ToList();
        }

        public async Task<PagedResultDto<FinanceAccountBalanceDto>> GetAccountBalancesPagedAsync(
            int page, int pageSize, string? search, DateTime? asOfDate, FinanceBalanceFilter? filter = null)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 20;
            if (pageSize > 200) pageSize = 200;

            var query = _context.FinanceAccountBalances.AsNoTracking().AsQueryable();
            if (asOfDate.HasValue)
            {
                var day = asOfDate.Value.Date;
                query = query.Where(b => b.Date == day);
            }
            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                query = query.Where(b => b.AccountNumber.Contains(term) || b.AccountName.Contains(term));
            }

            if (filter != null)
            {
                if (Has(filter.Branch)) query = query.Where(b => b.Branch == filter.Branch);
                if (filter.FromDate.HasValue)
                {
                    var from = filter.FromDate.Value.Date;
                    query = query.Where(b => b.Date >= from);
                }
                if (filter.ToDate.HasValue)
                {
                    var to = filter.ToDate.Value.Date;
                    query = query.Where(b => b.Date <= to);
                }

                // Classification filters live on the account tree, so they are applied as a
                // subquery over it rather than by pulling every row into memory first.
                if (Has(filter.Mapping) || Has(filter.RsmClassification) || Has(filter.ManagementClassification))
                {
                    var accountQuery = _context.ChartOfAccounts.AsNoTracking().AsQueryable();
                    if (Has(filter.Mapping)) accountQuery = accountQuery.Where(a => a.Mapping == filter.Mapping);
                    if (Has(filter.RsmClassification)) accountQuery = accountQuery.Where(a => a.RsmClassification == filter.RsmClassification);
                    if (Has(filter.ManagementClassification)) accountQuery = accountQuery.Where(a => a.ManagementClassification == filter.ManagementClassification);

                    var numbers = accountQuery.Select(a => a.AccountNumber);
                    query = query.Where(b => numbers.Contains(b.AccountNumber));
                }
            }

            var totalCount = await query.CountAsync();

            var rows = await query
                .OrderByDescending(b => b.Date).ThenBy(b => b.Branch).ThenBy(b => b.AccountNumber)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var accountNumbers = rows.Select(r => r.AccountNumber).Distinct().ToList();
            var accounts = await _context.ChartOfAccounts.AsNoTracking()
                .Where(a => accountNumbers.Contains(a.AccountNumber))
                .ToDictionaryAsync(a => a.AccountNumber, a => a);

            var items = rows.Select(b => ToBalanceDto(b, accounts.GetValueOrDefault(b.AccountNumber))).ToList();

            return new PagedResultDto<FinanceAccountBalanceDto>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        // The dropdown contents for both Finance pages: the distinct values the uploaded data
        // actually carries, blanks dropped.
        public async Task<FinanceAccountFilterOptionsDto> GetChartOfAccountFilterOptionsAsync()
        {
            var accounts = await _context.ChartOfAccounts.AsNoTracking().ToListAsync();
            return new FinanceAccountFilterOptionsDto
            {
                Mappings = Distinct(accounts.Select(a => a.Mapping)),
                BsClassifications = Distinct(accounts.Select(a => a.BsClassification)),
                IsClassifications = Distinct(accounts.Select(a => a.IsClassification)),
                RsmClassifications = Distinct(accounts.Select(a => a.RsmClassification)),
                ManagementClassifications = Distinct(accounts.Select(a => a.ManagementClassification)),
                RevenueMainClassifications = Distinct(accounts.Select(a => a.RevenueMainClassification)),
                RevenueSubClassifications = Distinct(accounts.Select(a => a.RevenueSubClassification))
            };
        }

        public async Task<FinanceBalanceFilterOptionsDto> GetBalanceFilterOptionsAsync()
        {
            var branches = await _context.FinanceAccountBalances.AsNoTracking()
                .Select(b => b.Branch).Distinct().ToListAsync();
            var dates = await _context.FinanceAccountBalances.AsNoTracking()
                .Select(b => b.Date).Distinct().OrderByDescending(d => d).ToListAsync();

            // Only the classifications that some uploaded figure actually sits on: a tree of 500
            // accounts would otherwise fill the dropdown with groups the month never touched.
            var usedNumbers = await _context.FinanceAccountBalances.AsNoTracking()
                .Select(b => b.AccountNumber).Distinct().ToListAsync();
            var accounts = await _context.ChartOfAccounts.AsNoTracking()
                .Where(a => usedNumbers.Contains(a.AccountNumber)).ToListAsync();

            return new FinanceBalanceFilterOptionsDto
            {
                Branches = Distinct(branches),
                Dates = dates,
                Mappings = Distinct(accounts.Select(a => a.Mapping)),
                RsmClassifications = Distinct(accounts.Select(a => a.RsmClassification)),
                ManagementClassifications = Distinct(accounts.Select(a => a.ManagementClassification))
            };
        }

        private static bool Has(string? value) => !string.IsNullOrWhiteSpace(value);

        private static List<string> Distinct(IEnumerable<string?> values) => values
            .Where(v => !string.IsNullOrWhiteSpace(v))
            .Select(v => v!.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(v => v, StringComparer.Ordinal)
            .ToList();

        #endregion

        #region KPIs
        // The ten dashboard indicators, every one of them a sum over the uploaded figures grouped
        // by the account's own COA classification. Two periods are reported side by side, because
        // an income statement and a balance sheet do not measure the same thing:
        //
        //   • Profitability (revenue, costs, margins) accumulates the الحركة (Debit - Credit) of
        //     every month uploaded for the latest financial year - year to date. Summing closing
        //     balances instead would report a figure that appears on no statement.
        //   • Position (cash, receivables, payables, assets, liabilities) is read from the closing
        //     balances of the latest reporting date only, because a balance sheet is a snapshot.
        //
        // Sign handling: the trial balance is exported as the accounting system holds it, so a
        // credit-natured account (revenue, liability, equity) arrives negative. Each bucket flips
        // the sign per the account's own classification rather than taking magnitudes, so a credit
        // note genuinely reduces revenue instead of inflating it.
        public async Task<FinanceKpisDto> GetFinanceKpisAsync(string? branch = null)
        {
            var query = _context.FinanceAccountBalances.AsNoTracking().AsQueryable();
            if (Has(branch)) query = query.Where(b => b.Branch == branch);

            var balances = await query.ToListAsync();
            var accounts = await _context.ChartOfAccounts.AsNoTracking().ToListAsync();
            var accountMap = accounts.ToDictionary(a => a.AccountNumber, a => a, StringComparer.OrdinalIgnoreCase);

            var latestDate = balances.Count > 0 ? balances.Max(b => b.Date.Date) : (DateTime?)null;
            if (latestDate == null)
            {
                return new FinanceKpisDto
                {
                    AccountsInChart = accounts.Count,
                    TopRevenueStreamName = "--",
                    TopExpenseCategoryName = "--"
                };
            }

            var fiscalYear = latestDate.Value.Year;
            var periodRows = balances.Where(b => b.Date.Year == fiscalYear).ToList();
            var closingRows = balances.Where(b => b.Date.Date == latestDate.Value).ToList();

            decimal revenue = 0m, costOfSales = 0m, operatingExpense = 0m, otherExpense = 0m;
            decimal unclassifiedExpense = 0m;
            var expenseByCategory = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
            var revenueByStream = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
            int unmatchedRows = 0;

            foreach (var row in periodRows)
            {
                if (!accountMap.TryGetValue(row.AccountNumber, out var account))
                {
                    unmatchedRows++;
                    continue;
                }

                var value = PeriodValue(row);
                if (value == 0m) continue;

                switch (ClassifyAccount(account))
                {
                    case AccountBucket.Revenue:
                        // Credit-natured: a credit of 100 is revenue of +100.
                        revenue += -value;
                        Accumulate(revenueByStream, RevenueStreamLabel(account), -value);
                        break;
                    case AccountBucket.CostOfSales:
                        costOfSales += value;
                        AccumulateExpense(expenseByCategory, account, value, ref unclassifiedExpense);
                        break;
                    case AccountBucket.OperatingExpense:
                        operatingExpense += value;
                        AccumulateExpense(expenseByCategory, account, value, ref unclassifiedExpense);
                        break;
                    case AccountBucket.OtherExpense:
                        // Interest and net gains/losses: a net gain arrives negative and correctly
                        // reduces total expenses rather than being counted as a cost.
                        otherExpense += value;
                        AccumulateExpense(expenseByCategory, account, value, ref unclassifiedExpense);
                        break;
                }
            }

            decimal totalAssets = 0m, totalLiabilities = 0m;
            decimal cash = 0m, receivables = 0m, payablesAndAccruals = 0m;

            // The closing position is rebuilt the way a ledger does it - the opening balance the
            // year started from, plus every movement reported since - rather than read off the
            // latest row. Their September sheet carries that month's debits and credits with no
            // opening balance in it, so reading the latest row alone would report September's
            // movement as if it were the whole cash position. Grouped per branch and account,
            // because two branches keep their own opening balance for the same account.
            foreach (var group in periodRows.GroupBy(b => new { b.Branch, b.AccountNumber }))
            {
                if (!accountMap.TryGetValue(group.Key.AccountNumber, out var account)) continue;

                var ordered = group.OrderBy(b => b.Date).ToList();
                var closing = ordered[0].OpeningBalance + ordered.Sum(PeriodValue);

                var bucket = ClassifyAccount(account);
                if (bucket == AccountBucket.Asset) totalAssets += closing;
                if (bucket == AccountBucket.Liability) totalLiabilities += -closing;

                // The three cards a finance manager watches week to week, grouped the way their own
                // COA groups them (Mapping / BS Classification), with the RSM code as the fallback
                // for an account whose Arabic mapping was left blank.
                if (InGroup(account, CashKeywords, CashCodes)) cash += closing;
                if (InGroup(account, ReceivableKeywords, ReceivableCodes)) receivables += closing;
                if (InGroup(account, PayableKeywords, PayableCodes)) payablesAndAccruals += -closing;
            }

            decimal totalExpenses = costOfSales + operatingExpense + otherExpense;
            decimal grossProfit = revenue - costOfSales;
            decimal netProfit = revenue - totalExpenses;

            double grossMargin = revenue > 0 ? (double)(grossProfit / revenue) * 100.0 : 0.0;
            double netMargin = revenue > 0 ? (double)(netProfit / revenue) * 100.0 : 0.0;
            double expenseRatio = revenue > 0 ? (double)(totalExpenses / revenue) * 100.0 : 0.0;

            var topExpense = Top(expenseByCategory);
            var topRevenue = Top(revenueByStream);

            return new FinanceKpisDto
            {
                AsOfDate = latestDate,
                FiscalYear = fiscalYear,
                PeriodFrom = periodRows.Count > 0 ? periodRows.Min(b => b.Date.Date) : null,

                TotalRevenue = revenue,
                CostOfSales = costOfSales,
                GrossProfit = grossProfit,
                GrossProfitMarginPercent = Math.Round(grossMargin, 1),
                OperatingExpenses = operatingExpense + otherExpense,
                TotalExpenses = totalExpenses,
                NetProfit = netProfit,
                NetProfitMarginPercent = Math.Round(netMargin, 1),
                ExpenseToRevenueRatioPercent = Math.Round(expenseRatio, 1),

                CashAndEquivalents = cash,
                TradeReceivables = receivables,
                PayablesAndAccruals = payablesAndAccruals,
                TotalAssets = totalAssets,
                TotalLiabilities = totalLiabilities,

                TopRevenueStreamName = topRevenue.Name,
                TopRevenueStreamAmount = topRevenue.Amount,
                TopExpenseCategoryName = topExpense.Name,
                TopExpenseCategoryAmount = topExpense.Amount,

                AccountsInChart = accounts.Count,
                BalancesLoaded = closingRows.Count,
                UnclassifiedBalances = unmatchedRows,
                UnclassifiedExpenseAmount = unclassifiedExpense,
                Branches = Distinct(balances.Select(b => b.Branch))
            };
        }

        // The period's own activity for one uploaded row. Debit/Credit is what the template asks
        // for; a row that carries only a closing figure (the first version of the template, or a
        // hand-entered balance) falls back to Balance - OpeningBalance, which is the same number.
        private static decimal PeriodValue(FinanceAccountBalance row) =>
            row.Debit != 0m || row.Credit != 0m ? row.Movement : row.Balance - row.OpeningBalance;

        // ── The balance-sheet groups, keyed off the COA's own Arabic Mapping and English BS
        // Classification, with the RSM code prefix as the fallback ──
        private static readonly string[] CashKeywords = { "نقد في الصندوق", "Cash and cash equivalents" };
        private static readonly string[] CashCodes = { "5000" };
        private static readonly string[] ReceivableKeywords = { "ذمم مدينة", "Trade receivables" };
        private static readonly string[] ReceivableCodes = { "5200" };
        private static readonly string[] PayableKeywords = { "ذمم دائنة", "Trade payables", "مصاريف مستحقة", "Accrued expenses" };
        private static readonly string[] PayableCodes = { "6100", "6200" };

        private static bool InGroup(ChartOfAccount account, string[] keywords, string[] rsmCodes)
        {
            foreach (var keyword in keywords)
            {
                if (Contains(account.Mapping, keyword) || Contains(account.BsClassification, keyword)) return true;
            }
            return rsmCodes.Any(code => IsCode(account, code));
        }

        private static bool IsCode(ChartOfAccount account, string prefix) =>
            (account.RsmClassification ?? string.Empty).TrimStart().StartsWith(prefix, StringComparison.Ordinal);

        private static void Accumulate(Dictionary<string, decimal> bag, string label, decimal amount) =>
            bag[label] = bag.GetValueOrDefault(label) + amount;

        // "Top expense item" is reported by the cost block the management accounts group it under,
        // because that is the label the finance team recognises ("تكلفة الموارد البشرية"، "مصروفات
        // تشغيلية"). Expense sitting on an account with no such label is money that cannot be
        // attributed, so it is both bucketed as غير مصنف and reported as its own coverage figure.
        private static void AccumulateExpense(
            Dictionary<string, decimal> bag, ChartOfAccount account, decimal amount, ref decimal unclassified)
        {
            // Measured on the management classification alone: the RSM code is a usable label to
            // bucket the cost under, but an account that carries no cost block is still money the
            // finance team cannot attribute, and that is what this figure is for.
            if (string.IsNullOrWhiteSpace(account.ManagementClassification)) unclassified += amount;

            var label = FirstNonEmpty(account.ManagementClassification, account.RsmClassification);
            Accumulate(bag, label ?? "غير مصنف", amount);
        }

        private static string RevenueStreamLabel(ChartOfAccount account) =>
            FirstNonEmpty(
                account.RevenueMainClassification,
                account.RevenueSubClassification,
                account.IsClassification,
                account.ManagementClassification) ?? "غير مصنف";

        private static (string Name, decimal Amount) Top(Dictionary<string, decimal> bag)
        {
            if (bag.Count == 0) return ("--", 0m);
            var top = bag.OrderByDescending(kv => kv.Value).First();
            return (top.Key, top.Value);
        }

        private static string? FirstNonEmpty(params string?[] values) =>
            values.FirstOrDefault(v => !string.IsNullOrWhiteSpace(v))?.Trim();
        #endregion

        #region Chart of accounts: index + CRUD (the شجرة الحسابات page)
        // Every filter is an exact match on one of the tree's own classification columns, because
        // that is how the finance team reads the tree: "show me the accounts mapped to مصاريف
        // مدفوعة مقدما وموجودات اخرى". Search stays a free substring over number, name and
        // classifications.
        public async Task<PagedResultDto<ChartOfAccountDto>> GetChartOfAccountsPagedAsync(
            int page, int pageSize, string? search, ChartOfAccountFilter? filter = null)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 20;
            if (pageSize > 200) pageSize = 200;

            var query = _context.ChartOfAccounts.AsNoTracking().AsQueryable();
            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                query = query.Where(a =>
                    a.AccountNumber.Contains(term) ||
                    a.AccountName.Contains(term) ||
                    a.RsmClassification.Contains(term) ||
                    (a.Mapping != null && a.Mapping.Contains(term)) ||
                    (a.ManagementClassification != null && a.ManagementClassification.Contains(term)));
            }

            if (filter != null)
            {
                if (Has(filter.Mapping)) query = query.Where(a => a.Mapping == filter.Mapping);
                if (Has(filter.BsClassification)) query = query.Where(a => a.BsClassification == filter.BsClassification);
                if (Has(filter.IsClassification)) query = query.Where(a => a.IsClassification == filter.IsClassification);
                if (Has(filter.RsmClassification)) query = query.Where(a => a.RsmClassification == filter.RsmClassification);
                if (Has(filter.ManagementClassification)) query = query.Where(a => a.ManagementClassification == filter.ManagementClassification);
                if (Has(filter.RevenueMainClassification)) query = query.Where(a => a.RevenueMainClassification == filter.RevenueMainClassification);
                if (Has(filter.RevenueSubClassification)) query = query.Where(a => a.RevenueSubClassification == filter.RevenueSubClassification);
            }

            var totalCount = await query.CountAsync();
            var items = await query.OrderBy(a => a.AccountNumber)
                .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            return new PagedResultDto<ChartOfAccountDto>
            {
                Items = items.Select(MapToAccountDto).ToList(),
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<ChartOfAccountDto?> GetAccountAsync(int id)
        {
            var account = await _context.ChartOfAccounts.AsNoTracking().FirstOrDefaultAsync(a => a.Id == id);
            return account == null ? null : MapToAccountDto(account);
        }

        public async Task<CrudResult<ChartOfAccountDto>> CreateAccountAsync(ChartOfAccountDto dto)
        {
            var errors = ValidateAccount(dto);
            if (errors.Count == 0 && await _context.ChartOfAccounts.AnyAsync(a => a.AccountNumber == dto.AccountNumber.Trim()))
                errors.Add("accountNumber", "رقم الحساب موجود مسبقاً في شجرة الحسابات.");
            if (errors.Count > 0) return CrudResult<ChartOfAccountDto>.Invalid(errors);

            var account = new ChartOfAccount();
            ApplyAccount(dto, account);
            _context.ChartOfAccounts.Add(account);
            await _context.SaveChangesAsync();
            return CrudResult<ChartOfAccountDto>.Ok(MapToAccountDto(account));
        }

        public async Task<CrudResult<ChartOfAccountDto>> UpdateAccountAsync(int id, ChartOfAccountDto dto)
        {
            var account = await _context.ChartOfAccounts.FirstOrDefaultAsync(a => a.Id == id);
            if (account == null) return CrudResult<ChartOfAccountDto>.Missing();

            var errors = ValidateAccount(dto);
            var newNumber = (dto.AccountNumber ?? string.Empty).Trim();
            if (errors.Count == 0 && await _context.ChartOfAccounts.AnyAsync(a => a.AccountNumber == newNumber && a.Id != id))
                errors.Add("accountNumber", "رقم الحساب موجود مسبقاً في شجرة الحسابات.");

            // Renumbering an account that already carries balances would orphan them from the tree
            // they are classified through, silently dropping them out of every KPI.
            if (errors.Count == 0 && !string.Equals(account.AccountNumber, newNumber, StringComparison.OrdinalIgnoreCase) &&
                await _context.FinanceAccountBalances.AnyAsync(b => b.AccountNumber == account.AccountNumber))
                errors.Add("accountNumber", "لا يمكن تغيير رقم حساب له أرصدة مسجلة.");

            if (errors.Count > 0) return CrudResult<ChartOfAccountDto>.Invalid(errors);

            ApplyAccount(dto, account);
            await _context.SaveChangesAsync();
            return CrudResult<ChartOfAccountDto>.Ok(MapToAccountDto(account));
        }

        public async Task<CrudResult<ChartOfAccountDto>> DeleteAccountAsync(int id)
        {
            var account = await _context.ChartOfAccounts.FirstOrDefaultAsync(a => a.Id == id);
            if (account == null) return CrudResult<ChartOfAccountDto>.Missing();

            if (await _context.FinanceAccountBalances.AnyAsync(b => b.AccountNumber == account.AccountNumber))
                return CrudResult<ChartOfAccountDto>.Invalid(new List<FieldErrorDto>
                {
                    new() { Field = "accountNumber", Message = "لا يمكن حذف حساب له أرصدة مسجلة. احذف أرصدته أولاً." }
                });

            _context.ChartOfAccounts.Remove(account);
            await _context.SaveChangesAsync();
            return CrudResult<ChartOfAccountDto>.Ok(MapToAccountDto(account));
        }

        // The single rule set for a chart-of-accounts row, shared by the Excel import and the form.
        // An account must carry at least one classification, since that is what decides whether it
        // is a revenue, a cost or a balance-sheet item - but any of the three will do. Their own COA
        // leaves RSM Classification blank on a handful of accounts (Account Receivables - Others,
        // Allowance for Bad Debts) while still classifying them under BS Classification, and
        // rejecting those would silently drop real receivables out of the KPI.
        private static List<FieldErrorDto> ValidateAccount(ChartOfAccountDto dto)
        {
            var errors = new List<FieldErrorDto>();
            if (string.IsNullOrWhiteSpace(dto.AccountNumber)) errors.Add("accountNumber", "رقم الحساب مطلوب.");
            if (string.IsNullOrWhiteSpace(dto.AccountName)) errors.Add("accountName", "اسم الحساب مطلوب.");
            if (string.IsNullOrWhiteSpace(dto.RsmClassification) &&
                string.IsNullOrWhiteSpace(dto.BsClassification) &&
                string.IsNullOrWhiteSpace(dto.IsClassification))
                errors.Add("rsmClassification",
                    "الحساب بحاجة إلى تصنيف واحد على الأقل (RSM أو BS Classification أو IS Classification)، فهو ما يحدد إن كان الحساب إيراداً أو تكلفة أو بنداً في المركز المالي.");
            return errors;
        }

        private static void ApplyAccount(ChartOfAccountDto dto, ChartOfAccount account)
        {
            account.AccountNumber = Truncate(dto.AccountNumber.Trim(), 50);
            account.AccountName = Truncate(dto.AccountName.Trim(), 250);
            // Stored as an empty string rather than null when blank: the column is NOT NULL, and the
            // KPI classifier already falls back to BS/IS Classification for such an account.
            account.RsmClassification = Truncate((dto.RsmClassification ?? string.Empty).Trim(), 200);
            account.Mapping = TruncateOrNull(dto.Mapping, 200);
            account.BsClassification = TruncateOrNull(dto.BsClassification, 200);
            account.IsClassification = TruncateOrNull(dto.IsClassification, 200);
            account.ManagementClassification = TruncateOrNull(dto.ManagementClassification, 200);
            account.RevenueMainClassification = TruncateOrNull(dto.RevenueMainClassification, 200);
            account.RevenueSubClassification = TruncateOrNull(dto.RevenueSubClassification, 200);
        }
        #endregion

        #region Account balances: CRUD (the الأرصدة والحركات page)
        public async Task<FinanceAccountBalanceDto?> GetBalanceAsync(int id)
        {
            var b = await _context.FinanceAccountBalances.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
            if (b == null) return null;
            var account = await _context.ChartOfAccounts.AsNoTracking()
                .FirstOrDefaultAsync(a => a.AccountNumber == b.AccountNumber);
            return ToBalanceDto(b, account);
        }

        public async Task<CrudResult<FinanceAccountBalanceDto>> CreateBalanceAsync(FinanceAccountBalanceDto dto)
        {
            var errors = await ValidateBalanceAsync(dto, excludeId: null);
            if (errors.Count > 0) return CrudResult<FinanceAccountBalanceDto>.Invalid(errors);

            var balance = new FinanceAccountBalance();
            await ApplyBalanceAsync(dto, balance);
            _context.FinanceAccountBalances.Add(balance);
            await _context.SaveChangesAsync();
            return CrudResult<FinanceAccountBalanceDto>.Ok((await GetBalanceAsync(balance.Id))!);
        }

        public async Task<CrudResult<FinanceAccountBalanceDto>> UpdateBalanceAsync(int id, FinanceAccountBalanceDto dto)
        {
            var balance = await _context.FinanceAccountBalances.FirstOrDefaultAsync(b => b.Id == id);
            if (balance == null) return CrudResult<FinanceAccountBalanceDto>.Missing();

            var errors = await ValidateBalanceAsync(dto, excludeId: id);
            if (errors.Count > 0) return CrudResult<FinanceAccountBalanceDto>.Invalid(errors);

            await ApplyBalanceAsync(dto, balance);
            await _context.SaveChangesAsync();
            return CrudResult<FinanceAccountBalanceDto>.Ok((await GetBalanceAsync(balance.Id))!);
        }

        public async Task<bool> DeleteBalanceAsync(int id)
        {
            var balance = await _context.FinanceAccountBalances.FirstOrDefaultAsync(b => b.Id == id);
            if (balance == null) return false;
            _context.FinanceAccountBalances.Remove(balance);
            await _context.SaveChangesAsync();
            return true;
        }

        // Field-level rules for a row of figures, shared by the Excel import and the form. The
        // account must exist in the chart of accounts, because an unclassifiable figure silently
        // drops out of every KPI.
        //
        // figuresProvided says whether the source actually carried numbers. A trial balance reports
        // hundreds of accounts that saw no movement, and an explicit zero is a real reported figure
        // - rejecting it would fill the upload report with hundreds of false errors. A row with no
        // numbers at all is a different thing: a reporting gap, and it is rejected.
        private static List<FieldErrorDto> ValidateBalance(
            FinanceAccountBalanceDto dto, ICollection<string> knownAccounts, bool figuresProvided = false)
        {
            var errors = new List<FieldErrorDto>();
            if (dto.Date == default) errors.Add("date", "التاريخ غير مقروء. استخدم الصيغة يوم/شهر/سنة.");
            if (string.IsNullOrWhiteSpace(dto.AccountNumber)) errors.Add("accountNumber", "رقم الحساب مطلوب.");
            else if (!knownAccounts.Contains(dto.AccountNumber.Trim()))
                errors.Add("accountNumber", $"رقم الحساب \"{dto.AccountNumber.Trim()}\" غير موجود في شجرة الحسابات (COA). ارفع ملف شجرة الحسابات أولاً.");
            if (!figuresProvided && dto.Balance == null && dto.Debit == 0m && dto.Credit == 0m && dto.OpeningBalance == 0m)
                errors.Add("balance", "الصف لا يحتوي على أي رقم: املأ المدين والدائن أو الرصيد.");
            return errors;
        }

        private async Task<List<FieldErrorDto>> ValidateBalanceAsync(FinanceAccountBalanceDto dto, int? excludeId)
        {
            var number = (dto.AccountNumber ?? string.Empty).Trim();
            var known = await _context.ChartOfAccounts.AnyAsync(a => a.AccountNumber == number)
                ? new HashSet<string>(StringComparer.OrdinalIgnoreCase) { number }
                : new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            var errors = ValidateBalance(dto, known);
            if (errors.Count == 0)
            {
                var day = dto.Date.Date;
                var branch = NormalizeBranch(dto.Branch);
                if (await _context.FinanceAccountBalances.AnyAsync(b =>
                        b.Date == day && b.Branch == branch && b.AccountNumber == number &&
                        (excludeId == null || b.Id != excludeId)))
                    errors.Add("accountNumber", "يوجد رصيد مسجل لهذا الحساب في نفس التاريخ ونفس الفرع.");
            }
            return errors;
        }

        // The account name is taken from the chart of accounts when the form leaves it blank, so a
        // hand-entered row reads the same as an uploaded one. The closing balance is computed from
        // the opening balance and the period's debits and credits; a row that carries only a
        // closing figure keeps it as given.
        private async System.Threading.Tasks.Task ApplyBalanceAsync(FinanceAccountBalanceDto dto, FinanceAccountBalance balance)
        {
            var number = dto.AccountNumber.Trim();
            var name = dto.AccountName;
            if (string.IsNullOrWhiteSpace(name))
                name = await _context.ChartOfAccounts.Where(a => a.AccountNumber == number)
                    .Select(a => a.AccountName).FirstOrDefaultAsync() ?? string.Empty;

            balance.Date = dto.Date.Date;
            balance.Branch = NormalizeBranch(dto.Branch);
            balance.AccountNumber = Truncate(number, 50);
            balance.AccountName = Truncate(name.Trim(), 250);
            balance.OpeningBalance = dto.OpeningBalance;
            balance.Debit = dto.Debit;
            balance.Credit = dto.Credit;
            balance.Balance = ResolveClosingBalance(dto);
        }

        // A blank branch is the single-branch case, and every row uploaded before the branch column
        // existed - it must land in the same bucket as the default the template ships with, or the
        // same month would appear twice under two names.
        private static string NormalizeBranch(string? branch) =>
            string.IsNullOrWhiteSpace(branch)
                ? FinanceAccountBalance.DefaultBranch
                : Truncate(branch.Trim(), 100);

        private static decimal ResolveClosingBalance(FinanceAccountBalanceDto dto)
        {
            if (dto.Debit != 0m || dto.Credit != 0m)
                return dto.OpeningBalance + dto.Debit - dto.Credit;
            return dto.Balance ?? dto.OpeningBalance;
        }

        private static FinanceAccountBalanceDto ToBalanceDto(FinanceAccountBalance b, ChartOfAccount? account) => new()
        {
            Id = b.Id,
            Date = b.Date,
            Branch = b.Branch,
            AccountNumber = b.AccountNumber,
            AccountName = b.AccountName,
            OpeningBalance = b.OpeningBalance,
            Debit = b.Debit,
            Credit = b.Credit,
            Balance = b.Balance,
            Movement = b.Movement,
            Classification = account?.RsmClassification,
            Mapping = account?.Mapping,
            ManagementClassification = account?.ManagementClassification
        };
        #endregion

        #region Bulk upload (two approved templates: the account tree and the monthly figures)
        // The chart of accounts and the monthly figures are uploaded as two separate files, each
        // from its own page, because they change on completely different clocks: the tree a few
        // times a year, the figures every month. The account tree must be uploaded first - a figure
        // whose account is not in the tree cannot be classified, so it is rejected by row with the
        // account number named rather than silently dropping out of every KPI.
        public async Task<ExcelImportResultDto> BulkUploadChartOfAccountsAsync(System.IO.Stream excelStream) =>
            await ImportChartOfAccountsAsync(excelStream, DepartmentTemplates.FinanceChartOfAccountsFile);

        public async Task<ExcelImportResultDto> BulkUploadBalancesAsync(System.IO.Stream excelStream) =>
            await ImportBalancesAsync(excelStream, DepartmentTemplates.FinanceBalancesFile);

        // The older single-workbook upload (COA + الحركات المالية in one file), kept working so a
        // file prepared against the previous template still imports: the same two row handlers, run
        // over the two named sheets of one workbook.
        public async Task<ExcelImportResultDto> BulkUploadFinanceWorkbookAsync(System.IO.Stream excelStream)
        {
            // Buffered once and replayed from a fresh MemoryStream per sheet: the engine opens an
            // XLWorkbook over whatever stream it is handed and disposes it, so the second pass must
            // not depend on the first pass having left the caller's stream usable.
            using var buffer = new System.IO.MemoryStream();
            if (excelStream.CanSeek) excelStream.Position = 0;
            await excelStream.CopyToAsync(buffer);

            var coaResult = await ImportChartOfAccountsAsync(
                new System.IO.MemoryStream(buffer.ToArray()), DepartmentTemplates.FinanceChartOfAccounts);

            // A workbook whose COA sheet is missing or malformed is rejected before any figure is
            // touched: importing figures against an account tree that failed to load would leave
            // every KPI reading zero with no obvious reason why.
            if (!coaResult.Success) return coaResult;

            var balanceResult = await ImportBalancesAsync(
                new System.IO.MemoryStream(buffer.ToArray()), DepartmentTemplates.FinanceBalances);

            if (!balanceResult.Success) return balanceResult;

            // One combined result, so the uploader sees what the whole file did rather than only
            // what its second sheet did.
            return new ExcelImportResultDto
            {
                Success = true,
                TotalRows = coaResult.TotalRows + balanceResult.TotalRows,
                DataRows = coaResult.DataRows + balanceResult.DataRows,
                InsertedRows = coaResult.InsertedRows + balanceResult.InsertedRows,
                UpdatedRows = coaResult.UpdatedRows + balanceResult.UpdatedRows,
                SkippedRows = coaResult.SkippedRows + balanceResult.SkippedRows,
                Errors = coaResult.Errors.Concat(balanceResult.Errors).ToList(),
                Message = coaResult.Errors.Count + balanceResult.Errors.Count > 0
                    ? $"تمت معالجة الملف: {coaResult.DataRows} حساب في شجرة الحسابات و {balanceResult.DataRows} سجل أرصدة، مع تجاهل {coaResult.SkippedRows + balanceResult.SkippedRows} صف. التفاصيل بالأسفل."
                    : $"تمت معالجة الملف بنجاح: {coaResult.DataRows} حساب في شجرة الحسابات و {balanceResult.DataRows} سجل أرصدة."
            };
        }

        private async Task<ExcelImportResultDto> ImportChartOfAccountsAsync(
            System.IO.Stream excelStream, ExcelTemplateDefinition template)
        {
            var accounts = await _context.ChartOfAccounts.ToListAsync();
            var accountColumns = new Dictionary<string, string>
            {
                ["accountNumber"] = "Account #", ["accountName"] = "Account Name", ["rsmClassification"] = "RSM Classification"
            };

            return await ExcelImportEngine.RunAsync(
                excelStream,
                template,
                async row =>
                {
                    var dto = new ChartOfAccountDto
                    {
                        AccountNumber = row.GetString(DepartmentTemplates.FinanceAccountNumber),
                        AccountName = row.GetString(DepartmentTemplates.FinanceAccountName),
                        RsmClassification = row.GetString(DepartmentTemplates.FinanceRsmClassification),
                        Mapping = row.GetString(DepartmentTemplates.FinanceMapping),
                        BsClassification = row.GetString(DepartmentTemplates.FinanceBsClassification),
                        IsClassification = row.GetString(DepartmentTemplates.FinanceIsClassification),
                        ManagementClassification = row.GetString(DepartmentTemplates.FinanceManagementClassification),
                        RevenueMainClassification = row.GetString(DepartmentTemplates.FinanceRevenueMainClassification),
                        RevenueSubClassification = row.GetString(DepartmentTemplates.FinanceRevenueSubClassification)
                    };

                    var errors = ValidateAccount(dto);
                    if (errors.Count > 0)
                        return ExcelRowOutcomeResult.Skipped(errors.Joined(), accountColumns.GetValueOrDefault(errors[0].Field));

                    var number = Truncate(dto.AccountNumber.Trim(), 50);
                    var existing = accounts.FirstOrDefault(a =>
                        string.Equals(a.AccountNumber, number, StringComparison.OrdinalIgnoreCase));

                    bool isNew = existing == null;
                    var account = existing ?? new ChartOfAccount();
                    ApplyAccount(dto, account);

                    if (isNew)
                    {
                        _context.ChartOfAccounts.Add(account);
                        accounts.Add(account);
                        await System.Threading.Tasks.Task.CompletedTask;
                        return ExcelRowOutcomeResult.Inserted();
                    }

                    return ExcelRowOutcomeResult.Updated();
                },
                () => _context.SaveChangesAsync(),
                _logger);
        }

        private async Task<ExcelImportResultDto> ImportBalancesAsync(
            System.IO.Stream excelStream, ExcelTemplateDefinition template)
        {
            var balances = await _context.FinanceAccountBalances.ToListAsync();
            var accounts = await _context.ChartOfAccounts.AsNoTracking()
                .ToDictionaryAsync(a => a.AccountNumber, a => a.AccountName, StringComparer.OrdinalIgnoreCase);
            var accountNumbers = new HashSet<string>(accounts.Keys, StringComparer.OrdinalIgnoreCase);

            // Nothing to classify figures through: say so once, instead of rejecting 500 rows one
            // by one for the same reason.
            if (accountNumbers.Count == 0)
            {
                return new ExcelImportResultDto
                {
                    Success = false,
                    Message = "شجرة الحسابات (COA) فارغة. ارفع ملف شجرة الحسابات أولاً، فكل رصيد يُصنَّف من خلالها."
                };
            }

            var balanceColumns = new Dictionary<string, string>
            {
                ["date"] = "Date", ["accountNumber"] = "Account No", ["balance"] = "Debit / Credit"
            };

            return await ExcelImportEngine.RunAsync(
                excelStream,
                template,
                async row =>
                {
                    // Read as nullables first: a blank cell and a typed zero mean different things
                    // here, and only the all-blank row is a reporting gap worth rejecting.
                    var opening = row.GetDecimal(DepartmentTemplates.FinanceBalanceOpening);
                    var debit = row.GetDecimal(DepartmentTemplates.FinanceBalanceDebit);
                    var credit = row.GetDecimal(DepartmentTemplates.FinanceBalanceCredit);
                    var closing = row.GetDecimal(DepartmentTemplates.FinanceBalanceAmount);

                    var dto = new FinanceAccountBalanceDto
                    {
                        Date = row.GetDate(DepartmentTemplates.FinanceBalanceDate)?.Date ?? default,
                        Branch = row.GetString(DepartmentTemplates.FinanceBalanceBranch),
                        AccountNumber = row.GetString(DepartmentTemplates.FinanceBalanceAccountNumber),
                        AccountName = row.GetString(DepartmentTemplates.FinanceBalanceAccountName),
                        OpeningBalance = opening ?? 0m,
                        Debit = debit ?? 0m,
                        Credit = credit ?? 0m,
                        Balance = closing
                    };

                    var figuresProvided = opening.HasValue || debit.HasValue || credit.HasValue || closing.HasValue;
                    var errors = ValidateBalance(dto, accountNumbers, figuresProvided);
                    if (errors.Count > 0)
                        return ExcelRowOutcomeResult.Skipped(errors.Joined(), balanceColumns.GetValueOrDefault(errors[0].Field));

                    var number = Truncate(dto.AccountNumber.Trim(), 50);
                    var branch = NormalizeBranch(dto.Branch);
                    var day = dto.Date.Date;

                    // The account name is the tree's, not the uploaded sheet's: a trial balance
                    // export often abbreviates it, and two spellings of one account read as two
                    // different accounts on the page.
                    var name = accounts.GetValueOrDefault(number);
                    if (string.IsNullOrWhiteSpace(name)) name = dto.AccountName;

                    var existing = balances.FirstOrDefault(b =>
                        b.Date.Date == day &&
                        string.Equals(b.Branch, branch, StringComparison.OrdinalIgnoreCase) &&
                        string.Equals(b.AccountNumber, number, StringComparison.OrdinalIgnoreCase));

                    var target = existing ?? new FinanceAccountBalance();
                    target.Date = day;
                    target.Branch = branch;
                    target.AccountNumber = number;
                    target.AccountName = Truncate((name ?? string.Empty).Trim(), 250);
                    target.OpeningBalance = dto.OpeningBalance;
                    target.Debit = dto.Debit;
                    target.Credit = dto.Credit;
                    target.Balance = ResolveClosingBalance(dto);

                    if (existing != null) return ExcelRowOutcomeResult.Updated();

                    _context.FinanceAccountBalances.Add(target);
                    balances.Add(target);
                    await System.Threading.Tasks.Task.CompletedTask;
                    return ExcelRowOutcomeResult.Inserted();
                },
                () => _context.SaveChangesAsync(),
                _logger);
        }


        private static string Truncate(string value, int maxLength) =>
            value.Length <= maxLength ? value : value.Substring(0, maxLength);

        private static string? TruncateOrNull(string? value, int maxLength) =>
            string.IsNullOrWhiteSpace(value) ? null : Truncate(value.Trim(), maxLength);

        private static ChartOfAccountDto MapToAccountDto(ChartOfAccount a) => new()
        {
            Id = a.Id,
            AccountNumber = a.AccountNumber,
            AccountName = a.AccountName,
            Mapping = a.Mapping,
            BsClassification = a.BsClassification,
            IsClassification = a.IsClassification,
            RsmClassification = a.RsmClassification,
            ManagementClassification = a.ManagementClassification,
            RevenueMainClassification = a.RevenueMainClassification,
            RevenueSubClassification = a.RevenueSubClassification
        };
        #endregion
    }
}
