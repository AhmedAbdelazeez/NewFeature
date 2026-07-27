using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NewFeature.Models;
using NewFeature.Services.Repositories;

namespace NewFeature.Services
{
    public class MohuStandardsService : IMohuStandardsService
    {
        private readonly ApplicationDbContext _context;

        public MohuStandardsService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<MohuKpiDto>> GetAllKpisAsync()
        {
            var kpis = await _context.MohuStandardKpis.ToListAsync();
            return kpis.Select(MapToDto).ToList();
        }

        public async Task<List<MohuKpiDto>> GetKpisByCategoryAsync(string categoryCode)
        {
            var kpis = await _context.MohuStandardKpis
                .Where(k => k.CategoryCode == categoryCode)
                .ToListAsync();
            return kpis.Select(MapToDto).ToList();
        }

        public async Task<MohuKpiDto?> GetKpiByCodeAsync(string kpiCode)
        {
            var kpi = await _context.MohuStandardKpis
                .FirstOrDefaultAsync(k => k.KpiCode == kpiCode);
            
            if (kpi == null) return null;
            return MapToDto(kpi);
        }

        public async Task<bool> UpdateKpiValuesAsync(string kpiCode, UpdateMohuKpiDto dto)
        {
            var kpi = await _context.MohuStandardKpis
                .FirstOrDefaultAsync(k => k.KpiCode == kpiCode);
                
            if (kpi == null) return false;

            kpi.ActualValue = dto.ActualValue;
            kpi.TargetValue = dto.TargetValue;
            kpi.LastUpdated = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<MohuKpiDto> CreateKpiAsync(MohuKpiDto dto)
        {
            var entity = new MohuStandardKpi
            {
                CategoryCode = dto.CategoryCode,
                KpiCode = dto.KpiCode,
                NameAr = dto.NameAr,
                NameEn = dto.NameEn,
                ActualValue = dto.ActualValue,
                TargetValue = dto.TargetValue,
                LastUpdated = DateTime.UtcNow
            };

            _context.MohuStandardKpis.Add(entity);
            await _context.SaveChangesAsync();
            return MapToDto(entity);
        }

        public async Task<bool> DeleteKpiAsync(string kpiCode)
        {
            var kpi = await _context.MohuStandardKpis.FirstOrDefaultAsync(k => k.KpiCode == kpiCode);
            if (kpi == null) return false;

            _context.MohuStandardKpis.Remove(kpi);
            await _context.SaveChangesAsync();
            return true;
        }

        #region Business Data Implementation
        
        // Pilgrim Groups
        public async Task<List<MohuPilgrimGroupDto>> GetAllPilgrimGroupsAsync()
        {
            return await _context.MohuPilgrimGroups.Select(g => new MohuPilgrimGroupDto
            {
                Id = g.Id, GroupNumber = g.GroupNumber, Nationality = g.Nationality,
                AgeGroup = g.AgeGroup, PackageCategory = g.PackageCategory, PackagePrice = g.PackagePrice,
                IsNewPilgrim = g.IsNewPilgrim, ArrivalDate = g.ArrivalDate,
                ArrivalPort = g.ArrivalPort, PilgrimCount = g.PilgrimCount
            }).ToListAsync();
        }

        public async Task<MohuPilgrimGroupDto> CreatePilgrimGroupAsync(MohuPilgrimGroupDto dto)
        {
            var entity = new MohuPilgrimGroup {
                GroupNumber = dto.GroupNumber, Nationality = dto.Nationality,
                AgeGroup = dto.AgeGroup, PackageCategory = dto.PackageCategory,
                PackagePrice = dto.PackagePrice, IsNewPilgrim = dto.IsNewPilgrim,
                ArrivalDate = dto.ArrivalDate, ArrivalPort = dto.ArrivalPort, PilgrimCount = dto.PilgrimCount
            };
            _context.MohuPilgrimGroups.Add(entity);
            await _context.SaveChangesAsync();
            dto.Id = entity.Id;
            return dto;
        }

        public async Task<bool> DeletePilgrimGroupAsync(int id)
        {
            var entity = await _context.MohuPilgrimGroups.FindAsync(id);
            if(entity == null) return false;
            _context.MohuPilgrimGroups.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }

        // Feedbacks
        public async Task<List<MohuFeedbackDto>> GetAllFeedbacksAsync()
        {
            return await _context.MohuFeedbacks.Select(f => new MohuFeedbackDto {
                Id = f.Id, MohuPilgrimGroupId = f.MohuPilgrimGroupId, Rating = f.Rating,
                ServiceType = f.ServiceType, HasComplaint = f.HasComplaint, 
                ComplaintDetails = f.ComplaintDetails, WaitingTimeMinutes = f.WaitingTimeMinutes,
                FeedbackDate = f.FeedbackDate
            }).ToListAsync();
        }

        public async Task<MohuFeedbackDto> CreateFeedbackAsync(MohuFeedbackDto dto)
        {
            var entity = new MohuFeedback {
                MohuPilgrimGroupId = dto.MohuPilgrimGroupId, Rating = dto.Rating,
                ServiceType = dto.ServiceType, HasComplaint = dto.HasComplaint,
                ComplaintDetails = dto.ComplaintDetails, WaitingTimeMinutes = dto.WaitingTimeMinutes,
                FeedbackDate = dto.FeedbackDate
            };
            _context.MohuFeedbacks.Add(entity);
            await _context.SaveChangesAsync();
            dto.Id = entity.Id;
            return dto;
        }

        public async Task<bool> DeleteFeedbackAsync(int id)
        {
            var entity = await _context.MohuFeedbacks.FindAsync(id);
            if(entity == null) return false;
            _context.MohuFeedbacks.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }

        // Violations
        public async Task<List<MohuViolationRecordDto>> GetAllViolationsAsync()
        {
            return await _context.MohuViolationRecords.Select(v => new MohuViolationRecordDto {
                Id = v.Id, ViolationType = v.ViolationType, PenaltyAmount = v.PenaltyAmount,
                IsClosed = v.IsClosed, AffectedPilgrimsCount = v.AffectedPilgrimsCount,
                CommitteeEvaluationScore = v.CommitteeEvaluationScore, InspectionDate = v.InspectionDate
            }).ToListAsync();
        }

        public async Task<MohuViolationRecordDto> CreateViolationAsync(MohuViolationRecordDto dto)
        {
            var entity = new MohuViolationRecord {
                ViolationType = dto.ViolationType, PenaltyAmount = dto.PenaltyAmount,
                IsClosed = dto.IsClosed, AffectedPilgrimsCount = dto.AffectedPilgrimsCount,
                CommitteeEvaluationScore = dto.CommitteeEvaluationScore, InspectionDate = dto.InspectionDate
            };
            _context.MohuViolationRecords.Add(entity);
            await _context.SaveChangesAsync();
            dto.Id = entity.Id;
            return dto;
        }

        public async Task<bool> DeleteViolationAsync(int id)
        {
            var entity = await _context.MohuViolationRecords.FindAsync(id);
            if(entity == null) return false;
            _context.MohuViolationRecords.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }

        // Permits
        public async Task<List<MohuPermitLogDto>> GetAllPermitsAsync()
        {
            return await _context.MohuPermitLogs.Select(p => new MohuPermitLogDto {
                Id = p.Id, PermitNumber = p.PermitNumber, IsEntryPortMatched = p.IsEntryPortMatched,
                IsEntryDateMatched = p.IsEntryDateMatched, IsHousingMatched = p.IsHousingMatched,
                VerificationDate = p.VerificationDate
            }).ToListAsync();
        }

        public async Task<MohuPermitLogDto> CreatePermitAsync(MohuPermitLogDto dto)
        {
            var entity = new MohuPermitLog {
                PermitNumber = dto.PermitNumber, IsEntryPortMatched = dto.IsEntryPortMatched,
                IsEntryDateMatched = dto.IsEntryDateMatched, IsHousingMatched = dto.IsHousingMatched,
                VerificationDate = dto.VerificationDate
            };
            _context.MohuPermitLogs.Add(entity);
            await _context.SaveChangesAsync();
            dto.Id = entity.Id;
            return dto;
        }

        public async Task<bool> DeletePermitAsync(int id)
        {
            var entity = await _context.MohuPermitLogs.FindAsync(id);
            if(entity == null) return false;
            _context.MohuPermitLogs.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdatePilgrimGroupAsync(int id, MohuPilgrimGroupDto dto)
        {
            var entity = await _context.MohuPilgrimGroups.FindAsync(id);
            if (entity == null) return false;
            
            entity.GroupNumber = dto.GroupNumber;
            entity.Nationality = dto.Nationality;
            entity.AgeGroup = dto.AgeGroup;
            entity.PackageCategory = dto.PackageCategory;
            entity.PackagePrice = dto.PackagePrice;
            entity.IsNewPilgrim = dto.IsNewPilgrim;
            entity.ArrivalDate = dto.ArrivalDate;
            entity.ArrivalPort = dto.ArrivalPort;
            entity.PilgrimCount = dto.PilgrimCount;
            
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateFeedbackAsync(int id, MohuFeedbackDto dto)
        {
            var entity = await _context.MohuFeedbacks.FindAsync(id);
            if (entity == null) return false;
            
            entity.MohuPilgrimGroupId = dto.MohuPilgrimGroupId;
            entity.Rating = dto.Rating;
            entity.ServiceType = dto.ServiceType;
            entity.HasComplaint = dto.HasComplaint;
            entity.ComplaintDetails = dto.ComplaintDetails;
            entity.WaitingTimeMinutes = dto.WaitingTimeMinutes;
            entity.FeedbackDate = dto.FeedbackDate;
            
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateViolationAsync(int id, MohuViolationRecordDto dto)
        {
            var entity = await _context.MohuViolationRecords.FindAsync(id);
            if (entity == null) return false;
            
            entity.ViolationType = dto.ViolationType;
            entity.PenaltyAmount = dto.PenaltyAmount;
            entity.IsClosed = dto.IsClosed;
            entity.AffectedPilgrimsCount = dto.AffectedPilgrimsCount;
            entity.CommitteeEvaluationScore = dto.CommitteeEvaluationScore;
            entity.InspectionDate = dto.InspectionDate;
            
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdatePermitAsync(int id, MohuPermitLogDto dto)
        {
            var entity = await _context.MohuPermitLogs.FindAsync(id);
            if (entity == null) return false;
            
            entity.PermitNumber = dto.PermitNumber;
            entity.IsEntryPortMatched = dto.IsEntryPortMatched;
            entity.IsEntryDateMatched = dto.IsEntryDateMatched;
            entity.IsHousingMatched = dto.IsHousingMatched;
            entity.VerificationDate = dto.VerificationDate;
            
            await _context.SaveChangesAsync();
            return true;
        }
        
        #endregion

        public async Task<bool> InitializeDefaultKpisAsync()
        {
            if (await _context.MohuStandardKpis.AnyAsync())
                return false;

            // This is just a backup fallback, actual seeding happens in DbInitializer
            return true;
        }

        private MohuKpiDto MapToDto(MohuStandardKpi entity)
        {
            return new MohuKpiDto
            {
                Id = entity.Id,
                CategoryCode = entity.CategoryCode,
                KpiCode = entity.KpiCode,
                NameAr = entity.NameAr,
                NameEn = entity.NameEn,
                ActualValue = entity.ActualValue,
                TargetValue = entity.TargetValue,
                LastUpdated = entity.LastUpdated
            };
        }
    }
}
