using System;
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

        // Paged, searchable listing of the real uploaded trip log, for the Operations landing page's
        // browsable table (mirrors GetVehiclesPagedAsync/maintenance-workorders paging patterns).
        Task<PagedResultDto<OperationsTripDto>> GetTripsPagedAsync(int page, int pageSize, string? search, DateTime? fromDate, DateTime? toDate);

        // Paged, searchable listing of the official-drivers compliance roster, for the Operations
        // landing page's browsable table.
        Task<PagedResultDto<OperationsDriverDto>> GetDriversPagedAsync(int page, int pageSize, string? search);

        // Bulk Upload
        Task<(int SuccessCount, List<string> Errors)> BulkUploadDailyPlansAsync(System.IO.Stream excelStream);

        // Bulk Upload of real daily operations/trip logs (e.g. "تشغيل شهر ..." monthly sheets).
        // Dynamically maps columns and creates/links Vehicles, Routes, Drivers and Trip records.
        Task<(int SuccessCount, List<string> Errors)> BulkUploadOperationsTripsAsync(System.IO.Stream excelStream);

        // Replaces the official-drivers compliance roster snapshot from the real Excel export.
        Task<(int SuccessCount, List<string> Errors)> BulkUploadOfficialDriversAsync(System.IO.Stream excelStream);

        // Replaces the route-scheduling requests snapshot from the real Excel export.
        Task<(int SuccessCount, List<string> Errors)> BulkUploadRouteSchedulesAsync(System.IO.Stream excelStream);
    }
}
