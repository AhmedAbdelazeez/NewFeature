using System.Collections.Generic;
using System.Threading.Tasks;
using NewFeature.Models;

namespace NewFeature.Services
{
    public interface IComplianceService
    {
        // Department Lookup CRUD
        Task<IEnumerable<DepartmentDto>> GetAllDepartmentsAsync();
        Task<DepartmentDto?> GetDepartmentByIdAsync(int id);
        Task<DepartmentDto> CreateDepartmentAsync(DepartmentDto dto);
        Task<bool> UpdateDepartmentAsync(DepartmentDto dto);
        Task<bool> DeleteDepartmentAsync(int id);

        // Violation Classification Lookup CRUD
        Task<IEnumerable<ViolationClassificationDto>> GetAllClassificationsAsync();
        Task<ViolationClassificationDto?> GetClassificationByIdAsync(int id);
        Task<ViolationClassificationDto> CreateClassificationAsync(ViolationClassificationDto dto);
        Task<bool> UpdateClassificationAsync(ViolationClassificationDto dto);
        Task<bool> DeleteClassificationAsync(int id);

        // Violation CRUD
        Task<IEnumerable<ViolationDto>> GetAllViolationsAsync();
        Task<ViolationDto?> GetViolationByIdAsync(int id);
        Task<ViolationDto> CreateViolationAsync(ViolationDto dto);
        Task<bool> UpdateViolationAsync(ViolationDto dto);
        Task<bool> DeleteViolationAsync(int id);

        // Internal Audit CRUD
        Task<IEnumerable<InternalAuditDto>> GetAllAuditsAsync();
        Task<InternalAuditDto?> GetAuditByIdAsync(int id);
        Task<InternalAuditDto> CreateAuditAsync(InternalAuditDto dto);
        Task<bool> UpdateAuditAsync(InternalAuditDto dto);
        Task<bool> DeleteAuditAsync(int id);

        // Improvement Action CRUD
        Task<IEnumerable<ImprovementActionDto>> GetAllImprovementsAsync();
        Task<ImprovementActionDto?> GetImprovementByIdAsync(int id);
        Task<ImprovementActionDto> CreateImprovementAsync(ImprovementActionDto dto);
        Task<bool> UpdateImprovementAsync(ImprovementActionDto dto);
        Task<bool> DeleteImprovementAsync(int id);

        // KPIs dashboard
        Task<ComplianceKpisDto> GetComplianceKpisAsync();
    }
}
