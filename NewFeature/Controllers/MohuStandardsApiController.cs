using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using NewFeature.Models;
using NewFeature.Services;

namespace NewFeature.Controllers
{
    [ApiController]
    [Route("api/mohu-standards")]
    [AllowAnonymous] // Assuming dashboard has its own access logic or auth is simple
    public class MohuStandardsApiController : ControllerBase
    {
        private readonly IMohuStandardsService _service;

        public MohuStandardsApiController(IMohuStandardsService service)
        {
            _service = service;
        }

        // GET: api/mohu-standards/all
        [HttpGet("all")]
        public async Task<ActionResult<List<MohuKpiDto>>> GetAllKpis()
        {
            var kpis = await _service.GetAllKpisAsync();
            return Ok(kpis);
        }

        // GET: api/mohu-standards/category/{categoryCode}
        [HttpGet("category/{categoryCode}")]
        public async Task<ActionResult<List<MohuKpiDto>>> GetKpisByCategory(string categoryCode)
        {
            var kpis = await _service.GetKpisByCategoryAsync(categoryCode);
            return Ok(kpis);
        }

        // GET: api/mohu-standards/{kpiCode}
        [HttpGet("{kpiCode}")]
        public async Task<ActionResult<MohuKpiDto>> GetKpiByCode(string kpiCode)
        {
            var kpi = await _service.GetKpiByCodeAsync(kpiCode);
            if (kpi == null)
            {
                return NotFound(new { message = $"KPI with code {kpiCode} not found." });
            }
            return Ok(kpi);
        }

        // PUT: api/mohu-standards/{kpiCode}
        [HttpPut("{kpiCode}")]
        public async Task<IActionResult> UpdateKpi(string kpiCode, [FromBody] UpdateMohuKpiDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var success = await _service.UpdateKpiValuesAsync(kpiCode, dto);
            if (!success)
            {
                return NotFound(new { message = $"KPI with code {kpiCode} not found." });
            }

            return Ok(new { message = "KPI updated successfully." });
        }

        // POST: api/mohu-standards
        [HttpPost]
        public async Task<ActionResult<MohuKpiDto>> CreateKpi([FromBody] MohuKpiDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var created = await _service.CreateKpiAsync(dto);
            return CreatedAtAction(nameof(GetKpiByCode), new { kpiCode = created.KpiCode }, created);
        }

        // DELETE: api/mohu-standards/{kpiCode}
        [HttpDelete("{kpiCode}")]
        public async Task<IActionResult> DeleteKpi(string kpiCode)
        {
            var success = await _service.DeleteKpiAsync(kpiCode);
            if (!success)
                return NotFound(new { message = $"KPI with code {kpiCode} not found." });

            return Ok(new { message = "KPI deleted successfully." });
        }
    }
}
