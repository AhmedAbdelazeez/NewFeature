using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NewFeature.Models;

namespace NewFeature.Services
{
    public interface IFinanceService
    {
        // Transactions CRUD
        Task<IEnumerable<FinanceTransactionDto>> GetAllTransactionsAsync();
        Task<FinanceTransactionDto?> GetTransactionByIdAsync(int id);
        Task<FinanceTransactionDto> CreateTransactionAsync(FinanceTransactionDto dto);
        Task<bool> UpdateTransactionAsync(FinanceTransactionDto dto);
        Task<bool> DeleteTransactionAsync(int id);

        // Transactions - paginated + searchable listing for the Finance page's table
        Task<PagedResultDto<FinanceTransactionDto>> GetTransactionsPagedAsync(int page, int pageSize, string? search, string? type);

        // Budgets CRUD
        Task<IEnumerable<FinanceBudgetDto>> GetAllBudgetsAsync();
        Task<FinanceBudgetDto?> GetBudgetByIdAsync(int id);
        Task<FinanceBudgetDto> CreateBudgetAsync(FinanceBudgetDto dto);
        Task<bool> UpdateBudgetAsync(FinanceBudgetDto dto);
        Task<bool> DeleteBudgetAsync(int id);

        // Budgets - paginated + searchable listing for the Finance page's table
        Task<PagedResultDto<FinanceBudgetDto>> GetBudgetsPagedAsync(int page, int pageSize, string? search);

        // KPIs. A branch narrows every figure to that فرع; null reports the company as a whole.
        Task<FinanceKpisDto> GetFinanceKpisAsync(string? branch = null);

        // Chart of accounts + account balances, from the two approved Finance templates.
        Task<IEnumerable<ChartOfAccountDto>> GetChartOfAccountsAsync();
        Task<PagedResultDto<FinanceAccountBalanceDto>> GetAccountBalancesPagedAsync(
            int page, int pageSize, string? search, DateTime? asOfDate, FinanceBalanceFilter? filter = null);

        // Index + CRUD for the two Finance pages: the account tree and the monthly figures.
        Task<PagedResultDto<ChartOfAccountDto>> GetChartOfAccountsPagedAsync(
            int page, int pageSize, string? search, ChartOfAccountFilter? filter = null);

        // The values each page's filter dropdowns offer, read from the uploaded data itself.
        Task<FinanceAccountFilterOptionsDto> GetChartOfAccountFilterOptionsAsync();
        Task<FinanceBalanceFilterOptionsDto> GetBalanceFilterOptionsAsync();
        Task<ChartOfAccountDto?> GetAccountAsync(int id);
        Task<CrudResult<ChartOfAccountDto>> CreateAccountAsync(ChartOfAccountDto dto);
        Task<CrudResult<ChartOfAccountDto>> UpdateAccountAsync(int id, ChartOfAccountDto dto);
        Task<CrudResult<ChartOfAccountDto>> DeleteAccountAsync(int id);
        Task<FinanceAccountBalanceDto?> GetBalanceAsync(int id);
        Task<CrudResult<FinanceAccountBalanceDto>> CreateBalanceAsync(FinanceAccountBalanceDto dto);
        Task<CrudResult<FinanceAccountBalanceDto>> UpdateBalanceAsync(int id, FinanceAccountBalanceDto dto);
        Task<bool> DeleteBalanceAsync(int id);

        // The two approved uploads: the account tree, and one month's figures. Each stream must
        // already be a readable, seekable .xlsx stream - see ExcelCompatibility.EnsureXlsxStream.
        Task<ExcelImportResultDto> BulkUploadChartOfAccountsAsync(System.IO.Stream excelStream);
        Task<ExcelImportResultDto> BulkUploadBalancesAsync(System.IO.Stream excelStream);

        // The older single-workbook upload carrying both the COA sheet and the الحركات المالية
        // sheet, kept so a file prepared against the previous template still imports.
        Task<ExcelImportResultDto> BulkUploadFinanceWorkbookAsync(System.IO.Stream excelStream);
    }
}
