using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using NewFeature.Models;
using NewFeature.Services;

namespace NewFeature.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [AllowAnonymous] // allowing anonymous access to simplify dashboard integration (same as DashboardApiController)
    public class OperationalAuditsController : ControllerBase
    {
        private readonly IOperationalAuditService _auditService;

        public OperationalAuditsController(IOperationalAuditService auditService)
        {
            _auditService = auditService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<OperationalAuditDto>>> GetAudits()
        {
            var audits = await _auditService.GetAllAuditsAsync();
            return Ok(audits);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<OperationalAuditDto>> GetAudit(int id)
        {
            var audit = await _auditService.GetAuditByIdAsync(id);
            if (audit == null) return NotFound();
            return Ok(audit);
        }

        [HttpGet("kpis")]
        public async Task<ActionResult<OperationalAuditKpisDto>> GetKpis()
        {
            var kpis = await _auditService.GetOperationalAuditKpisAsync();
            return Ok(kpis);
        }

        [HttpPost]
        public async Task<ActionResult<OperationalAuditDto>> CreateAudit([FromBody] OperationalAuditDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var created = await _auditService.CreateAuditAsync(dto);
            return CreatedAtAction(nameof(GetAudit), new { id = created.Id }, created);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAudit(int id, [FromBody] OperationalAuditDto dto)
        {
            if (id != dto.Id) return BadRequest("ID mismatch");
            if (!ModelState.IsValid) return BadRequest(ModelState);
            
            var success = await _auditService.UpdateAuditAsync(dto);
            if (!success) return NotFound();
            
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAudit(int id)
        {
            var success = await _auditService.DeleteAuditAsync(id);
            if (!success) return NotFound();
            
            return NoContent();
        }
    }
}
