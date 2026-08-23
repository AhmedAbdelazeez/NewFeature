using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using NewFeature.Models;

namespace NewFeature.Services
{
    // Storage / Warehouse department (إدارة التخزين) service: paginated + date-filtered inventory
    // listing, KPIs, and dynamic Excel bulk-upload. Reuses the existing InventoryItem model
    // (shared with Procurement) but exposes it as its own department module.
    public interface IWarehouseService
    {
        Task<PagedResultDto<InventoryItemDto>> GetInventoryItemsAsync(int page, int pageSize, DateTime? fromDate, DateTime? toDate);
        Task<InventoryItemDto?> GetInventoryItemByIdAsync(int id);
        Task<InventoryItemDto> CreateInventoryItemAsync(InventoryItemDto dto);
        Task<bool> UpdateInventoryItemAsync(InventoryItemDto dto);
        Task<bool> DeleteInventoryItemAsync(int id);
        Task<WarehouseKpisDto> GetWarehouseKpisAsync();
        Task<(int SuccessCount, List<string> Errors)> BulkUploadInventoryAsync(Stream excelStream);
    }
}
