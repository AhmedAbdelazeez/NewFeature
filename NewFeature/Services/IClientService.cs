using System.Collections.Generic;
using System.Threading.Tasks;
using NewFeature.Models;

namespace NewFeature.Services
{
    public interface IClientService
    {
        Task<IEnumerable<ClientDto>> GetAllClientsAsync();
        Task<ClientDto?> GetClientByIdAsync(int id);
        Task<ClientDto> CreateClientAsync(ClientDto dto);
        Task<bool> UpdateClientAsync(ClientDto dto);
        Task<bool> DeleteClientAsync(int id);
    }
}
