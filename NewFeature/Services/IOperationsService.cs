using System.Collections.Generic;
using System.Threading.Tasks;
using NewFeature.Models;

namespace NewFeature.Services
{
    public interface IOperationsService
    {
        // Daily Plans CRUD
        Task<IEnumerable<OperationsDailyPlanDto>> GetAllDailyPlansAsync();
        Task<OperationsDailyPlanDto?> GetDailyPlanByIdAsync(int id);
        Task<OperationsDailyPlanDto> CreateDailyPlanAsync(OperationsDailyPlanDto dto);
        Task<bool> UpdateDailyPlanAsync(OperationsDailyPlanDto dto);
        Task<bool> DeleteDailyPlanAsync(int id);

        // Incidents/Violations CRUD
        Task<IEnumerable<OperationsIncidentDto>> GetAllIncidentsAsync();
        Task<OperationsIncidentDto?> GetIncidentByIdAsync(int id);
        Task<OperationsIncidentDto> CreateIncidentAsync(OperationsIncidentDto dto);
        Task<bool> UpdateIncidentAsync(OperationsIncidentDto dto);
        Task<bool> DeleteIncidentAsync(int id);

        // KPIs calculation
        Task<OperationsKpisDto> GetOperationsKpisAsync();

        // Bulk Upload
        Task<(int SuccessCount, List<string> Errors)> BulkUploadDailyPlansAsync(System.IO.Stream excelStream);
    }
}
