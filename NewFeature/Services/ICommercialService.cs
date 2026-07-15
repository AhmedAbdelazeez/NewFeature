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

        // Leads CRUD
        Task<IEnumerable<CommercialLeadDto>> GetAllLeadsAsync();
        Task<CommercialLeadDto?> GetLeadByIdAsync(int id);
        Task<CommercialLeadDto> CreateLeadAsync(CommercialLeadDto dto);
        Task<bool> UpdateLeadAsync(CommercialLeadDto dto);
        Task<bool> DeleteLeadAsync(int id);

        // KPIs
        Task<CommercialKpisDto> GetCommercialKpisAsync();
    }
}
