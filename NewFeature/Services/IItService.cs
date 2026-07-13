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

        // Systems CRUD
        Task<IEnumerable<ItSystemDto>> GetAllSystemsAsync();
        Task<ItSystemDto?> GetSystemByIdAsync(int id);
        Task<ItSystemDto> CreateSystemAsync(ItSystemDto dto);
        Task<bool> UpdateSystemAsync(ItSystemDto dto);
        Task<bool> DeleteSystemAsync(int id);

        // KPIs
        Task<ItKpisDto> GetItKpisAsync();
    }
}
