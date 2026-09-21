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

        // KPIs
        Task<FinanceKpisDto> GetFinanceKpisAsync();

        // Chart of accounts + account balances, from the approved Finance template.
        Task<IEnumerable<ChartOfAccountDto>> GetChartOfAccountsAsync();
        Task<PagedResultDto<FinanceAccountBalanceDto>> GetAccountBalancesPagedAsync(int page, int pageSize, string? search, DateTime? asOfDate);

        // Index + CRUD for the Finance page: the chart of accounts and the account balances.
        Task<PagedResultDto<ChartOfAccountDto>> GetChartOfAccountsPagedAsync(int page, int pageSize, string? search);
        Task<ChartOfAccountDto?> GetAccountAsync(int id);
        Task<CrudResult<ChartOfAccountDto>> CreateAccountAsync(ChartOfAccountDto dto);
        Task<CrudResult<ChartOfAccountDto>> UpdateAccountAsync(int id, ChartOfAccountDto dto);
        Task<CrudResult<ChartOfAccountDto>> DeleteAccountAsync(int id);
        Task<FinanceAccountBalanceDto?> GetBalanceAsync(int id);
        Task<CrudResult<FinanceAccountBalanceDto>> CreateBalanceAsync(FinanceAccountBalanceDto dto);
        Task<CrudResult<FinanceAccountBalanceDto>> UpdateBalanceAsync(int id, FinanceAccountBalanceDto dto);
        Task<bool> DeleteBalanceAsync(int id);

        // Bulk upload of the approved Finance template: one workbook carrying both the COA sheet
        // and the الحركات المالية balances sheet. The stream must already be a readable .xlsx
        // stream and seekable - see ExcelCompatibility.EnsureXlsxStream - because both sheets are
        // read from it in turn.
        Task<ExcelImportResultDto> BulkUploadFinanceWorkbookAsync(System.IO.Stream excelStream);
    }
}
