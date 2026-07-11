using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using NewFeature.Models;
using NewFeature.Services.Repositories;

namespace NewFeature.Services
{
    public class ClientService : IClientService
    {
        private readonly IRepository<Client> _clientRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ClientService(IRepository<Client> clientRepository, IHttpContextAccessor httpContextAccessor)
        {
            _clientRepository = clientRepository;
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

        public async Task<IEnumerable<ClientDto>> GetAllClientsAsync()
        {
            var clients = await _clientRepository.GetAllAsync();
            var isAr = IsArabic();
            return clients.OrderByDescending(c => c.Id).Select(c => new ClientDto
            {
                Id = c.Id,
                NameEn = c.NameEn,
                NameAr = c.NameAr,
                Code = c.Code,
                Email = c.Email,
                Phone = c.Phone,
                Name = isAr ? c.NameAr : c.NameEn
            }).ToList();
        }

        public async Task<ClientDto?> GetClientByIdAsync(int id)
        {
            var c = await _clientRepository.GetByIdAsync(id);
            if (c == null) return null;

            var isAr = IsArabic();
            return new ClientDto
            {
                Id = c.Id,
                NameEn = c.NameEn,
                NameAr = c.NameAr,
                Code = c.Code,
                Email = c.Email,
                Phone = c.Phone,
                Name = isAr ? c.NameAr : c.NameEn
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
