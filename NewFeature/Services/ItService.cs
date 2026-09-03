using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NewFeature.Models;
using NewFeature.Services.Repositories;

namespace NewFeature.Services
{
    public class ItService : IItService
    {
        private readonly ApplicationDbContext _context;

        public ItService(ApplicationDbContext context)
        {
            _context = context;
        }

        #region Tickets CRUD
        public async Task<IEnumerable<ItTicketDto>> GetAllTicketsAsync()
        {
            var tickets = await _context.ItTickets.OrderByDescending(t => t.CreatedAt).ToListAsync();
            return tickets.Select(MapToTicketDto);
        }

        public async Task<ItTicketDto?> GetTicketByIdAsync(int id)
        {
            var ticket = await _context.ItTickets.FindAsync(id);
            if (ticket == null) return null;
            return MapToTicketDto(ticket);
        }

        public async Task<ItTicketDto> CreateTicketAsync(ItTicketDto dto)
        {
            var ticket = new ItTicket
            {
                TitleEn = dto.TitleEn,
                TitleAr = dto.TitleAr,
                DescriptionEn = dto.DescriptionEn,
                DescriptionAr = dto.DescriptionAr,
                Status = dto.Status,
                Priority = dto.Priority,
                CreatedAt = DateTime.UtcNow,
                ResolvedAt = dto.Status == "Resolved" ? DateTime.UtcNow : null,
                UserSatisfaction = dto.Status == "Resolved" ? dto.UserSatisfaction : null
            };

            _context.ItTickets.Add(ticket);
            await _context.SaveChangesAsync();
            return MapToTicketDto(ticket);
        }

        public async Task<bool> UpdateTicketAsync(ItTicketDto dto)
        {
            var ticket = await _context.ItTickets.FindAsync(dto.Id);
            if (ticket == null) return false;

            ticket.TitleEn = dto.TitleEn;
            ticket.TitleAr = dto.TitleAr;
            ticket.DescriptionEn = dto.DescriptionEn;
            ticket.DescriptionAr = dto.DescriptionAr;
            
            if (ticket.Status != "Resolved" && dto.Status == "Resolved")
            {
                ticket.ResolvedAt = DateTime.UtcNow;
                ticket.UserSatisfaction = dto.UserSatisfaction;
            }
            else if (dto.Status != "Resolved")
            {
                ticket.ResolvedAt = null;
                ticket.UserSatisfaction = null;
            }
            else
            {
                ticket.UserSatisfaction = dto.UserSatisfaction;
            }

            ticket.Status = dto.Status;
            ticket.Priority = dto.Priority;

            _context.Entry(ticket).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteTicketAsync(int id)
        {
            var ticket = await _context.ItTickets.FindAsync(id);
            if (ticket == null) return false;

            _context.ItTickets.Remove(ticket);
            await _context.SaveChangesAsync();
            return true;
        }

        // Paginated + searchable listing used by the IT page's Tickets table.
        public async Task<PagedResultDto<ItTicketDto>> GetTicketsPagedAsync(int page, int pageSize, string? search, string? status)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 20;
            if (pageSize > 200) pageSize = 200;

            var tickets = (await _context.ItTickets.ToListAsync()).AsEnumerable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                tickets = tickets.Where(t =>
                    Contains(t.TitleEn, term) ||
                    Contains(t.TitleAr, term) ||
                    Contains(t.DescriptionEn, term) ||
                    Contains(t.DescriptionAr, term));
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                tickets = tickets.Where(t => string.Equals(t.Status, status, StringComparison.OrdinalIgnoreCase));
            }

            var ordered = tickets.OrderByDescending(t => t.CreatedAt).ToList();
            var totalCount = ordered.Count;

            var items = ordered
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(MapToTicketDto)
                .ToList();

            return new PagedResultDto<ItTicketDto>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }
        #endregion

        #region Systems CRUD
        public async Task<IEnumerable<ItSystemDto>> GetAllSystemsAsync()
        {
            var systems = await _context.ItSystems.OrderBy(s => s.NameEn).ToListAsync();
            return systems.Select(MapToSystemDto);
        }

        public async Task<ItSystemDto?> GetSystemByIdAsync(int id)
        {
            var system = await _context.ItSystems.FindAsync(id);
            if (system == null) return null;
            return MapToSystemDto(system);
        }

        public async Task<ItSystemDto> CreateSystemAsync(ItSystemDto dto)
        {
            var system = new ItSystem
            {
                NameEn = dto.NameEn,
                NameAr = dto.NameAr,
                UptimePercentage = dto.UptimePercentage,
                LastBackupStatus = dto.LastBackupStatus,
                Status = dto.Status,
                IsAutomated = dto.IsAutomated
            };

            _context.ItSystems.Add(system);
            await _context.SaveChangesAsync();
            return MapToSystemDto(system);
        }

        public async Task<bool> UpdateSystemAsync(ItSystemDto dto)
        {
            var system = await _context.ItSystems.FindAsync(dto.Id);
            if (system == null) return false;

            system.NameEn = dto.NameEn;
            system.NameAr = dto.NameAr;
            system.UptimePercentage = dto.UptimePercentage;
            system.LastBackupStatus = dto.LastBackupStatus;
            system.Status = dto.Status;
            system.IsAutomated = dto.IsAutomated;

            _context.Entry(system).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteSystemAsync(int id)
        {
            var system = await _context.ItSystems.FindAsync(id);
            if (system == null) return false;

            _context.ItSystems.Remove(system);
            await _context.SaveChangesAsync();
            return true;
        }

        // Paginated + searchable listing used by the IT page's Systems table.
        public async Task<PagedResultDto<ItSystemDto>> GetSystemsPagedAsync(int page, int pageSize, string? search)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 20;
            if (pageSize > 200) pageSize = 200;

            var systems = (await _context.ItSystems.ToListAsync()).AsEnumerable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                systems = systems.Where(s =>
                    Contains(s.NameEn, term) ||
                    Contains(s.NameAr, term));
            }

            var ordered = systems.OrderBy(s => s.NameEn).ToList();
            var totalCount = ordered.Count;

            var items = ordered
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(MapToSystemDto)
                .ToList();

            return new PagedResultDto<ItSystemDto>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }
        #endregion

        #region KPIs Calculation
        public async Task<ItKpisDto> GetItKpisAsync()
        {
            var tickets = await _context.ItTickets.ToListAsync();
            var systems = await _context.ItSystems.ToListAsync();
            var projects = await _context.Projects.ToListAsync();

            // 1. Digital Transformation Rate
            double automRate = 0;
            if (systems.Any())
            {
                automRate = Math.Round((double)systems.Count(s => s.IsAutomated) / systems.Count * 100, 1);
            }

            // 2. System Uptime
            double avgUptime = 99.9;
            if (systems.Any())
            {
                avgUptime = Math.Round(systems.Average(s => s.UptimePercentage), 2);
            }

            // 3. Avg Resolution Time
            double avgResTime = 2.0;
            var resolvedTickets = tickets.Where(t => t.Status == "Resolved" && t.ResolvedAt.HasValue).ToList();
            if (resolvedTickets.Any())
            {
                avgResTime = Math.Round(resolvedTickets.Average(t => (t.ResolvedAt.Value - t.CreatedAt).TotalHours), 1);
            }

            // 4. Cybersecurity Incidents
            int securityIncidents = tickets.Count(t => 
                t.Priority == "Critical" && 
                (t.TitleEn.Contains("hack", StringComparison.OrdinalIgnoreCase) || 
                 t.TitleEn.Contains("breach", StringComparison.OrdinalIgnoreCase) || 
                 t.TitleEn.Contains("ransomware", StringComparison.OrdinalIgnoreCase) || 
                 t.TitleEn.Contains("leak", StringComparison.OrdinalIgnoreCase) || 
                 t.TitleEn.Contains("phishing", StringComparison.OrdinalIgnoreCase) ||
                 t.TitleAr.Contains("امن", StringComparison.OrdinalIgnoreCase) ||
                 t.TitleAr.Contains("اختراق", StringComparison.OrdinalIgnoreCase)));

            // 5. User Satisfaction
            double satisfaction = 90.0;
            var ratedTickets = resolvedTickets.Where(t => t.UserSatisfaction.HasValue).ToList();
            if (ratedTickets.Any())
            {
                satisfaction = Math.Round((ratedTickets.Average(t => t.UserSatisfaction.Value) / 5.0) * 100, 1);
            }

            // 6. Backup Success Rate
            double backupRate = 100.0;
            if (systems.Any())
            {
                backupRate = Math.Round((double)systems.Count(s => s.LastBackupStatus) / systems.Count * 100, 1);
            }

            // 7. IT Project Delivery
            double projectDelivery = 95.0;
            var itProjects = projects.Where(p => 
                p.NameEn.Contains("IT", StringComparison.OrdinalIgnoreCase) || 
                p.NameEn.Contains("Tech", StringComparison.OrdinalIgnoreCase) || 
                p.NameEn.Contains("System", StringComparison.OrdinalIgnoreCase) ||
                p.NameAr.Contains("تقنية", StringComparison.OrdinalIgnoreCase) ||
                p.NameAr.Contains("نظام", StringComparison.OrdinalIgnoreCase)).ToList();

            if (itProjects.Any())
            {
                var completed = itProjects.Count(p => p.Status == ProjectStatus.Completed);
                projectDelivery = Math.Round((double)completed / itProjects.Count * 100, 1);
            }

            return new ItKpisDto
            {
                DigitalTransformationRateActual = automRate,
                DigitalTransformationRateTarget = 80.0,

                SystemUptimeActual = avgUptime,
                SystemUptimeTarget = 99.9,

                AvgTicketResolutionTimeActual = avgResTime,
                AvgTicketResolutionTimeTarget = 2.0,

                CybersecurityIncidentsActual = securityIncidents,
                CybersecurityIncidentsTarget = 0,

                UserSatisfactionActual = satisfaction,
                UserSatisfactionTarget = 90.0,

                BackupSuccessRateActual = backupRate,
                BackupSuccessRateTarget = 100.0,

                ItProjectDeliveryActual = projectDelivery,
                ItProjectDeliveryTarget = 95.0
            };
        }
        #endregion

        private static bool Contains(string? haystack, string term) =>
            !string.IsNullOrEmpty(haystack) && haystack.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0;

        #region Mappers
        private static ItTicketDto MapToTicketDto(ItTicket t)
        {
            return new ItTicketDto
            {
                Id = t.Id,
                TitleEn = t.TitleEn,
                TitleAr = t.TitleAr,
                DescriptionEn = t.DescriptionEn,
                DescriptionAr = t.DescriptionAr,
                Status = t.Status,
                Priority = t.Priority,
                CreatedAt = t.CreatedAt,
                ResolvedAt = t.ResolvedAt,
                UserSatisfaction = t.UserSatisfaction
            };
        }

        private static ItSystemDto MapToSystemDto(ItSystem s)
        {
            return new ItSystemDto
            {
                Id = s.Id,
                NameEn = s.NameEn,
                NameAr = s.NameAr,
                UptimePercentage = s.UptimePercentage,
                LastBackupStatus = s.LastBackupStatus,
                Status = s.Status,
                IsAutomated = s.IsAutomated
            };
        }
        #endregion
    }
}
