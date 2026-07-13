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
    public class HseController : ControllerBase
    {
        private readonly IHseService _hseService;

        public HseController(IHseService hseService)
        {
            _hseService = hseService;
        }

        #region Incidents
        [HttpGet("incidents")]
        public async Task<ActionResult<IEnumerable<HseIncidentDto>>> GetIncidents()
        {
            var incidents = await _hseService.GetAllIncidentsAsync();
            return Ok(incidents);
        }

        [HttpGet("incidents/{id}")]
        public async Task<ActionResult<HseIncidentDto>> GetIncident(int id)
        {
            var incident = await _hseService.GetIncidentByIdAsync(id);
            if (incident == null) return NotFound();
            return Ok(incident);
        }

        [HttpPost("incidents")]
        public async Task<ActionResult<HseIncidentDto>> CreateIncident([FromBody] HseIncidentDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var created = await _hseService.CreateIncidentAsync(dto);
            return CreatedAtAction(nameof(GetIncident), new { id = created.Id }, created);
        }

        [HttpPut("incidents/{id}")]
        public async Task<IActionResult> UpdateIncident(int id, [FromBody] HseIncidentDto dto)
        {
            if (id != dto.Id) return BadRequest("ID mismatch");
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var success = await _hseService.UpdateIncidentAsync(dto);
            if (!success) return NotFound();
            return NoContent();
        }

        [HttpDelete("incidents/{id}")]
        public async Task<IActionResult> DeleteIncident(int id)
        {
            var success = await _hseService.DeleteIncidentAsync(id);
            if (!success) return NotFound();
            return NoContent();
        }
        #endregion

        #region Inspections
        [HttpGet("inspections")]
        public async Task<ActionResult<IEnumerable<HseInspectionDto>>> GetInspections()
        {
            var inspections = await _hseService.GetAllInspectionsAsync();
            return Ok(inspections);
        }

        [HttpGet("inspections/{id}")]
        public async Task<ActionResult<HseInspectionDto>> GetInspection(int id)
        {
            var inspection = await _hseService.GetInspectionByIdAsync(id);
            if (inspection == null) return NotFound();
            return Ok(inspection);
        }

        [HttpPost("inspections")]
        public async Task<ActionResult<HseInspectionDto>> CreateInspection([FromBody] HseInspectionDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var created = await _hseService.CreateInspectionAsync(dto);
            return CreatedAtAction(nameof(GetInspection), new { id = created.Id }, created);
        }

        [HttpPut("inspections/{id}")]
        public async Task<IActionResult> UpdateInspection(int id, [FromBody] HseInspectionDto dto)
        {
            if (id != dto.Id) return BadRequest("ID mismatch");
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var success = await _hseService.UpdateInspectionAsync(dto);
            if (!success) return NotFound();
            return NoContent();
        }

        [HttpDelete("inspections/{id}")]
        public async Task<IActionResult> DeleteInspection(int id)
        {
            var success = await _hseService.DeleteInspectionAsync(id);
            if (!success) return NotFound();
            return NoContent();
        }
        #endregion

        #region KPIs
        [HttpGet("kpis")]
        public async Task<ActionResult<HseKpisDto>> GetKpis()
        {
            var kpis = await _hseService.GetHseKpisAsync();
            return Ok(kpis);
        }
        #endregion
    }
}
