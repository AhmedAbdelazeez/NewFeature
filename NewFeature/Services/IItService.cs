using System.Collections.Generic;
using System.Threading.Tasks;
using NewFeature.Models;

namespace NewFeature.Services
{
    public interface IItService
    {
        // Tickets CRUD
        Task<IEnumerable<ItTicketDto>> GetAllTicketsAsync();
        Task<ItTicketDto?> GetTicketByIdAsync(int id);
        Task<ItTicketDto> CreateTicketAsync(ItTicketDto dto);
        Task<bool> UpdateTicketAsync(ItTicketDto dto);
        Task<bool> DeleteTicketAsync(int id);

        // Tickets - paginated + searchable listing for the IT page's table
        Task<PagedResultDto<ItTicketDto>> GetTicketsPagedAsync(int page, int pageSize, string? search, string? status);

        // Systems CRUD
        Task<IEnumerable<ItSystemDto>> GetAllSystemsAsync();
        Task<ItSystemDto?> GetSystemByIdAsync(int id);
        Task<ItSystemDto> CreateSystemAsync(ItSystemDto dto);
        Task<bool> UpdateSystemAsync(ItSystemDto dto);
        Task<bool> DeleteSystemAsync(int id);

        // Systems - paginated + searchable listing for the IT page's table
        Task<PagedResultDto<ItSystemDto>> GetSystemsPagedAsync(int page, int pageSize, string? search);

        // KPIs
        Task<ItKpisDto> GetItKpisAsync();
    }
}
