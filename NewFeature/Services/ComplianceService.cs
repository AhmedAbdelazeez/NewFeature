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
    public class ComplianceService : IComplianceService
    {
        private readonly IRepository<Department> _departmentRepository;
        private readonly IRepository<ViolationClassification> _classificationRepository;
        private readonly IRepository<Violation> _violationRepository;
        private readonly IRepository<InternalAudit> _auditRepository;
        private readonly IRepository<ImprovementAction> _improvementRepository;
        private readonly IRepository<ContractItem> _contractItemRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ComplianceService(
            IRepository<Department> departmentRepository,
            IRepository<ViolationClassification> classificationRepository,
            IRepository<Violation> violationRepository,
            IRepository<InternalAudit> auditRepository,
            IRepository<ImprovementAction> improvementRepository,
            IRepository<ContractItem> contractItemRepository,
            IHttpContextAccessor httpContextAccessor)
        {
            _departmentRepository = departmentRepository;
            _classificationRepository = classificationRepository;
            _violationRepository = violationRepository;
            _auditRepository = auditRepository;
            _improvementRepository = improvementRepository;
            _contractItemRepository = contractItemRepository;
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

        #region Department Lookup CRUD
        public async Task<IEnumerable<DepartmentDto>> GetAllDepartmentsAsync()
        {
            var departments = await _departmentRepository.GetAllAsync();
            var isAr = IsArabic();
            return departments.Select(d => new DepartmentDto
            {
                Id = d.Id,
                NameEn = d.NameEn,
                NameAr = d.NameAr,
                Code = d.Code,
                IsCompliant = d.IsCompliant,
                Name = isAr ? d.NameAr : d.NameEn
            }).ToList();
        }

        public async Task<DepartmentDto?> GetDepartmentByIdAsync(int id)
        {
            var d = await _departmentRepository.GetByIdAsync(id);
            if (d == null) return null;

            var isAr = IsArabic();
            return new DepartmentDto
            {
                Id = d.Id,
                NameEn = d.NameEn,
                NameAr = d.NameAr,
                Code = d.Code,
                IsCompliant = d.IsCompliant,
                Name = isAr ? d.NameAr : d.NameEn
            };
        }

        public async Task<DepartmentDto> CreateDepartmentAsync(DepartmentDto dto)
        {
            var d = new Department
            {
                NameEn = dto.NameEn,
                NameAr = dto.NameAr,
                Code = dto.Code,
                IsCompliant = dto.IsCompliant
            };

            await _departmentRepository.AddAsync(d);
            await _departmentRepository.SaveChangesAsync();
            dto.Id = d.Id;
            return dto;
        }

        public async Task<bool> UpdateDepartmentAsync(DepartmentDto dto)
        {
            var d = await _departmentRepository.GetByIdAsync(dto.Id);
            if (d == null) return false;

            d.NameEn = dto.NameEn;
            d.NameAr = dto.NameAr;
            d.Code = dto.Code;
            d.IsCompliant = dto.IsCompliant;

            await _departmentRepository.UpdateAsync(d);
            await _departmentRepository.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteDepartmentAsync(int id)
        {
            var d = await _departmentRepository.GetByIdAsync(id);
            if (d == null) return false;

            await _departmentRepository.DeleteAsync(d);
            await _departmentRepository.SaveChangesAsync();
            return true;
        }
        #endregion

        #region Violation Classification Lookup CRUD
        public async Task<IEnumerable<ViolationClassificationDto>> GetAllClassificationsAsync()
        {
            var classifications = await _classificationRepository.GetAllAsync();
            var isAr = IsArabic();
            return classifications.Select(c => new ViolationClassificationDto
            {
                Id = c.Id,
                NameEn = c.NameEn,
                NameAr = c.NameAr,
                Code = c.Code,
                Name = isAr ? c.NameAr : c.NameEn
            }).ToList();
        }

        public async Task<ViolationClassificationDto?> GetClassificationByIdAsync(int id)
        {
            var c = await _classificationRepository.GetByIdAsync(id);
            if (c == null) return null;

            var isAr = IsArabic();
            return new ViolationClassificationDto
            {
                Id = c.Id,
                NameEn = c.NameEn,
                NameAr = c.NameAr,
                Code = c.Code,
                Name = isAr ? c.NameAr : c.NameEn
            };
        }

        public async Task<ViolationClassificationDto> CreateClassificationAsync(ViolationClassificationDto dto)
        {
            var c = new ViolationClassification
            {
                NameEn = dto.NameEn,
                NameAr = dto.NameAr,
                Code = dto.Code
            };

            await _classificationRepository.AddAsync(c);
            await _classificationRepository.SaveChangesAsync();
            dto.Id = c.Id;
            return dto;
        }

        public async Task<bool> UpdateClassificationAsync(ViolationClassificationDto dto)
        {
            var c = await _classificationRepository.GetByIdAsync(dto.Id);
            if (c == null) return false;

            c.NameEn = dto.NameEn;
            c.NameAr = dto.NameAr;
            c.Code = dto.Code;

            await _classificationRepository.UpdateAsync(c);
            await _classificationRepository.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteClassificationAsync(int id)
        {
            var c = await _classificationRepository.GetByIdAsync(id);
            if (c == null) return false;

            await _classificationRepository.DeleteAsync(c);
            await _classificationRepository.SaveChangesAsync();
            return true;
        }
        #endregion

        #region Violation CRUD
        public async Task<IEnumerable<ViolationDto>> GetAllViolationsAsync()
        {
            var violations = await _violationRepository.GetAllAsync();
            var departments = await _departmentRepository.GetAllAsync();
            var classifications = await _classificationRepository.GetAllAsync();

            var isAr = IsArabic();
            var deptMap = departments.ToDictionary(d => d.Id, d => isAr ? d.NameAr : d.NameEn);
            var classMap = classifications.ToDictionary(c => c.Id, c => isAr ? c.NameAr : c.NameEn);

            return violations.Select(v => new ViolationDto
            {
                Id = v.Id,
                TitleEn = v.TitleEn,
                TitleAr = v.TitleAr,
                DescriptionEn = v.DescriptionEn,
                DescriptionAr = v.DescriptionAr,
                DetectionDate = v.DetectionDate,
                ResolutionDate = v.ResolutionDate,
                Status = v.Status,
                Severity = v.Severity,
                ClassificationId = v.ClassificationId,
                ClassificationName = classMap.TryGetValue(v.ClassificationId, out var className) ? className : "Unknown",
                DepartmentId = v.DepartmentId,
                DepartmentName = deptMap.TryGetValue(v.DepartmentId, out var deptName) ? deptName : "Unknown",
                FineAmount = v.FineAmount,
                Title = isAr ? v.TitleAr : v.TitleEn,
                Description = isAr ? v.DescriptionAr : v.DescriptionEn
            }).ToList();
        }

        public async Task<ViolationDto?> GetViolationByIdAsync(int id)
        {
            var v = await _violationRepository.GetByIdAsync(id);
            if (v == null) return null;

            var dept = await _departmentRepository.GetByIdAsync(v.DepartmentId);
            var cls = await _classificationRepository.GetByIdAsync(v.ClassificationId);
            var isAr = IsArabic();

            return new ViolationDto
            {
                Id = v.Id,
                TitleEn = v.TitleEn,
                TitleAr = v.TitleAr,
                DescriptionEn = v.DescriptionEn,
                DescriptionAr = v.DescriptionAr,
                DetectionDate = v.DetectionDate,
                ResolutionDate = v.ResolutionDate,
                Status = v.Status,
                Severity = v.Severity,
                ClassificationId = v.ClassificationId,
                ClassificationName = cls != null ? (isAr ? cls.NameAr : cls.NameEn) : "Unknown",
                DepartmentId = v.DepartmentId,
                DepartmentName = dept != null ? (isAr ? dept.NameAr : dept.NameEn) : "Unknown",
                FineAmount = v.FineAmount,
                Title = isAr ? v.TitleAr : v.TitleEn,
                Description = isAr ? v.DescriptionAr : v.DescriptionEn
            };
        }

        public async Task<ViolationDto> CreateViolationAsync(ViolationDto dto)
        {
            var v = new Violation
            {
                TitleEn = dto.TitleEn,
                TitleAr = dto.TitleAr,
                DescriptionEn = dto.DescriptionEn,
                DescriptionAr = dto.DescriptionAr,
                DetectionDate = dto.DetectionDate,
                ResolutionDate = dto.ResolutionDate,
                Status = dto.Status,
                Severity = dto.Severity,
                ClassificationId = dto.ClassificationId,
                DepartmentId = dto.DepartmentId,
                FineAmount = dto.FineAmount
            };

            await _violationRepository.AddAsync(v);
            await _violationRepository.SaveChangesAsync();
            dto.Id = v.Id;
            return dto;
        }

        public async Task<bool> UpdateViolationAsync(ViolationDto dto)
        {
            var v = await _violationRepository.GetByIdAsync(dto.Id);
            if (v == null) return false;

            v.TitleEn = dto.TitleEn;
            v.TitleAr = dto.TitleAr;
            v.DescriptionEn = dto.DescriptionEn;
            v.DescriptionAr = dto.DescriptionAr;
            v.DetectionDate = dto.DetectionDate;
            v.ResolutionDate = dto.ResolutionDate;
            v.Status = dto.Status;
            v.Severity = dto.Severity;
            v.ClassificationId = dto.ClassificationId;
            v.DepartmentId = dto.DepartmentId;
            v.FineAmount = dto.FineAmount;

            await _violationRepository.UpdateAsync(v);
            await _violationRepository.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteViolationAsync(int id)
        {
            var v = await _violationRepository.GetByIdAsync(id);
            if (v == null) return false;

            await _violationRepository.DeleteAsync(v);
            await _violationRepository.SaveChangesAsync();
            return true;
        }
        #endregion

        #region Internal Audit CRUD
        public async Task<IEnumerable<InternalAuditDto>> GetAllAuditsAsync()
        {
            var audits = await _auditRepository.GetAllAsync();
            var departments = await _departmentRepository.GetAllAsync();

            var isAr = IsArabic();
            var deptMap = departments.ToDictionary(d => d.Id, d => isAr ? d.NameAr : d.NameEn);

            return audits.Select(a => new InternalAuditDto
            {
                Id = a.Id,
                TitleEn = a.TitleEn,
                TitleAr = a.TitleAr,
                AuditDate = a.AuditDate,
                DepartmentId = a.DepartmentId,
                DepartmentName = deptMap.TryGetValue(a.DepartmentId, out var deptName) ? deptName : "Unknown",
                TotalControlsAudited = a.TotalControlsAudited,
                PassedControlsCount = a.PassedControlsCount,
                CriticalFindingsCount = a.CriticalFindingsCount,
                Title = isAr ? a.TitleAr : a.TitleEn
            }).ToList();
        }

        public async Task<InternalAuditDto?> GetAuditByIdAsync(int id)
        {
            var a = await _auditRepository.GetByIdAsync(id);
            if (a == null) return null;

            var dept = await _departmentRepository.GetByIdAsync(a.DepartmentId);
            var isAr = IsArabic();

            return new InternalAuditDto
            {
                Id = a.Id,
                TitleEn = a.TitleEn,
                TitleAr = a.TitleAr,
                AuditDate = a.AuditDate,
                DepartmentId = a.DepartmentId,
                DepartmentName = dept != null ? (isAr ? dept.NameAr : dept.NameEn) : "Unknown",
                TotalControlsAudited = a.TotalControlsAudited,
                PassedControlsCount = a.PassedControlsCount,
                CriticalFindingsCount = a.CriticalFindingsCount,
                Title = isAr ? a.TitleAr : a.TitleEn
            };
        }

        public async Task<InternalAuditDto> CreateAuditAsync(InternalAuditDto dto)
        {
            var a = new InternalAudit
            {
                TitleEn = dto.TitleEn,
                TitleAr = dto.TitleAr,
                AuditDate = dto.AuditDate,
                DepartmentId = dto.DepartmentId,
                TotalControlsAudited = dto.TotalControlsAudited,
                PassedControlsCount = dto.PassedControlsCount,
                CriticalFindingsCount = dto.CriticalFindingsCount
            };

            await _auditRepository.AddAsync(a);
            await _auditRepository.SaveChangesAsync();
            dto.Id = a.Id;
            return dto;
        }

        public async Task<bool> UpdateAuditAsync(InternalAuditDto dto)
        {
            var a = await _auditRepository.GetByIdAsync(dto.Id);
            if (a == null) return false;

            a.TitleEn = dto.TitleEn;
            a.TitleAr = dto.TitleAr;
            a.AuditDate = dto.AuditDate;
            a.DepartmentId = dto.DepartmentId;
            a.TotalControlsAudited = dto.TotalControlsAudited;
            a.PassedControlsCount = dto.PassedControlsCount;
            a.CriticalFindingsCount = dto.CriticalFindingsCount;

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
        #endregion

        #region Improvement Action CRUD
        public async Task<IEnumerable<ImprovementActionDto>> GetAllImprovementsAsync()
        {
            var improvements = await _improvementRepository.GetAllAsync();
            var departments = await _departmentRepository.GetAllAsync();

            var isAr = IsArabic();
            var deptMap = departments.ToDictionary(d => d.Id, d => isAr ? d.NameAr : d.NameEn);

            return improvements.Select(i => new ImprovementActionDto
            {
                Id = i.Id,
                TitleEn = i.TitleEn,
                TitleAr = i.TitleAr,
                DescriptionEn = i.DescriptionEn,
                DescriptionAr = i.DescriptionAr,
                DepartmentId = i.DepartmentId,
                DepartmentName = deptMap.TryGetValue(i.DepartmentId, out var deptName) ? deptName : "Unknown",
                Status = i.Status,
                DateProposed = i.DateProposed,
                DateImplemented = i.DateImplemented,
                Title = isAr ? i.TitleAr : i.TitleEn,
                Description = isAr ? i.DescriptionAr : i.DescriptionEn
            }).ToList();
        }

        public async Task<ImprovementActionDto?> GetImprovementByIdAsync(int id)
        {
            var i = await _improvementRepository.GetByIdAsync(id);
            if (i == null) return null;

            var dept = await _departmentRepository.GetByIdAsync(i.DepartmentId);
            var isAr = IsArabic();

            return new ImprovementActionDto
            {
                Id = i.Id,
                TitleEn = i.TitleEn,
                TitleAr = i.TitleAr,
                DescriptionEn = i.DescriptionEn,
                DescriptionAr = i.DescriptionAr,
                DepartmentId = i.DepartmentId,
                DepartmentName = dept != null ? (isAr ? dept.NameAr : dept.NameEn) : "Unknown",
                Status = i.Status,
                DateProposed = i.DateProposed,
                DateImplemented = i.DateImplemented,
                Title = isAr ? i.TitleAr : i.TitleEn,
                Description = isAr ? i.DescriptionAr : i.DescriptionEn
            };
        }

        public async Task<ImprovementActionDto> CreateImprovementAsync(ImprovementActionDto dto)
        {
            var i = new ImprovementAction
            {
                TitleEn = dto.TitleEn,
                TitleAr = dto.TitleAr,
                DescriptionEn = dto.DescriptionEn,
                DescriptionAr = dto.DescriptionAr,
                DepartmentId = dto.DepartmentId,
                Status = dto.Status,
                DateProposed = dto.DateProposed,
                DateImplemented = dto.DateImplemented
            };

            await _improvementRepository.AddAsync(i);
            await _improvementRepository.SaveChangesAsync();
            dto.Id = i.Id;
            return dto;
        }

        public async Task<bool> UpdateImprovementAsync(ImprovementActionDto dto)
        {
            var i = await _improvementRepository.GetByIdAsync(dto.Id);
            if (i == null) return false;

            i.TitleEn = dto.TitleEn;
            i.TitleAr = dto.TitleAr;
            i.DescriptionEn = dto.DescriptionEn;
            i.DescriptionAr = dto.DescriptionAr;
            i.DepartmentId = dto.DepartmentId;
            i.Status = dto.Status;
            i.DateProposed = dto.DateProposed;
            i.DateImplemented = dto.DateImplemented;

            await _improvementRepository.UpdateAsync(i);
            await _improvementRepository.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteImprovementAsync(int id)
        {
            var i = await _improvementRepository.GetByIdAsync(id);
            if (i == null) return false;

            await _improvementRepository.DeleteAsync(i);
            await _improvementRepository.SaveChangesAsync();
            return true;
        }
        #endregion

        #region KPIs Dashboard Calculation
        public async Task<ComplianceKpisDto> GetComplianceKpisAsync()
        {
            var violations = await _violationRepository.GetAllAsync();
            var audits = await _auditRepository.GetAllAsync();
            var improvements = await _improvementRepository.GetAllAsync();
            var departments = await _departmentRepository.GetAllAsync();
            var contractItems = await _contractItemRepository.GetAllAsync();

            var kpis = new ComplianceKpisDto();
            var isAr = IsArabic();

            // 1. Registered Violations Count
            kpis.RegisteredViolationsCountActual = violations.Count();

            // 2. Violations Closure Rate
            int closedViolations = violations.Count(v => v.Status == ViolationStatus.Closed);
            kpis.ViolationsClosureRateActual = kpis.RegisteredViolationsCountActual > 0
                ? Math.Round((double)closedViolations / kpis.RegisteredViolationsCountActual * 100, 1)
                : 100.0;

            // 3. Average Violation Resolution Time (for closed violations)
            var closedList = violations.Where(v => v.Status == ViolationStatus.Closed && v.ResolutionDate.HasValue).ToList();
            if (closedList.Any())
            {
                double totalDays = closedList.Sum(v => (v.ResolutionDate!.Value.Date - v.DetectionDate.Date).TotalDays);
                kpis.AverageViolationResolutionTimeActual = Math.Round(totalDays / closedList.Count, 1);
            }
            else
            {
                kpis.AverageViolationResolutionTimeActual = 0;
            }

            // 4. Contractual Compliance Rate
            int totalContracts = contractItems.Count();
            int compliantContracts = contractItems.Count(ci => ci.IsCompliant);
            kpis.ContractualComplianceRateActual = totalContracts > 0
                ? Math.Round((double)compliantContracts / totalContracts * 100, 1)
                : 100.0;

            // 5. Policy & Procedure Adherence Rate
            int totalDepts = departments.Count();
            int compliantDepts = departments.Count(d => d.IsCompliant);
            kpis.PolicyAdherenceRateActual = totalDepts > 0
                ? Math.Round((double)compliantDepts / totalDepts * 100, 1)
                : 100.0;

            // 6. Internal Audit Passing Rate
            int totalControls = audits.Sum(a => a.TotalControlsAudited);
            int passedControls = audits.Sum(a => a.PassedControlsCount);
            kpis.InternalAuditPassingRateActual = totalControls > 0
                ? Math.Round((double)passedControls / totalControls * 100, 1)
                : 100.0;

            // 7. Critical Audit Findings
            kpis.CriticalAuditFindingsActual = audits.Sum(a => a.CriticalFindingsCount);

            // 8. Monthly Overall Compliance Index
            // Average of Contractual, Policy Adherence, and Audit Passing Rates.
            kpis.MonthlyOverallComplianceIndexActual = Math.Round(
                (kpis.ContractualComplianceRateActual + kpis.PolicyAdherenceRateActual + kpis.InternalAuditPassingRateActual) / 3.0, 1);

            // 9. Continuous Improvement Rate
            int totalImprovements = improvements.Count();
            int implementedImprovements = improvements.Count(i => i.Status == ImprovementStatus.Implemented);
            kpis.ContinuousImprovementRateActual = totalImprovements > 0
                ? Math.Round((double)implementedImprovements / totalImprovements * 100, 1)
                : 100.0;

            // --- Charts Data ---

            // Violations by Severity
            kpis.ViolationsBySeverity = new List<ChartDataPoint>
            {
                new ChartDataPoint { Label = isAr ? "حرجة" : "Critical", Value = violations.Count(v => v.Severity == ViolationSeverity.Critical), Color = "#ef4444" },
                new ChartDataPoint { Label = isAr ? "جسيمة" : "Major", Value = violations.Count(v => v.Severity == ViolationSeverity.Major), Color = "#f59e0b" },
                new ChartDataPoint { Label = isAr ? "بسيطة" : "Minor", Value = violations.Count(v => v.Severity == ViolationSeverity.Minor), Color = "#3b82f6" }
            };

            // Violations by Status
            kpis.ViolationsByStatus = new List<ChartDataPoint>
            {
                new ChartDataPoint { Label = isAr ? "مفتوحة" : "Open", Value = violations.Count(v => v.Status == ViolationStatus.Open), Color = "#ef4444" },
                new ChartDataPoint { Label = isAr ? "مغلقة" : "Closed", Value = closedViolations, Color = "#10b981" }
            };

            // Improvements by Status
            kpis.ImprovementsByStatus = new List<ChartDataPoint>
            {
                new ChartDataPoint { Label = isAr ? "مقترح" : "Proposed", Value = improvements.Count(i => i.Status == ImprovementStatus.Proposed), Color = "#6b7280" },
                new ChartDataPoint { Label = isAr ? "معتمد" : "Approved", Value = improvements.Count(i => i.Status == ImprovementStatus.Approved), Color = "#3b82f6" },
                new ChartDataPoint { Label = isAr ? "منفذ" : "Implemented", Value = implementedImprovements, Color = "#10b981" },
                new ChartDataPoint { Label = isAr ? "ملغى" : "Cancelled", Value = improvements.Count(i => i.Status == ImprovementStatus.Cancelled), Color = "#ef4444" }
            };

            // Audits Passing by Department
            kpis.AuditsPassingByDepartment = departments.Select(d =>
            {
                var deptAudits = audits.Where(a => a.DepartmentId == d.Id).ToList();
                int deptTotal = deptAudits.Sum(a => a.TotalControlsAudited);
                int deptPassed = deptAudits.Sum(a => a.PassedControlsCount);
                double passRate = deptTotal > 0 ? Math.Round((double)deptPassed / deptTotal * 100, 1) : 100.0;

                return new ChartDataPoint
                {
                    Label = isAr ? d.NameAr : d.NameEn,
                    Value = (decimal)passRate,
                    Color = passRate >= 90 ? "#10b981" : (passRate >= 70 ? "#f59e0b" : "#ef4444")
                };
            }).ToList();

            return kpis;
        }
        #endregion
    }
}
