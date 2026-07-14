using System.Collections.Generic;
using System.Threading.Tasks;
using NewFeature.Models;

namespace NewFeature.Services
{
    public interface IStrategyService
    {
        // Goals
        Task<IEnumerable<StrategicGoalDto>> GetAllGoalsAsync();
        Task<StrategicGoalDto?> GetGoalByIdAsync(int id);
        Task<StrategicGoalDto> CreateGoalAsync(StrategicGoalDto dto);
        Task<bool> UpdateGoalAsync(StrategicGoalDto dto);
        Task<bool> DeleteGoalAsync(int id);

        // PMO Initiatives
        Task<IEnumerable<PmoInitiativeDto>> GetAllInitiativesAsync();
        Task<PmoInitiativeDto?> GetInitiativeByIdAsync(int id);
        Task<PmoInitiativeDto> CreateInitiativeAsync(PmoInitiativeDto dto);
        Task<bool> UpdateInitiativeAsync(PmoInitiativeDto dto);
        Task<bool> DeleteInitiativeAsync(int id);

        // KPIs
        Task<StrategyKpisDto> GetStrategyKpisAsync();
    }
}
