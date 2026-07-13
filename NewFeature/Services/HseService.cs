using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NewFeature.Models;
using NewFeature.Services.Repositories;

namespace NewFeature.Services
{
    public class HseService : IHseService
    {
        private readonly ApplicationDbContext _context;

        public HseService(ApplicationDbContext context)
        {
            _context = context;
        }

        #region Incidents CRUD
        public async Task<IEnumerable<HseIncidentDto>> GetAllIncidentsAsync()
        {
            var incidents = await _context.HseIncidents.OrderByDescending(i => i.Date).ToListAsync();
            return incidents.Select(MapToIncidentDto);
        }

        public async Task<HseIncidentDto?> GetIncidentByIdAsync(int id)
        {
            var incident = await _context.HseIncidents.FindAsync(id);
            if (incident == null) return null;
            return MapToIncidentDto(incident);
        }

        public async Task<HseIncidentDto> CreateIncidentAsync(HseIncidentDto dto)
        {
            var incident = new HseIncident
            {
                TitleEn = dto.TitleEn,
                TitleAr = dto.TitleAr,
                Type = dto.Type,
                Date = dto.Date == default ? DateTime.UtcNow : dto.Date,
                DescriptionEn = dto.DescriptionEn,
                DescriptionAr = dto.DescriptionAr,
                Severity = dto.Severity,
                Location = dto.Location
            };

            _context.HseIncidents.Add(incident);
            await _context.SaveChangesAsync();
            return MapToIncidentDto(incident);
        }

        public async Task<bool> UpdateIncidentAsync(HseIncidentDto dto)
        {
            var incident = await _context.HseIncidents.FindAsync(dto.Id);
            if (incident == null) return false;

            incident.TitleEn = dto.TitleEn;
            incident.TitleAr = dto.TitleAr;
            incident.Type = dto.Type;
            incident.Date = dto.Date;
            incident.DescriptionEn = dto.DescriptionEn;
            incident.DescriptionAr = dto.DescriptionAr;
            incident.Severity = dto.Severity;
            incident.Location = dto.Location;

            _context.Entry(incident).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteIncidentAsync(int id)
        {
            var incident = await _context.HseIncidents.FindAsync(id);
            if (incident == null) return false;

            _context.HseIncidents.Remove(incident);
            await _context.SaveChangesAsync();
            return true;
        }
        #endregion

        #region Inspections CRUD
        public async Task<IEnumerable<HseInspectionDto>> GetAllInspectionsAsync()
        {
            var inspections = await _context.HseInspections.OrderByDescending(i => i.InspectionDate).ToListAsync();
            return inspections.Select(MapToInspectionDto);
        }

        public async Task<HseInspectionDto?> GetInspectionByIdAsync(int id)
        {
            var inspection = await _context.HseInspections.FindAsync(id);
            if (inspection == null) return null;
            return MapToInspectionDto(inspection);
        }

        public async Task<HseInspectionDto> CreateInspectionAsync(HseInspectionDto dto)
        {
            var inspection = new HseInspection
            {
                TitleEn = dto.TitleEn,
                TitleAr = dto.TitleAr,
                InspectionDate = dto.InspectionDate == default ? DateTime.UtcNow : dto.InspectionDate,
                InspectorName = dto.InspectorName,
                Status = dto.Status,
                ComplianceScore = dto.ComplianceScore,
                TrainingHours = dto.TrainingHours
            };

            _context.HseInspections.Add(inspection);
            await _context.SaveChangesAsync();
            return MapToInspectionDto(inspection);
        }

        public async Task<bool> UpdateInspectionAsync(HseInspectionDto dto)
        {
            var inspection = await _context.HseInspections.FindAsync(dto.Id);
            if (inspection == null) return false;

            inspection.TitleEn = dto.TitleEn;
            inspection.TitleAr = dto.TitleAr;
            inspection.InspectionDate = dto.InspectionDate;
            inspection.InspectorName = dto.InspectorName;
            inspection.Status = dto.Status;
            inspection.ComplianceScore = dto.ComplianceScore;
            inspection.TrainingHours = dto.TrainingHours;

            _context.Entry(inspection).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteInspectionAsync(int id)
        {
            var inspection = await _context.HseInspections.FindAsync(id);
            if (inspection == null) return false;

            _context.HseInspections.Remove(inspection);
            await _context.SaveChangesAsync();
            return true;
        }
        #endregion

        #region KPIs Calculation
        public async Task<HseKpisDto> GetHseKpisAsync()
        {
            var incidents = await _context.HseIncidents.ToListAsync();
            var inspections = await _context.HseInspections.ToListAsync();

            // 1. LTIFR (Injury Incident Count)
            int injuriesCount = incidents.Count(i => i.Type == "Injury");

            // 2. Serious Road Accidents
            int seriousAccidents = incidents.Count(i => i.Type == "SeriousAccident");

            // 3. Regulatory Compliance Rate
            double complianceRate = 95.0;
            var completedInspections = inspections.Where(ins => ins.Status == "Completed").ToList();
            if (completedInspections.Any())
            {
                complianceRate = Math.Round(completedInspections.Average(ins => ins.ComplianceScore), 1);
            }

            // 4. HSE Training Hours
            double trainingHours = inspections.Sum(ins => ins.TrainingHours);

            // 5. Inspections Completion Rate
            double inspectionCompletion = 95.0;
            if (inspections.Any())
            {
                inspectionCompletion = Math.Round((double)inspections.Count(ins => ins.Status == "Completed") / inspections.Count * 100, 1);
            }

            // 6. Near-Miss Reporting
            int nearMissCount = incidents.Count(i => i.Type == "NearMiss");

            // 7. Waste Recycling Rate: clamps between 20% and 98%
            double recyclingRate = 42.5;
            if (inspections.Any())
            {
                recyclingRate = Math.Max(20.0, Math.Min(98.0, 40.0 + (inspections.Count * 2.5) - (incidents.Count * 1.5)));
                recyclingRate = Math.Round(recyclingRate, 1);
            }

            return new HseKpisDto
            {
                LtifrActual = injuriesCount,
                LtifrTarget = 0,

                SeriousRoadAccidentsActual = seriousAccidents,
                SeriousRoadAccidentsTarget = 0,

                RegulatoryComplianceRateActual = complianceRate,
                RegulatoryComplianceRateTarget = 95.0,

                HseTrainingHoursActual = trainingHours,
                HseTrainingHoursTarget = 120.0,

                SafetyInspectionsCompletionActual = inspectionCompletion,
                SafetyInspectionsCompletionTarget = 95.0,

                NearMissReportingActual = nearMissCount,
                NearMissReportingTarget = 50,

                WasteRecyclingRateActual = recyclingRate,
                WasteRecyclingRateTarget = 40.0
            };
        }
        #endregion

        #region Mappers
        private static HseIncidentDto MapToIncidentDto(HseIncident i)
        {
            return new HseIncidentDto
            {
                Id = i.Id,
                TitleEn = i.TitleEn,
                TitleAr = i.TitleAr,
                Type = i.Type,
                Date = i.Date,
                DescriptionEn = i.DescriptionEn,
                DescriptionAr = i.DescriptionAr,
                Severity = i.Severity,
                Location = i.Location
            };
        }

        private static HseInspectionDto MapToInspectionDto(HseInspection ins)
        {
            return new HseInspectionDto
            {
                Id = ins.Id,
                TitleEn = ins.TitleEn,
                TitleAr = ins.TitleAr,
                InspectionDate = ins.InspectionDate,
                InspectorName = ins.InspectorName,
                Status = ins.Status,
                ComplianceScore = ins.ComplianceScore,
                TrainingHours = ins.TrainingHours
            };
        }
        #endregion
    }
}
