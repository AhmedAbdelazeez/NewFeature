using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NewFeature.Models;
using NewFeature.Services.Repositories;

namespace NewFeature.Services
{
    public class TaskService : ITaskService
    {
        private readonly IRepository<Models.Task> _taskRepository;
        private readonly IRepository<Project> _projectRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TaskService(
            IRepository<Models.Task> taskRepository,
            IRepository<Project> projectRepository,
            UserManager<ApplicationUser> userManager,
            IHttpContextAccessor httpContextAccessor)
        {
            _taskRepository = taskRepository;
            _projectRepository = projectRepository;
            _userManager = userManager;
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

        public async Task<IEnumerable<TaskDto>> GetAllTasksAsync()
        {
            var tasks = await _taskRepository.GetAllAsync();
            var projects = await _projectRepository.GetAllAsync();
            var users = await _userManager.Users.ToListAsync();
            var isAr = IsArabic();

            var projectMap = projects.ToDictionary(p => p.Id, p => isAr ? p.NameAr : p.NameEn);
            var userMap = users.ToDictionary(u => u.Id, u => isAr ? u.FullNameAr : u.FullNameEn);

            return tasks.OrderByDescending(t => t.Id).Select(t => new TaskDto
            {
                Id = t.Id,
                ProjectId = t.ProjectId,
                ProjectName = projectMap.TryGetValue(t.ProjectId, out var projectName) ? projectName : "Unknown",
                TitleEn = t.TitleEn,
                TitleAr = t.TitleAr,
                DescriptionEn = t.DescriptionEn,
                DescriptionAr = t.DescriptionAr,
                StartDate = t.StartDate,
                DueDate = t.DueDate,
                EstimatedHours = t.EstimatedHours,
                Status = t.Status,
                AssignedToUserId = t.AssignedToUserId,
                AssignedToUserName = t.AssignedToUserId != null && userMap.TryGetValue(t.AssignedToUserId, out var userName) ? userName : "Unassigned",
                Title = isAr ? t.TitleAr : t.TitleEn,
                Description = isAr ? t.DescriptionAr : t.DescriptionEn
            }).ToList();
        }

        // Backs the Project Management page's per-project Tasks tab. Same in-memory
        // page/search/order approach as FleetService.GetVehiclesPagedAsync; projectId is an
        // optional filter since this tab always scopes tasks to one project.
        public async Task<PagedResultDto<TaskDto>> GetTasksPagedAsync(int page, int pageSize, string? search, int? projectId)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 20;
            if (pageSize > 200) pageSize = 200;

            var tasks = await _taskRepository.GetAllAsync();
            var projects = await _projectRepository.GetAllAsync();
            var users = await _userManager.Users.ToListAsync();
            var isAr = IsArabic();

            var projectMap = projects.ToDictionary(p => p.Id, p => isAr ? p.NameAr : p.NameEn);
            var userMap = users.ToDictionary(u => u.Id, u => isAr ? u.FullNameAr : u.FullNameEn);


            var dtos = tasks.Select(t => new TaskDto

            {
                Id = t.Id,
                ProjectId = t.ProjectId,
                ProjectName = projectMap.TryGetValue(t.ProjectId, out var projectName) ? projectName : "Unknown",
                TitleEn = t.TitleEn,
                TitleAr = t.TitleAr,
                DescriptionEn = t.DescriptionEn,
                DescriptionAr = t.DescriptionAr,
                StartDate = t.StartDate,
                DueDate = t.DueDate,
                EstimatedHours = t.EstimatedHours,
                Status = t.Status,
                AssignedToUserId = t.AssignedToUserId,
                AssignedToUserName = t.AssignedToUserId != null && userMap.TryGetValue(t.AssignedToUserId, out var userName) ? userName : "Unassigned",
                Title = isAr ? t.TitleAr : t.TitleEn,
                Description = isAr ? t.DescriptionAr : t.DescriptionEn
            }).AsEnumerable();

            if (projectId.HasValue)
            {
                dtos = dtos.Where(d => d.ProjectId == projectId.Value);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                dtos = dtos.Where(d =>
                    Contains(d.TitleEn, term) ||
                    Contains(d.TitleAr, term) ||
                    Contains(d.DescriptionEn, term) ||
                    Contains(d.DescriptionAr, term) ||
                    Contains(d.Status.ToString(), term));
            }

            var ordered = dtos.OrderByDescending(d => d.Id).ToList();
            var totalCount = ordered.Count;

            var items = ordered.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            return new PagedResultDto<TaskDto>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        private static bool Contains(string? haystack, string term) =>
            !string.IsNullOrEmpty(haystack) && haystack.IndexOf(term, System.StringComparison.OrdinalIgnoreCase) >= 0;

        public async Task<TaskDto?> GetTaskByIdAsync(int id)
        {
            var t = await _taskRepository.GetByIdAsync(id);
            if (t == null) return null;

            var project = await _projectRepository.GetByIdAsync(t.ProjectId);
            var user = t.AssignedToUserId != null ? await _userManager.FindByIdAsync(t.AssignedToUserId) : null;
            var isAr = IsArabic();

            return new TaskDto
            {
                Id = t.Id,
                ProjectId = t.ProjectId,
                ProjectName = project != null ? (isAr ? project.NameAr : project.NameEn) : "Unknown",
                TitleEn = t.TitleEn,
                TitleAr = t.TitleAr,
                DescriptionEn = t.DescriptionEn,
                DescriptionAr = t.DescriptionAr,
                StartDate = t.StartDate,
                DueDate = t.DueDate,
                EstimatedHours = t.EstimatedHours,
                Status = t.Status,
                AssignedToUserId = t.AssignedToUserId,
                AssignedToUserName = user != null ? (isAr ? user.FullNameAr : user.FullNameEn) : "Unassigned",
                Title = isAr ? t.TitleAr : t.TitleEn,
                Description = isAr ? t.DescriptionAr : t.DescriptionEn
            };
        }

        public async Task<TaskDto> CreateTaskAsync(TaskDto dto)
        {
            var task = new Models.Task
            {
                ProjectId = dto.ProjectId,
                TitleEn = dto.TitleEn,
                TitleAr = dto.TitleAr,
                DescriptionEn = dto.DescriptionEn,
                DescriptionAr = dto.DescriptionAr,
                StartDate = dto.StartDate,
                DueDate = dto.DueDate,
                EstimatedHours = dto.EstimatedHours,
                Status = dto.Status,
                AssignedToUserId = dto.AssignedToUserId
            };

            await _taskRepository.AddAsync(task);
            await _taskRepository.SaveChangesAsync();

            dto.Id = task.Id;
            return dto;
        }

        public async Task<bool> UpdateTaskAsync(TaskDto dto)
        {
            var task = await _taskRepository.GetByIdAsync(dto.Id);
            if (task == null) return false;

            task.ProjectId = dto.ProjectId;
            task.TitleEn = dto.TitleEn;
            task.TitleAr = dto.TitleAr;
            task.DescriptionEn = dto.DescriptionEn;
            task.DescriptionAr = dto.DescriptionAr;
            task.StartDate = dto.StartDate;
            task.DueDate = dto.DueDate;
            task.EstimatedHours = dto.EstimatedHours;
            task.Status = dto.Status;
            task.AssignedToUserId = dto.AssignedToUserId;

            await _taskRepository.UpdateAsync(task);
            await _taskRepository.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteTaskAsync(int id)
        {
            var task = await _taskRepository.GetByIdAsync(id);
            if (task == null) return false;

            await _taskRepository.DeleteAsync(task);
            await _taskRepository.SaveChangesAsync();
            return true;
        }
    }
}
