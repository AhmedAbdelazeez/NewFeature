using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NewFeature.Models;
using NewFeature.Services.Repositories;

namespace NewFeature.Services
{
    public class StrategyService : IStrategyService
    {
        private readonly ApplicationDbContext _context;

        public StrategyService(ApplicationDbContext context)
        {
            _context = context;
        }

        #region Goals CRUD
        public async Task<IEnumerable<StrategicGoalDto>> GetAllGoalsAsync()
        {
            var goals = await _context.StrategicGoals.OrderByDescending(g => g.Weight).ToListAsync();
            return goals.Select(MapToGoalDto);
        }

        // Paginated + search listing for the Strategy page's Strategic Goals table (mirrors the
        // "fetch everything, then filter/order/Skip/Take in memory" pattern used by
        // FleetService.GetVehiclesPagedAsync). GetAllGoalsAsync above is left untouched.
        public async Task<PagedResultDto<StrategicGoalDto>> GetGoalsPagedAsync(int page, int pageSize, string? search)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 20;
            if (pageSize > 200) pageSize = 200;

            var goals = (await _context.StrategicGoals.ToListAsync()).AsEnumerable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                goals = goals.Where(g => Contains(g.TitleEn, term) || Contains(g.TitleAr, term) || Contains(g.Status, term));
            }

            var ordered = goals.OrderByDescending(g => g.Weight).ToList();
            var totalCount = ordered.Count;

            var items = ordered
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(MapToGoalDto)
                .ToList();

            return new PagedResultDto<StrategicGoalDto>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        private static bool Contains(string? haystack, string term) =>
            !string.IsNullOrEmpty(haystack) && haystack.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0;

        public async Task<StrategicGoalDto?> GetGoalByIdAsync(int id)
        {
            var goal = await _context.StrategicGoals.FindAsync(id);
            if (goal == null) return null;
            return MapToGoalDto(goal);
        }

        public async Task<StrategicGoalDto> CreateGoalAsync(StrategicGoalDto dto)
        {
            var goal = new StrategicGoal
            {
                TitleEn = dto.TitleEn,
                TitleAr = dto.TitleAr,
                Weight = dto.Weight,
                Progress = dto.Progress,
                Status = dto.Status,
                TargetDate = dto.TargetDate == default ? DateTime.UtcNow.AddYears(1) : dto.TargetDate
            };

            _context.StrategicGoals.Add(goal);
            await _context.SaveChangesAsync();
            return MapToGoalDto(goal);
        }

        public async Task<bool> UpdateGoalAsync(StrategicGoalDto dto)
        {
            var goal = await _context.StrategicGoals.FindAsync(dto.Id);
            if (goal == null) return false;

            goal.TitleEn = dto.TitleEn;
            goal.TitleAr = dto.TitleAr;
            goal.Weight = dto.Weight;
            goal.Progress = dto.Progress;
            goal.Status = dto.Status;
            goal.TargetDate = dto.TargetDate;

            _context.Entry(goal).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteGoalAsync(int id)
        {
            var goal = await _context.StrategicGoals.FindAsync(id);
            if (goal == null) return false;

            _context.StrategicGoals.Remove(goal);
            await _context.SaveChangesAsync();
            return true;
        }
        #endregion

        #region PMO Initiatives CRUD
        public async Task<IEnumerable<PmoInitiativeDto>> GetAllInitiativesAsync()
        {
            var initiatives = await _context.PmoInitiatives.OrderByDescending(i => i.StartDate).ToListAsync();
            return initiatives.Select(MapToInitiativeDto);
        }

        // Paginated + search listing for the Strategy page's PMO Initiatives table.
        public async Task<PagedResultDto<PmoInitiativeDto>> GetInitiativesPagedAsync(int page, int pageSize, string? search)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 20;
            if (pageSize > 200) pageSize = 200;

            var initiatives = (await _context.PmoInitiatives.ToListAsync()).AsEnumerable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                initiatives = initiatives.Where(i =>
                    Contains(i.TitleEn, term) ||
                    Contains(i.TitleAr, term) ||
                    Contains(i.ManagerName, term) ||
                    Contains(i.Status, term));
            }

            var ordered = initiatives.OrderByDescending(i => i.StartDate).ToList();
            var totalCount = ordered.Count;

            var items = ordered
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(MapToInitiativeDto)
                .ToList();

            return new PagedResultDto<PmoInitiativeDto>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<PmoInitiativeDto?> GetInitiativeByIdAsync(int id)
        {
            var init = await _context.PmoInitiatives.FindAsync(id);
            if (init == null) return null;
            return MapToInitiativeDto(init);
        }

        public async Task<PmoInitiativeDto> CreateInitiativeAsync(PmoInitiativeDto dto)
        {
            var init = new PmoInitiative
            {
                TitleEn = dto.TitleEn,
                TitleAr = dto.TitleAr,
                ManagerName = dto.ManagerName,
                Progress = dto.Progress,
                Budget = dto.Budget,
                Status = dto.Status,
                StartDate = dto.StartDate == default ? DateTime.UtcNow : dto.StartDate,
                EndDate = dto.EndDate == default ? DateTime.UtcNow.AddMonths(6) : dto.EndDate,
                GovernanceMaturityScore = dto.GovernanceMaturityScore,
                MilestoneOnTime = dto.MilestoneOnTime
            };

            _context.PmoInitiatives.Add(init);
            await _context.SaveChangesAsync();
            return MapToInitiativeDto(init);
        }

        public async Task<bool> UpdateInitiativeAsync(PmoInitiativeDto dto)
        {
            var init = await _context.PmoInitiatives.FindAsync(dto.Id);
            if (init == null) return false;

            init.TitleEn = dto.TitleEn;
            init.TitleAr = dto.TitleAr;
            init.ManagerName = dto.ManagerName;
            init.Progress = dto.Progress;
            init.Budget = dto.Budget;
            init.Status = dto.Status;
            init.StartDate = dto.StartDate;
            init.EndDate = dto.EndDate;
            init.GovernanceMaturityScore = dto.GovernanceMaturityScore;
            init.MilestoneOnTime = dto.MilestoneOnTime;

            _context.Entry(init).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteInitiativeAsync(int id)
        {
            var init = await _context.PmoInitiatives.FindAsync(id);
            if (init == null) return false;

            _context.PmoInitiatives.Remove(init);
            await _context.SaveChangesAsync();
            return true;
        }
        #endregion

        #region KPIs Calculation
        public async Task<StrategyKpisDto> GetStrategyKpisAsync()
        {
            var goals = await _context.StrategicGoals.ToListAsync();
            var initiatives = await _context.PmoInitiatives.ToListAsync();

            // 1. Strategic Goals Achievement
            double goalsAchieve = 82.0; // target 85%
            if (goals.Any())
            {
                goalsAchieve = Math.Round(goals.Average(g => g.Progress), 1);
            }

            // 2. PMO Initiative Delivery
            double initDelivery = 85.0; // target 90%
            if (initiatives.Any())
            {
                initDelivery = Math.Round(initiatives.Average(i => i.Progress), 1);
            }

            // 3. Enterprise Risk Mitigation
            double riskHandling = 88.0; // target 90%
            if (initiatives.Any())
            {
                // portion with status completed or progress > 50
                int mitigated = initiatives.Count(i => i.Status == "Completed" || i.Progress >= 50.0);
                riskHandling = Math.Round((double)mitigated / initiatives.Count * 100, 1);
            }

            // 4. Corporate Governance Maturity
            double govMaturity = 94.0; // target 95%
            if (initiatives.Any())
            {
                double avgMaturity = initiatives.Average(i => i.GovernanceMaturityScore);
                govMaturity = Math.Round((avgMaturity / 5.0) * 100, 1);
            }

            // 5. Combined Strategic Performance Index
            double combinedIndex = 85.0; // target 85%
            if (goals.Any() || initiatives.Any())
            {
                double sum = 0;
                int count = 0;
                if (goals.Any())
                {
                    sum += goals.Average(g => g.Progress);
                    count++;
                }
                if (initiatives.Any())
                {
                    sum += initiatives.Average(i => i.Progress);
                    count++;
                }
                combinedIndex = Math.Round(sum / count, 1);
            }

            // 6. On-Time PMO Milestones Completion
            double onTimeMilestones = 90.0; // target 90%
            if (initiatives.Any())
            {
                int onTime = initiatives.Count(i => i.MilestoneOnTime);
                onTimeMilestones = Math.Round((double)onTime / initiatives.Count * 100, 1);
            }

            // 7. Strategic Budget Variance/Efficiency
            double budgetEfficiency = 95.0; // target 95%
            if (initiatives.Any())
            {
                // Portions within budget (mocked as non-delayed/non-critical status)
                int withinBudget = initiatives.Count(i => i.Status != "Delayed");
                budgetEfficiency = Math.Round((double)withinBudget / initiatives.Count * 100, 1);
            }

            return new StrategyKpisDto
            {
                StrategicGoalsAchievementActual = goalsAchieve,
                StrategicGoalsAchievementTarget = 85.0,

                PmoInitiativeDeliveryActual = initDelivery,
                PmoInitiativeDeliveryTarget = 90.0,

                RiskHandlingActual = riskHandling,
                RiskHandlingTarget = 90.0,

                GovMaturityActual = govMaturity,
                GovMaturityTarget = 95.0,

                StrategicGoalsAchieveMinedActual = combinedIndex,
                StrategicGoalsAchieveMinedTarget = 85.0,

                OnTimeMilestonesDeliveryActual = onTimeMilestones,
                OnTimeMilestonesDeliveryTarget = 90.0,

                StrategicBudgetEfficiencyActual = budgetEfficiency,
                StrategicBudgetEfficiencyTarget = 95.0
            };
        }
        #endregion

        #region Mappers
        private static StrategicGoalDto MapToGoalDto(StrategicGoal g)
        {
            return new StrategicGoalDto
            {
                Id = g.Id,
                TitleEn = g.TitleEn,
                TitleAr = g.TitleAr,
                Weight = g.Weight,
                Progress = g.Progress,
                Status = g.Status,
                TargetDate = g.TargetDate
            };
        }

        private static PmoInitiativeDto MapToInitiativeDto(PmoInitiative i)
        {
            return new PmoInitiativeDto
            {
                Id = i.Id,
                TitleEn = i.TitleEn,
                TitleAr = i.TitleAr,
                ManagerName = i.ManagerName,
                Progress = i.Progress,
                Budget = i.Budget,
                Status = i.Status,
                StartDate = i.StartDate,
                EndDate = i.EndDate,
                GovernanceMaturityScore = i.GovernanceMaturityScore,
                MilestoneOnTime = i.MilestoneOnTime
            };
        }
        #endregion
    }
}
