using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using NewFeature.Models;

namespace NewFeature.Services
{
    public interface IMaintenanceService
    {
        Task<IEnumerable<MaintenanceWorkOrderDto>> GetAllWorkOrdersAsync();
        Task<PagedResultDto<MaintenanceWorkOrderDto>> GetWorkOrdersPagedAsync(int page, int pageSize, System.DateTime? fromDate, System.DateTime? toDate);
        Task<MaintenanceWorkOrderDto?> GetWorkOrderByIdAsync(int id);
        Task<MaintenanceWorkOrderDto> CreateWorkOrderAsync(MaintenanceWorkOrderDto dto);
        Task<bool> UpdateWorkOrderAsync(MaintenanceWorkOrderDto dto);
        Task<bool> DeleteWorkOrderAsync(int id);
        Task<MaintenanceKpisDto> GetMaintenanceKpisAsync();
        Task<(int SuccessCount, List<string> Errors)> BulkUploadWorkshopLogsAsync(Stream excelStream, string branchName);
    }
}
