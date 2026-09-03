using System.Collections.Generic;
using System.Threading.Tasks;
using NewFeature.Models;

namespace NewFeature.Services
{
    public interface IProjectService
    {
        Task<IEnumerable<ProjectDto>> GetAllProjectsAsync();
        Task<PagedResultDto<ProjectDto>> GetProjectsPagedAsync(int page, int pageSize, string? search);
        Task<ProjectDto?> GetProjectByIdAsync(int id);
        Task<ProjectDto> CreateProjectAsync(ProjectDto dto);
        Task<bool> UpdateProjectAsync(ProjectDto dto);
        Task<bool> DeleteProjectAsync(int id);
        Task<ProjectDetailsDto?> GetProjectDetailsAsync(int id);

        // Bulk upload of the approved single-sheet projects register (see DepartmentTemplates.Projects).
        // The stream must already be a readable .xlsx stream - see ExcelCompatibility.EnsureXlsxStream.
        Task<ExcelImportResultDto> BulkUploadProjectsAsync(System.IO.Stream excelStream);
    }
}
