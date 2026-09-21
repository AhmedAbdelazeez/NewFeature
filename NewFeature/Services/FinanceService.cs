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
            int page, int pageSize, string? search, DateTime? asOfDate)
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

            var totalCount = await query.CountAsync();

            var rows = await query
                .OrderByDescending(b => b.Date).ThenBy(b => b.AccountNumber)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var accountNumbers = rows.Select(r => r.AccountNumber).Distinct().ToList();
            var classifications = await _context.ChartOfAccounts.AsNoTracking()
                .Where(a => accountNumbers.Contains(a.AccountNumber))
                .ToDictionaryAsync(a => a.AccountNumber, a => a.RsmClassification);

            var items = rows.Select(b => new FinanceAccountBalanceDto
            {
                Id = b.Id,
                Date = b.Date,
                AccountNumber = b.AccountNumber,
                AccountName = b.AccountName,
                Balance = b.Balance,
                Classification = classifications.GetValueOrDefault(b.AccountNumber)
            }).ToList();

            return new PagedResultDto<FinanceAccountBalanceDto>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        #endregion

        #region KPIs
        // Ten indicators, every one of them a sum over the balances sheet grouped by the account's
        // own COA classification. Nothing here assumes an asset base, a depreciation figure or a
        // target the uploaded file doesn't carry.
        public async Task<FinanceKpisDto> GetFinanceKpisAsync()
        {
            var balances = await _context.FinanceAccountBalances.AsNoTracking().ToListAsync();
            var accounts = await _context.ChartOfAccounts.AsNoTracking().ToListAsync();

            var accountMap = accounts.ToDictionary(a => a.AccountNumber, a => a, StringComparer.OrdinalIgnoreCase);

            // KPIs describe the latest reporting date on file, not the sum of every month ever
            // uploaded - a trial balance is a snapshot, so adding twelve of them together would
            // report a figure that never existed on any statement.
            var latestDate = balances.Count > 0 ? balances.Max(b => b.Date.Date) : (DateTime?)null;
            var current = latestDate.HasValue
                ? balances.Where(b => b.Date.Date == latestDate.Value).ToList()
                : new List<FinanceAccountBalance>();

            decimal revenue = 0m, costOfSales = 0m, operatingExpense = 0m, otherExpense = 0m;
            decimal totalAssets = 0m, totalLiabilities = 0m;
            decimal cash = 0m, receivables = 0m;
            int unclassified = 0;

            // Expense and revenue magnitudes: direction is already carried by the account's own
            // classification, so a credit-natured revenue account arriving negative (which is how
            // most trial-balance exports write it) must not subtract from revenue.
            var expenseByCategory = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);

            foreach (var balance in current)
            {
                if (!accountMap.TryGetValue(balance.AccountNumber, out var account))
                {
                    unclassified++;
                    continue;
                }

                var amount = Math.Abs(balance.Balance);
                var bucket = ClassifyAccount(account);

                switch (bucket)
                {
                    case AccountBucket.Revenue:
                        revenue += amount;
                        break;
                    case AccountBucket.CostOfSales:
                        costOfSales += amount;
                        AccumulateExpense(expenseByCategory, account, amount);
                        break;
                    case AccountBucket.OperatingExpense:
                        operatingExpense += amount;
                        AccumulateExpense(expenseByCategory, account, amount);
                        break;
                    case AccountBucket.OtherExpense:
                        otherExpense += amount;
                        AccumulateExpense(expenseByCategory, account, amount);
                        break;
                    case AccountBucket.Asset:
                        totalAssets += balance.Balance;
                        if (IsCode(account, "5000")) cash += balance.Balance;
                        if (IsCode(account, "5200")) receivables += balance.Balance;
                        break;
                    case AccountBucket.Liability:
                        totalLiabilities += Math.Abs(balance.Balance);
                        break;
                    case AccountBucket.Equity:
                        break;
                    default:
                        unclassified++;
                        break;
                }
            }

            decimal totalExpenses = costOfSales + operatingExpense + otherExpense;
            decimal grossProfit = revenue - costOfSales;
            decimal netProfit = revenue - totalExpenses;

            double grossMargin = revenue > 0 ? (double)(grossProfit / revenue) * 100.0 : 0.0;
            double netMargin = revenue > 0 ? (double)(netProfit / revenue) * 100.0 : 0.0;
            double expenseRatio = revenue > 0 ? (double)(totalExpenses / revenue) * 100.0 : 0.0;

            var topExpense = expenseByCategory
                .OrderByDescending(kv => kv.Value)
                .Select(kv => (Name: kv.Key, Amount: kv.Value))
                .FirstOrDefault();

            return new FinanceKpisDto
            {
                AsOfDate = latestDate,
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
                TotalAssets = totalAssets,
                TotalLiabilities = totalLiabilities,
                TopExpenseCategoryName = topExpense.Name ?? "--",
                TopExpenseCategoryAmount = topExpense.Amount,
                AccountsInChart = accounts.Count,
                BalancesLoaded = current.Count,
                UnclassifiedBalances = unclassified
            };
        }

        private static bool IsCode(ChartOfAccount account, string prefix) =>
            (account.RsmClassification ?? string.Empty).TrimStart().StartsWith(prefix, StringComparison.Ordinal);

        // "Top expense item" is reported by the most human-readable label the COA offers for that
        // account, falling back through the management and RSM classifications.
        private static void AccumulateExpense(Dictionary<string, decimal> bag, ChartOfAccount account, decimal amount)
        {
            var label = FirstNonEmpty(
                account.RevenueMainClassification,
                account.ManagementClassification,
                account.RsmClassification) ?? "غير مصنف";

            bag[label] = bag.GetValueOrDefault(label) + amount;
        }

        private static string? FirstNonEmpty(params string?[] values) =>
            values.FirstOrDefault(v => !string.IsNullOrWhiteSpace(v))?.Trim();
        #endregion

        #region Chart of accounts: index + CRUD (the Finance page)
        public async Task<PagedResultDto<ChartOfAccountDto>> GetChartOfAccountsPagedAsync(int page, int pageSize, string? search)
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
                    (a.ManagementClassification != null && a.ManagementClassification.Contains(term)));
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
        private static List<FieldErrorDto> ValidateAccount(ChartOfAccountDto dto)
        {
            var errors = new List<FieldErrorDto>();
            if (string.IsNullOrWhiteSpace(dto.AccountNumber)) errors.Add("accountNumber", "رقم الحساب مطلوب.");
            if (string.IsNullOrWhiteSpace(dto.AccountName)) errors.Add("accountName", "اسم الحساب مطلوب.");
            if (string.IsNullOrWhiteSpace(dto.RsmClassification))
                errors.Add("rsmClassification", "تصنيف RSM مطلوب، فهو ما يحدد إن كان الحساب إيراداً أو تكلفة أو بنداً في المركز المالي.");
            return errors;
        }

        private static void ApplyAccount(ChartOfAccountDto dto, ChartOfAccount account)
        {
            account.AccountNumber = Truncate(dto.AccountNumber.Trim(), 50);
            account.AccountName = Truncate(dto.AccountName.Trim(), 250);
            account.RsmClassification = Truncate(dto.RsmClassification.Trim(), 200);
            account.Mapping = TruncateOrNull(dto.Mapping, 200);
            account.BsClassification = TruncateOrNull(dto.BsClassification, 200);
            account.IsClassification = TruncateOrNull(dto.IsClassification, 200);
            account.ManagementClassification = TruncateOrNull(dto.ManagementClassification, 200);
            account.RevenueMainClassification = TruncateOrNull(dto.RevenueMainClassification, 200);
            account.RevenueSubClassification = TruncateOrNull(dto.RevenueSubClassification, 200);
        }
        #endregion

        #region Account balances: CRUD (the Finance page)
        public async Task<FinanceAccountBalanceDto?> GetBalanceAsync(int id)
        {
            var b = await _context.FinanceAccountBalances.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
            if (b == null) return null;
            var classification = await _context.ChartOfAccounts.AsNoTracking()
                .Where(a => a.AccountNumber == b.AccountNumber).Select(a => a.RsmClassification).FirstOrDefaultAsync();
            return ToBalanceDto(b, classification);
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

        // Field-level rules for a balance, shared by the Excel import and the form. The account
        // must exist in the chart of accounts, because an unclassifiable balance silently drops out
        // of every KPI.
        private static List<FieldErrorDto> ValidateBalance(FinanceAccountBalanceDto dto, ICollection<string> knownAccounts)
        {
            var errors = new List<FieldErrorDto>();
            if (dto.Date == default) errors.Add("date", "التاريخ غير مقروء. استخدم الصيغة يوم/شهر/سنة.");
            if (string.IsNullOrWhiteSpace(dto.AccountNumber)) errors.Add("accountNumber", "رقم الحساب مطلوب.");
            else if (!knownAccounts.Contains(dto.AccountNumber.Trim()))
                errors.Add("accountNumber", $"رقم الحساب \"{dto.AccountNumber.Trim()}\" غير موجود في شجرة الحسابات (COA).");
            if (dto.Balance == null) errors.Add("balance", "الرصيد يجب أن يكون رقماً.");
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
                if (await _context.FinanceAccountBalances.AnyAsync(b =>
                        b.Date == day && b.AccountNumber == number && (excludeId == null || b.Id != excludeId)))
                    errors.Add("accountNumber", "يوجد رصيد مسجل لهذا الحساب في نفس التاريخ.");
            }
            return errors;
        }

        // The account name is taken from the chart of accounts when the form leaves it blank, so a
        // hand-entered balance reads the same as an uploaded one.
        private async System.Threading.Tasks.Task ApplyBalanceAsync(FinanceAccountBalanceDto dto, FinanceAccountBalance balance)
        {
            var number = dto.AccountNumber.Trim();
            var name = dto.AccountName;
            if (string.IsNullOrWhiteSpace(name))
                name = await _context.ChartOfAccounts.Where(a => a.AccountNumber == number)
                    .Select(a => a.AccountName).FirstOrDefaultAsync() ?? string.Empty;

            balance.Date = dto.Date.Date;
            balance.AccountNumber = Truncate(number, 50);
            balance.AccountName = Truncate(name.Trim(), 250);
            balance.Balance = dto.Balance!.Value;
        }

        private static FinanceAccountBalanceDto ToBalanceDto(FinanceAccountBalance b, string? classification) => new()
        {
            Id = b.Id,
            Date = b.Date,
            AccountNumber = b.AccountNumber,
            AccountName = b.AccountName,
            Balance = b.Balance,
            Classification = classification
        };
        #endregion

        #region Bulk upload (the approved Finance template: COA + balances in one workbook)
        // Both data sheets of the same uploaded file are read in one pass: the chart of accounts
        // first, so that every balance read afterwards already has an account to be classified
        // through. The engine runs twice over the same workbook because each sheet has its own
        // header contract; a file missing either sheet is rejected with a message naming the tab.
        public async Task<ExcelImportResultDto> BulkUploadFinanceWorkbookAsync(System.IO.Stream excelStream)
        {
            // Buffered once and replayed from a fresh MemoryStream per sheet: the engine opens an
            // XLWorkbook over whatever stream it is handed and disposes it, so the second pass must
            // not depend on the first pass having left the caller's stream usable.
            using var buffer = new System.IO.MemoryStream();
            if (excelStream.CanSeek) excelStream.Position = 0;
            await excelStream.CopyToAsync(buffer);

            var accounts = await _context.ChartOfAccounts.ToListAsync();
            var accountColumns = new Dictionary<string, string>
            {
                ["accountNumber"] = "Account #", ["accountName"] = "Account Name", ["rsmClassification"] = "RSM Classification"
            };

            var coaResult = await ExcelImportEngine.RunAsync(
                new System.IO.MemoryStream(buffer.ToArray()),
                DepartmentTemplates.FinanceChartOfAccounts,
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

            // A workbook whose COA sheet is missing or malformed is rejected before any balance is
            // touched: importing balances against an account tree that failed to load would leave
            // every KPI reading zero with no obvious reason why.
            if (!coaResult.Success) return coaResult;

            var balances = await _context.FinanceAccountBalances.ToListAsync();
            var accountNumbers = new HashSet<string>(accounts.Select(a => a.AccountNumber), StringComparer.OrdinalIgnoreCase);
            var balanceColumns = new Dictionary<string, string>
            {
                ["date"] = "Date", ["accountNumber"] = "Account No", ["balance"] = "Balance"
            };

            var balanceResult = await ExcelImportEngine.RunAsync(
                new System.IO.MemoryStream(buffer.ToArray()),
                DepartmentTemplates.FinanceBalances,
                async row =>
                {
                    var dto = new FinanceAccountBalanceDto
                    {
                        Date = row.GetDate(DepartmentTemplates.FinanceBalanceDate)?.Date ?? default,
                        AccountNumber = row.GetString(DepartmentTemplates.FinanceBalanceAccountNumber),
                        AccountName = row.GetString(DepartmentTemplates.FinanceBalanceAccountName),
                        Balance = row.GetDecimal(DepartmentTemplates.FinanceBalanceAmount)
                    };

                    var errors = ValidateBalance(dto, accountNumbers);
                    if (errors.Count > 0)
                        return ExcelRowOutcomeResult.Skipped(errors.Joined(), balanceColumns.GetValueOrDefault(errors[0].Field));

                    var number = Truncate(dto.AccountNumber.Trim(), 50);
                    var day = dto.Date.Date;
                    var existing = balances.FirstOrDefault(b =>
                        b.Date.Date == day &&
                        string.Equals(b.AccountNumber, number, StringComparison.OrdinalIgnoreCase));

                    if (existing != null)
                    {
                        existing.AccountName = Truncate(dto.AccountName.Trim(), 250);
                        existing.Balance = dto.Balance!.Value;
                        return ExcelRowOutcomeResult.Updated();
                    }

                    var balance = new FinanceAccountBalance
                    {
                        Date = day,
                        AccountNumber = number,
                        AccountName = Truncate(dto.AccountName.Trim(), 250),
                        Balance = dto.Balance!.Value
                    };
                    _context.FinanceAccountBalances.Add(balance);
                    balances.Add(balance);
                    await System.Threading.Tasks.Task.CompletedTask;
                    return ExcelRowOutcomeResult.Inserted();
                },
                () => _context.SaveChangesAsync(),
                _logger);

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
                    ? $"تمت معالجة الملف: {coaResult.DataRows} حساب في شجرة الحسابات و {balanceResult.DataRows} رصيد، مع تجاهل {coaResult.SkippedRows + balanceResult.SkippedRows} صف. التفاصيل بالأسفل."
                    : $"تمت معالجة الملف بنجاح: {coaResult.DataRows} حساب في شجرة الحسابات و {balanceResult.DataRows} رصيد."
            };
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
