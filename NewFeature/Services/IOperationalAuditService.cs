using System.Collections.Generic;
using System.Threading.Tasks;
using NewFeature.Models;

namespace NewFeature.Services
{
    public interface IOperationalAuditService
    {
        Task<IEnumerable<OperationalAuditDto>> GetAllAuditsAsync();
        Task<OperationalAuditDto?> GetAuditByIdAsync(int id);
        Task<OperationalAuditDto> CreateAuditAsync(OperationalAuditDto dto);
        Task<bool> UpdateAuditAsync(OperationalAuditDto dto);
        Task<bool> DeleteAuditAsync(int id);
        Task<OperationalAuditKpisDto> GetOperationalAuditKpisAsync();
    }
}
