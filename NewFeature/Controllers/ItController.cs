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
    [AllowAnonymous] // same behavior like we do in project management/compliance/hr
    public class ItController : ControllerBase
    {
        private readonly IItService _itService;

        public ItController(IItService itService)
        {
            _itService = itService;
        }

        #region Tickets
        [HttpGet("tickets")]
        public async Task<ActionResult<IEnumerable<ItTicketDto>>> GetTickets()
        {
            var tickets = await _itService.GetAllTicketsAsync();
            return Ok(tickets);
        }

        [HttpGet("tickets/{id}")]
        public async Task<ActionResult<ItTicketDto>> GetTicket(int id)
        {
            var ticket = await _itService.GetTicketByIdAsync(id);
            if (ticket == null) return NotFound();
            return Ok(ticket);
        }

        [HttpPost("tickets")]
        public async Task<ActionResult<ItTicketDto>> CreateTicket([FromBody] ItTicketDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var created = await _itService.CreateTicketAsync(dto);
            return CreatedAtAction(nameof(GetTicket), new { id = created.Id }, created);
        }

        [HttpPut("tickets/{id}")]
        public async Task<IActionResult> UpdateTicket(int id, [FromBody] ItTicketDto dto)
        {
            if (id != dto.Id) return BadRequest("ID mismatch");
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var success = await _itService.UpdateTicketAsync(dto);
            if (!success) return NotFound();
            return NoContent();
        }

        [HttpDelete("tickets/{id}")]
        public async Task<IActionResult> DeleteTicket(int id)
        {
            var success = await _itService.DeleteTicketAsync(id);
            if (!success) return NotFound();
            return NoContent();
        }
        #endregion

        #region Systems
        [HttpGet("systems")]
        public async Task<ActionResult<IEnumerable<ItSystemDto>>> GetSystems()
        {
            var systems = await _itService.GetAllSystemsAsync();
            return Ok(systems);
        }

        [HttpGet("systems/{id}")]
        public async Task<ActionResult<ItSystemDto>> GetSystem(int id)
        {
            var system = await _itService.GetSystemByIdAsync(id);
            if (system == null) return NotFound();
            return Ok(system);
        }

        [HttpPost("systems")]
        public async Task<ActionResult<ItSystemDto>> CreateSystem([FromBody] ItSystemDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var created = await _itService.CreateSystemAsync(dto);
            return CreatedAtAction(nameof(GetSystem), new { id = created.Id }, created);
        }

        [HttpPut("systems/{id}")]
        public async Task<IActionResult> UpdateSystem(int id, [FromBody] ItSystemDto dto)
        {
            if (id != dto.Id) return BadRequest("ID mismatch");
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var success = await _itService.UpdateSystemAsync(dto);
            if (!success) return NotFound();
            return NoContent();
        }

        [HttpDelete("systems/{id}")]
        public async Task<IActionResult> DeleteSystem(int id)
        {
            var success = await _itService.DeleteSystemAsync(id);
            if (!success) return NotFound();
            return NoContent();
        }
        #endregion

        #region KPIs
        [HttpGet("kpis")]
        public async Task<ActionResult<ItKpisDto>> GetKpis()
        {
            var kpis = await _itService.GetItKpisAsync();
            return Ok(kpis);
        }
        #endregion
    }
}
