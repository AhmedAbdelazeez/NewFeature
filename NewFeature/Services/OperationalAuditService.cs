using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using NewFeature.Models;
using NewFeature.Services.Repositories;

namespace NewFeature.Services
{
    public class OperationalAuditService : IOperationalAuditService
    {
        private readonly IRepository<OperationalAudit> _auditRepository;
        private readonly IRepository<Department> _departmentRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public OperationalAuditService(
            IRepository<OperationalAudit> auditRepository,
            IRepository<Department> departmentRepository,
            IHttpContextAccessor httpContextAccessor)
        {
            _auditRepository = auditRepository;
            _departmentRepository = departmentRepository;
            _httpContextAccessor = httpContextAccessor;
        }

        private bool IsArabic()
        {
            var context = _httpContextAccessor.HttpContext;
            if (context != null)
            {
                if (context.Request.Headers.TryGetValue("Accept-Language", out var lang))
                {
                    if (lang.ToString().ToLower().Contains("ar")) return true;
                }
                if (context.Request.Headers.TryGetValue("X-Language", out var xLang))
                {
                    if (xLang.ToString().ToLower().Contains("ar")) return true;
                }
            }
            return false;
        }

        public async Task<IEnumerable<OperationalAuditDto>> GetAllAuditsAsync()
        {
            var audits = await _auditRepository.GetAllAsync();
            var departments = await _departmentRepository.GetAllAsync();
            var isAr = IsArabic();

            var deptMap = departments.ToDictionary(d => d.Id, d => isAr ? d.NameAr : d.NameEn);

            return audits.OrderByDescending(a => a.Id).Select(a => new OperationalAuditDto
            {
                Id = a.Id,
                TitleEn = a.TitleEn,
                TitleAr = a.TitleAr,
                AuditDate = a.AuditDate,
                DepartmentId = a.DepartmentId,
                DepartmentName = deptMap.TryGetValue(a.DepartmentId, out var name) ? name : "Unknown",
                AuditedProcessCount = a.AuditedProcessCount,
                PassedProcessCount = a.PassedProcessCount,
                CriticalFindingsCount = a.CriticalFindingsCount,
                RecommendationsCount = a.RecommendationsCount,
                RiskMitigationRate = a.RiskMitigationRate,
                Status = a.Status,
                Title = isAr ? a.TitleAr : a.TitleEn
            }).ToList();
        }

        public async Task<OperationalAuditDto?> GetAuditByIdAsync(int id)
        {
            var a = await _auditRepository.GetByIdAsync(id);
            if (a == null) return null;

            var dept = await _departmentRepository.GetByIdAsync(a.DepartmentId);
            var isAr = IsArabic();

            return new OperationalAuditDto
            {
                Id = a.Id,
                TitleEn = a.TitleEn,
                TitleAr = a.TitleAr,
                AuditDate = a.AuditDate,
                DepartmentId = a.DepartmentId,
                DepartmentName = dept != null ? (isAr ? dept.NameAr : dept.NameEn) : "Unknown",
                AuditedProcessCount = a.AuditedProcessCount,
                PassedProcessCount = a.PassedProcessCount,
                CriticalFindingsCount = a.CriticalFindingsCount,
                RecommendationsCount = a.RecommendationsCount,
                RiskMitigationRate = a.RiskMitigationRate,
                Status = a.Status,
                Title = isAr ? a.TitleAr : a.TitleEn
            };
        }

        public async Task<OperationalAuditDto> CreateAuditAsync(OperationalAuditDto dto)
        {
            var a = new OperationalAudit
            {
                TitleEn = dto.TitleEn,
                TitleAr = dto.TitleAr,
                AuditDate = dto.AuditDate,
                DepartmentId = dto.DepartmentId,
                AuditedProcessCount = dto.AuditedProcessCount,
                PassedProcessCount = dto.PassedProcessCount,
                CriticalFindingsCount = dto.CriticalFindingsCount,
                RecommendationsCount = dto.RecommendationsCount,
                RiskMitigationRate = dto.RiskMitigationRate,
                Status = dto.Status
            };

            await _auditRepository.AddAsync(a);
            await _auditRepository.SaveChangesAsync();
            dto.Id = a.Id;
            return dto;
        }

        public async Task<bool> UpdateAuditAsync(OperationalAuditDto dto)
        {
            var a = await _auditRepository.GetByIdAsync(dto.Id);
            if (a == null) return false;

            a.TitleEn = dto.TitleEn;
            a.TitleAr = dto.TitleAr;
            a.AuditDate = dto.AuditDate;
            a.DepartmentId = dto.DepartmentId;
            a.AuditedProcessCount = dto.AuditedProcessCount;
            a.PassedProcessCount = dto.PassedProcessCount;
            a.CriticalFindingsCount = dto.CriticalFindingsCount;
            a.RecommendationsCount = dto.RecommendationsCount;
            a.RiskMitigationRate = dto.RiskMitigationRate;
            a.Status = dto.Status;

            await _auditRepository.UpdateAsync(a);
            await _auditRepository.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAuditAsync(int id)
        {
            var a = await _auditRepository.GetByIdAsync(id);
            if (a == null) return false;

            await _auditRepository.DeleteAsync(a);
            await _auditRepository.SaveChangesAsync();
            return true;
        }

        public async Task<OperationalAuditKpisDto> GetOperationalAuditKpisAsync()
        {
            var audits = await _auditRepository.GetAllAsync();
            var departments = await _departmentRepository.GetAllAsync();
            var isAr = IsArabic();

            var kpis = new OperationalAuditKpisDto();

            int totalAudits = audits.Count();
            if (totalAudits > 0)
            {
                int completedAudits = audits.Count(a => a.Status == OperationalAuditStatus.Completed);
                kpis.AuditExecutionRateActual = Math.Round((double)completedAudits / totalAudits * 100, 1);

                int totalProcesses = audits.Sum(a => a.AuditedProcessCount);
                int passedProcesses = audits.Sum(a => a.PassedProcessCount);

                kpis.TotalAuditedProcessesActual = totalProcesses;
                kpis.PassedProcessesCountActual = passedProcesses;

                kpis.OperationalComplianceRateActual = totalProcesses > 0
                    ? Math.Round((double)passedProcesses / totalProcesses * 100, 1)
                    : 100.0;

                kpis.CriticalFindingsCountActual = audits.Sum(a => a.CriticalFindingsCount);
                kpis.RecommendationsCountActual = audits.Sum(a => a.RecommendationsCount);
                kpis.RiskMitigationRateActual = Math.Round(audits.Average(a => a.RiskMitigationRate), 1);
            }
            else
            {
                kpis.AuditExecutionRateActual = 100.0;
                kpis.OperationalComplianceRateActual = 100.0;
                kpis.TotalAuditedProcessesActual = 0;
                kpis.PassedProcessesCountActual = 0;
                kpis.CriticalFindingsCountActual = 0;
                kpis.RecommendationsCountActual = 0;
                kpis.RiskMitigationRateActual = 100.0;
            }

            // Charts Data
            kpis.AuditsByStatus = new List<ChartDataPoint>
            {
                new ChartDataPoint { Label = isAr ? "مخطط" : "Planned", Value = audits.Count(a => a.Status == OperationalAuditStatus.Planned), Color = "#6b7280" },
                new ChartDataPoint { Label = isAr ? "جارٍ" : "In Progress", Value = audits.Count(a => a.Status == OperationalAuditStatus.InProgress), Color = "#3b82f6" },
                new ChartDataPoint { Label = isAr ? "مكتمل" : "Completed", Value = audits.Count(a => a.Status == OperationalAuditStatus.Completed), Color = "#10b981" },
                new ChartDataPoint { Label = isAr ? "متابعة" : "Follow-Up", Value = audits.Count(a => a.Status == OperationalAuditStatus.FollowUp), Color = "#f59e0b" }
            };

            kpis.ComplianceByDepartment = departments.Select(d =>
            {
                var deptAudits = audits.Where(a => a.DepartmentId == d.Id).ToList();
                int deptTotal = deptAudits.Sum(a => a.AuditedProcessCount);
                int deptPassed = deptAudits.Sum(a => a.PassedProcessCount);
                double passRate = deptTotal > 0 ? Math.Round((double)deptPassed / deptTotal * 100, 1) : 100.0;

                return new ChartDataPoint
                {
                    Label = isAr ? d.NameAr : d.NameEn,
                    Value = (decimal)passRate,
                    Color = passRate >= 95 ? "#10b981" : (passRate >= 80 ? "#f59e0b" : "#ef4444")
                };
            }).ToList();

            return kpis;
        }
    }
}
