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

        // Budgets CRUD
        Task<IEnumerable<FinanceBudgetDto>> GetAllBudgetsAsync();
        Task<FinanceBudgetDto?> GetBudgetByIdAsync(int id);
        Task<FinanceBudgetDto> CreateBudgetAsync(FinanceBudgetDto dto);
        Task<bool> UpdateBudgetAsync(FinanceBudgetDto dto);
        Task<bool> DeleteBudgetAsync(int id);

        // KPIs
        Task<FinanceKpisDto> GetFinanceKpisAsync();
    }
}
