using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NewFeature.Models;
using NewFeature.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NewFeature.Controllers
{
    [ApiController]
    [Route("api/[controller]")]

    [AllowAnonymous]
    public class OperationsController : ControllerBase
    {
        private readonly IOperationsService _operationsService;

        public OperationsController(IOperationsService operationsService)
        {
            _operationsService = operationsService;
        }

        #region Daily Plans
        [HttpGet("dailyplans")]
        public async Task<ActionResult<IEnumerable<OperationsDailyPlanDto>>> GetDailyPlans()
        {
            var plans = await _operationsService.GetAllDailyPlansAsync();
            return Ok(plans);
        }

        [HttpGet("dailyplans/{id}")]
        public async Task<ActionResult<OperationsDailyPlanDto>> GetDailyPlan(int id)
        {
            var plan = await _operationsService.GetDailyPlanByIdAsync(id);
            if (plan == null) return NotFound();
            return Ok(plan);
        }

        [HttpPost("dailyplans")]
        public async Task<ActionResult<OperationsDailyPlanDto>> CreateDailyPlan([FromBody] OperationsDailyPlanDto dto)
        {
            var created = await _operationsService.CreateDailyPlanAsync(dto);
            return CreatedAtAction(nameof(GetDailyPlan), new { id = created.Id }, created);
        }

        [HttpPut("dailyplans/{id}")]
        public async Task<IActionResult> UpdateDailyPlan(int id, [FromBody] OperationsDailyPlanDto dto)
        {
            if (id != dto.Id) return BadRequest();
            var result = await _operationsService.UpdateDailyPlanAsync(dto);
            if (!result) return NotFound();
            return NoContent();
        }

        [HttpDelete("dailyplans/{id}")]
        public async Task<IActionResult> DeleteDailyPlan(int id)
        {
            var result = await _operationsService.DeleteDailyPlanAsync(id);
            if (!result) return NotFound();
            return NoContent();
        }
        #endregion

        #region Incidents
        [HttpGet("incidents")]
        public async Task<ActionResult<IEnumerable<OperationsIncidentDto>>> GetIncidents()
        {
            var incidents = await _operationsService.GetAllIncidentsAsync();
            return Ok(incidents);
        }

        [HttpGet("incidents/{id}")]
        public async Task<ActionResult<OperationsIncidentDto>> GetIncident(int id)
        {
            var incident = await _operationsService.GetIncidentByIdAsync(id);
            if (incident == null) return NotFound();
            return Ok(incident);
        }

        [HttpPost("incidents")]
        public async Task<ActionResult<OperationsIncidentDto>> CreateIncident([FromBody] OperationsIncidentDto dto)
        {
            var created = await _operationsService.CreateIncidentAsync(dto);
            return CreatedAtAction(nameof(GetIncident), new { id = created.Id }, created);
        }

        [HttpPut("incidents/{id}")]
        public async Task<IActionResult> UpdateIncident(int id, [FromBody] OperationsIncidentDto dto)
        {
            if (id != dto.Id) return BadRequest();
            var result = await _operationsService.UpdateIncidentAsync(dto);
            if (!result) return NotFound();
            return NoContent();
        }

        [HttpDelete("incidents/{id}")]
        public async Task<IActionResult> DeleteIncident(int id)
        {
            var result = await _operationsService.DeleteIncidentAsync(id);
            if (!result) return NotFound();
            return NoContent();
        }
        #endregion

        #region KPIs
        [HttpGet("kpis")]
        public async Task<ActionResult<OperationsKpisDto>> GetOperationsKpis()
        {
            var kpis = await _operationsService.GetOperationsKpisAsync();
            return Ok(kpis);
        }
        #endregion
    }
}
