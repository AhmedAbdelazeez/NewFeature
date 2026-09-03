using System.Collections.Generic;
using System.Threading.Tasks;
using NewFeature.Models;

namespace NewFeature.Services
{
    public interface IClientService
    {
        Task<IEnumerable<ClientDto>> GetAllClientsAsync();
        // Paginated + searchable listing for the Clients management page's table.
        // GetAllClientsAsync above is left untouched in case other callers rely on the full list.
        Task<PagedResultDto<ClientDto>> GetClientsPagedAsync(int page, int pageSize, string? search);
        Task<ClientDto?> GetClientByIdAsync(int id);
        Task<ClientDto> CreateClientAsync(ClientDto dto);
        Task<bool> UpdateClientAsync(ClientDto dto);
        Task<bool> DeleteClientAsync(int id);
    }
}
