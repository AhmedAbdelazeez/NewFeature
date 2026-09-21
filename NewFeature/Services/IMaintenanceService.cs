using System.IO;
using System.Threading.Tasks;
using NewFeature.Models;

namespace NewFeature.Services
{
    public interface IMaintenanceService
    {
        // Index + CRUD over the uploaded work orders, for the Maintenance page. The paged listing is
        // also what the executive dashboard's work-orders table reads.
        Task<PagedResultDto<MaintenanceWorkOrderDto>> GetWorkOrdersPagedAsync(int page, int pageSize, System.DateTime? fromDate, System.DateTime? toDate, string? search = null);
        Task<MaintenanceWorkOrderDto?> GetWorkOrderByIdAsync(int id);
        Task<CrudResult<MaintenanceWorkOrderDto>> CreateWorkOrderAsync(MaintenanceWorkOrderDto dto);
        Task<CrudResult<MaintenanceWorkOrderDto>> UpdateWorkOrderAsync(int id, MaintenanceWorkOrderDto dto);
        Task<bool> DeleteWorkOrderAsync(int id);

        Task<MaintenanceKpisDto> GetMaintenanceKpisAsync();

        // Imports the approved single-sheet "Internal work orders" template. Returns the shared
        // row-level import result so the uploader is told exactly which row and column failed.
        Task<ExcelImportResultDto> BulkUploadWorkOrdersAsync(Stream excelStream, string branchName);
    }
}
