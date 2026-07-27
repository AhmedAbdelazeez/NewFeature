using System.Collections.Generic;
using System.Threading.Tasks;
using NewFeature.Models;

namespace NewFeature.Services
{
    public interface IMohuStandardsService
    {
        Task<List<MohuKpiDto>> GetAllKpisAsync();
        Task<List<MohuKpiDto>> GetKpisByCategoryAsync(string categoryCode);
        Task<MohuKpiDto?> GetKpiByCodeAsync(string kpiCode);
        Task<bool> UpdateKpiValuesAsync(string kpiCode, UpdateMohuKpiDto dto);
        Task<MohuKpiDto> CreateKpiAsync(MohuKpiDto dto);
        Task<bool> DeleteKpiAsync(string kpiCode);
        Task<bool> InitializeDefaultKpisAsync();
        
        // Business Data (Pilgrim Groups)
        Task<List<MohuPilgrimGroupDto>> GetAllPilgrimGroupsAsync();
        Task<MohuPilgrimGroupDto> CreatePilgrimGroupAsync(MohuPilgrimGroupDto dto);
        Task<bool> UpdatePilgrimGroupAsync(int id, MohuPilgrimGroupDto dto);
        Task<bool> DeletePilgrimGroupAsync(int id);
        
        // Business Data (Feedback)
        Task<List<MohuFeedbackDto>> GetAllFeedbacksAsync();
        Task<MohuFeedbackDto> CreateFeedbackAsync(MohuFeedbackDto dto);
        Task<bool> UpdateFeedbackAsync(int id, MohuFeedbackDto dto);
        Task<bool> DeleteFeedbackAsync(int id);
        
        // Business Data (Violations)
        Task<List<MohuViolationRecordDto>> GetAllViolationsAsync();
        Task<MohuViolationRecordDto> CreateViolationAsync(MohuViolationRecordDto dto);
        Task<bool> UpdateViolationAsync(int id, MohuViolationRecordDto dto);
        Task<bool> DeleteViolationAsync(int id);
        
        // Business Data (Permits)
        Task<List<MohuPermitLogDto>> GetAllPermitsAsync();
        Task<MohuPermitLogDto> CreatePermitAsync(MohuPermitLogDto dto);
        Task<bool> UpdatePermitAsync(int id, MohuPermitLogDto dto);
        Task<bool> DeletePermitAsync(int id);
    }
}
