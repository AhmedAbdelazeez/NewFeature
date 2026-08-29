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

        [HttpPost("bulk-upload-daily-plans")]
        public async Task<IActionResult> BulkUploadDailyPlans(Microsoft.AspNetCore.Http.IFormFile file)
        {
            if (file == null || file.Length == 0) return BadRequest("No file uploaded.");
            if (!ExcelCompatibility.IsSupportedExcelFile(file.FileName, file.ContentType))
                return BadRequest("Only .xlsx or .xls files are supported.");

            using var rawStream = file.OpenReadStream();
            using var stream = ExcelCompatibility.EnsureXlsxStream(rawStream);
            var result = await _operationsService.BulkUploadDailyPlansAsync(stream);

            return Ok(new { successCount = result.SuccessCount, errors = result.Errors });
        }

        // Uploads the real monthly operations sheet (e.g. "تشغيل شهر مايو 2026.xlsx") which contains
        // one row per executed trip. Dynamically resolves/creates Vehicles, Routes and Drivers, and
        // creates Trip records used to compute the real OTP / Total Trips / Active Drivers / Fuel-Odometer KPIs.
        [HttpPost("bulk-upload-trips")]
        public async Task<IActionResult> BulkUploadOperationsTrips(Microsoft.AspNetCore.Http.IFormFile file)
        {
            if (file == null || file.Length == 0) return BadRequest("No file uploaded.");
            if (!ExcelCompatibility.IsSupportedExcelFile(file.FileName, file.ContentType))
                return BadRequest("Only .xlsx or .xls files are supported.");

            using var rawStream = file.OpenReadStream();
            using var stream = ExcelCompatibility.EnsureXlsxStream(rawStream);
            var result = await _operationsService.BulkUploadOperationsTripsAsync(stream);

            return Ok(new { successCount = result.SuccessCount, errors = result.Errors });
        }

        // Replaces the official-drivers compliance roster snapshot (اسطول الحافلات - السائقين الرسميين.xlsx).
        [HttpPost("bulk-upload-drivers")]
        public async Task<IActionResult> BulkUploadOfficialDrivers(Microsoft.AspNetCore.Http.IFormFile file)
        {
            if (file == null || file.Length == 0) return BadRequest("No file uploaded.");
            if (!ExcelCompatibility.IsSupportedExcelFile(file.FileName, file.ContentType))
                return BadRequest("Only .xlsx or .xls files are supported.");

            using var rawStream = file.OpenReadStream();
            using var stream = ExcelCompatibility.EnsureXlsxStream(rawStream);
            var result = await _operationsService.BulkUploadOfficialDriversAsync(stream);

            return Ok(new { successCount = result.SuccessCount, errors = result.Errors });
        }

        // Replaces the route-scheduling requests snapshot (جدولة الخطوط.xlsx).
        [HttpPost("bulk-upload-route-schedules")]
        public async Task<IActionResult> BulkUploadRouteSchedules(Microsoft.AspNetCore.Http.IFormFile file)
        {
            if (file == null || file.Length == 0) return BadRequest("No file uploaded.");
            if (!ExcelCompatibility.IsSupportedExcelFile(file.FileName, file.ContentType))
                return BadRequest("Only .xlsx or .xls files are supported.");

            using var rawStream = file.OpenReadStream();
            using var stream = ExcelCompatibility.EnsureXlsxStream(rawStream);
            var result = await _operationsService.BulkUploadRouteSchedulesAsync(stream);

            return Ok(new { successCount = result.SuccessCount, errors = result.Errors });
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

        // Paged, searchable listing of the real uploaded trip log - lets the Operations landing page
        // show an actual browsable table instead of only aggregate KPI numbers.
        [HttpGet("trips")]
        public async Task<ActionResult<PagedResultDto<OperationsTripDto>>> GetTrips(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? search = null,
            [FromQuery] System.DateTime? fromDate = null,
            [FromQuery] System.DateTime? toDate = null)
        {
            var result = await _operationsService.GetTripsPagedAsync(page, pageSize, search, fromDate, toDate);
            return Ok(result);
        }
        #endregion
    }
}
