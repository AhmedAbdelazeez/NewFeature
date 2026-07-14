using System.Collections.Generic;
using System.Threading.Tasks;
using NewFeature.Models;

namespace NewFeature.Services
{
    public interface IProcurementService
    {
        // Requests
        Task<IEnumerable<ProcurementRequestDto>> GetAllRequestsAsync();
        Task<ProcurementRequestDto?> GetRequestByIdAsync(int id);
        Task<ProcurementRequestDto> CreateRequestAsync(ProcurementRequestDto dto);
        Task<bool> UpdateRequestAsync(ProcurementRequestDto dto);
        Task<bool> DeleteRequestAsync(int id);

        // Inventory
        Task<IEnumerable<InventoryItemDto>> GetAllInventoryItemsAsync();
        Task<InventoryItemDto?> GetInventoryItemByIdAsync(int id);
        Task<InventoryItemDto> CreateInventoryItemAsync(InventoryItemDto dto);
        Task<bool> UpdateInventoryItemAsync(InventoryItemDto dto);
        Task<bool> DeleteInventoryItemAsync(int id);

        // KPIs
        Task<ProcurementKpisDto> GetProcurementKpisAsync();
    }
}
