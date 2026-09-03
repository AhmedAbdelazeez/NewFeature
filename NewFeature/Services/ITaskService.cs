using System.Collections.Generic;
using System.Threading.Tasks;
using NewFeature.Models;

namespace NewFeature.Services
{
    public interface ITaskService
    {
        Task<IEnumerable<TaskDto>> GetAllTasksAsync();
        Task<PagedResultDto<TaskDto>> GetTasksPagedAsync(int page, int pageSize, string? search, int? projectId);
        Task<TaskDto?> GetTaskByIdAsync(int id);
        Task<TaskDto> CreateTaskAsync(TaskDto dto);
        Task<bool> UpdateTaskAsync(TaskDto dto);
        Task<bool> DeleteTaskAsync(int id);
    }
}
