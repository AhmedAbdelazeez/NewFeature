using System.Collections.Generic;
using System.Threading.Tasks;
using NewFeature.Models;

namespace NewFeature.Services
{
    public interface ICommercialService
    {
        // Contracts CRUD
        Task<IEnumerable<CommercialContractDto>> GetAllContractsAsync();
        Task<CommercialContractDto?> GetContractByIdAsync(int id);
        Task<CommercialContractDto> CreateContractAsync(CommercialContractDto dto);
        Task<bool> UpdateContractAsync(CommercialContractDto dto);
        Task<bool> DeleteContractAsync(int id);

        // Contracts - paginated + searchable listing for the Commercial page's table
        Task<PagedResultDto<CommercialContractDto>> GetContractsPagedAsync(int page, int pageSize, string? search, string? status);

        // Leads CRUD
        Task<IEnumerable<CommercialLeadDto>> GetAllLeadsAsync();
        Task<CommercialLeadDto?> GetLeadByIdAsync(int id);
        Task<CommercialLeadDto> CreateLeadAsync(CommercialLeadDto dto);
        Task<bool> UpdateLeadAsync(CommercialLeadDto dto);
        Task<bool> DeleteLeadAsync(int id);

        // Leads - paginated + searchable listing for the Commercial page's table
        Task<PagedResultDto<CommercialLeadDto>> GetLeadsPagedAsync(int page, int pageSize, string? search, string? status);

        // KPIs
        Task<CommercialKpisDto> GetCommercialKpisAsync();
    }
}
