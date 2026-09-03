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

        // Bulk upload of the approved single-sheet finance ledger (see DepartmentTemplates.Finance).
        // The stream must already be a readable .xlsx stream - see ExcelCompatibility.EnsureXlsxStream.
        Task<ExcelImportResultDto> BulkUploadTransactionsAsync(System.IO.Stream excelStream);
    }
}
