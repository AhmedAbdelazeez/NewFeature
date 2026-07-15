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
    [AllowAnonymous]
    public class CommercialController : ControllerBase
    {
        private readonly ICommercialService _commercialService;

        public CommercialController(ICommercialService commercialService)
        {
            _commercialService = commercialService;
        }

        #region Contracts
        [HttpGet("contracts")]
        public async Task<ActionResult<IEnumerable<CommercialContractDto>>> GetContracts()
        {
            var items = await _commercialService.GetAllContractsAsync();
            return Ok(items);
        }

        [HttpGet("contracts/{id}")]
        public async Task<ActionResult<CommercialContractDto>> GetContract(int id)
        {
            var item = await _commercialService.GetContractByIdAsync(id);
            if (item == null) return NotFound();
            return Ok(item);
        }

        [HttpPost("contracts")]
        public async Task<ActionResult<CommercialContractDto>> CreateContract([FromBody] CommercialContractDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var created = await _commercialService.CreateContractAsync(dto);
            return CreatedAtAction(nameof(GetContract), new { id = created.Id }, created);
        }

        [HttpPut("contracts/{id}")]
        public async Task<IActionResult> UpdateContract(int id, [FromBody] CommercialContractDto dto)
        {
            if (id != dto.Id) return BadRequest("ID mismatch");
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var success = await _commercialService.UpdateContractAsync(dto);
            if (!success) return NotFound();
            return NoContent();
        }

        [HttpDelete("contracts/{id}")]
        public async Task<IActionResult> DeleteContract(int id)
        {
            var success = await _commercialService.DeleteContractAsync(id);
            if (!success) return NotFound();
            return NoContent();
        }
        #endregion

        #region Leads
        [HttpGet("leads")]
        public async Task<ActionResult<IEnumerable<CommercialLeadDto>>> GetLeads()
        {
            var items = await _commercialService.GetAllLeadsAsync();
            return Ok(items);
        }

        [HttpGet("leads/{id}")]
        public async Task<ActionResult<CommercialLeadDto>> GetLead(int id)
        {
            var item = await _commercialService.GetLeadByIdAsync(id);
            if (item == null) return NotFound();
            return Ok(item);
        }

        [HttpPost("leads")]
        public async Task<ActionResult<CommercialLeadDto>> CreateLead([FromBody] CommercialLeadDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var created = await _commercialService.CreateLeadAsync(dto);
            return CreatedAtAction(nameof(GetLead), new { id = created.Id }, created);
        }

        [HttpPut("leads/{id}")]
        public async Task<IActionResult> UpdateLead(int id, [FromBody] CommercialLeadDto dto)
        {
            if (id != dto.Id) return BadRequest("ID mismatch");
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var success = await _commercialService.UpdateLeadAsync(dto);
            if (!success) return NotFound();
            return NoContent();
        }

        [HttpDelete("leads/{id}")]
        public async Task<IActionResult> DeleteLead(int id)
        {
            var success = await _commercialService.DeleteLeadAsync(id);
            if (!success) return NotFound();
            return NoContent();
        }
        #endregion

        #region KPIs
        [HttpGet("kpis")]
        public async Task<ActionResult<CommercialKpisDto>> GetKpis()
        {
            var kpis = await _commercialService.GetCommercialKpisAsync();
            return Ok(kpis);
        }
        #endregion
    }
}
