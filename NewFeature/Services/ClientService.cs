using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NewFeature.Models;
using NewFeature.Services.Repositories;

namespace NewFeature.Services
{
    public class ClientService : IClientService
    {
        private readonly IRepository<Client> _clientRepository;

        public ClientService(IRepository<Client> clientRepository)
        {
            _clientRepository = clientRepository;
        }

        public async Task<IEnumerable<ClientDto>> GetAllClientsAsync()
        {
            var clients = await _clientRepository.GetAllAsync();
            return clients.Select(c => new ClientDto
            {
                Id = c.Id,
                NameEn = c.NameEn,
                NameAr = c.NameAr,
                Code = c.Code,
                Email = c.Email,
                Phone = c.Phone
            }).ToList();
        }

        public async Task<ClientDto?> GetClientByIdAsync(int id)
        {
            var c = await _clientRepository.GetByIdAsync(id);
            if (c == null) return null;

            return new ClientDto
            {
                Id = c.Id,
                NameEn = c.NameEn,
                NameAr = c.NameAr,
                Code = c.Code,
                Email = c.Email,
                Phone = c.Phone
            };
        }

        public async Task<ClientDto> CreateClientAsync(ClientDto dto)
        {
            var client = new Client
            {
                NameEn = dto.NameEn,
                NameAr = dto.NameAr,
                Code = dto.Code,
                Email = dto.Email,
                Phone = dto.Phone
            };

            await _clientRepository.AddAsync(client);
            await _clientRepository.SaveChangesAsync();

            dto.Id = client.Id;
            return dto;
        }

        public async Task<bool> UpdateClientAsync(ClientDto dto)
        {
            var client = await _clientRepository.GetByIdAsync(dto.Id);
            if (client == null) return false;

            client.NameEn = dto.NameEn;
            client.NameAr = dto.NameAr;
            client.Code = dto.Code;
            client.Email = dto.Email;
            client.Phone = dto.Phone;

            await _clientRepository.UpdateAsync(client);
            await _clientRepository.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteClientAsync(int id)
        {
            var client = await _clientRepository.GetByIdAsync(id);
            if (client == null) return false;

            await _clientRepository.DeleteAsync(client);
            await _clientRepository.SaveChangesAsync();
            return true;
        }
    }
}
