using System.Collections.Generic;
using System.Threading.Tasks;
using NewFeature.Models;

namespace NewFeature.Services
{
    public interface IHseService
    {
        // Incidents CRUD
        Task<IEnumerable<HseIncidentDto>> GetAllIncidentsAsync();
        Task<HseIncidentDto?> GetIncidentByIdAsync(int id);
        Task<HseIncidentDto> CreateIncidentAsync(HseIncidentDto dto);
        Task<bool> UpdateIncidentAsync(HseIncidentDto dto);
        Task<bool> DeleteIncidentAsync(int id);

        // Incidents - paginated + searchable listing for the HSE page's table
        Task<PagedResultDto<HseIncidentDto>> GetIncidentsPagedAsync(int page, int pageSize, string? search, string? type);

        // Inspections CRUD
        Task<IEnumerable<HseInspectionDto>> GetAllInspectionsAsync();
        Task<HseInspectionDto?> GetInspectionByIdAsync(int id);
        Task<HseInspectionDto> CreateInspectionAsync(HseInspectionDto dto);
        Task<bool> UpdateInspectionAsync(HseInspectionDto dto);
        Task<bool> DeleteInspectionAsync(int id);

        // Inspections - paginated + searchable listing for the HSE page's table
        Task<PagedResultDto<HseInspectionDto>> GetInspectionsPagedAsync(int page, int pageSize, string? search);

        // KPIs
        Task<HseKpisDto> GetHseKpisAsync();
    }
}
