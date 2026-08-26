using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NewFeature.Models;
using NewFeature.Services;

namespace NewFeature.Controllers
{
    // Sales Department API. Read endpoints are anonymous to match this project's existing
    // inter-app integration convention (the `project` CEO-dashboard app calls these endpoints
    // server-to-server with no auth token configured between the two apps - the same pattern every
    // other department's *-kpis endpoint already uses). The upload and import-history endpoints are
    // the exception: they require an authenticated Sales-department (or Admin) user, since the task
    // explicitly requires the import endpoint to be protected from unauthorized use.
    [ApiController]
    [Route("api/sales")]
    public class SalesController : ControllerBase
    {
        private const long MaxUploadBytes = 20 * 1024 * 1024; // 20 MB

        private readonly ISalesService _salesService;
        private readonly UserManager<ApplicationUser> _userManager;

        public SalesController(ISalesService salesService, UserManager<ApplicationUser> userManager)
        {
            _salesService = salesService;
            _userManager = userManager;
        }

        #region Read endpoints (dashboard + KPIs)

        [AllowAnonymous]
        [HttpGet("kpis")]
        public async Task<ActionResult<SalesKpisDto>> GetKpis()
        {
            var kpis = await _salesService.GetSalesKpisAsync();
            return Ok(kpis);
        }

        [AllowAnonymous]
        [HttpGet("customers/summary")]
        public async Task<ActionResult<SalesCustomersOverviewDto>> GetCustomersSummary([FromQuery] int? year = null)
        {
            var summary = await _salesService.GetCustomersOverviewAsync(year);
            return Ok(summary);
        }

        [AllowAnonymous]
        [HttpGet("fleet-capacity")]
        public async Task<ActionResult<SalesFleetCapacitySummaryDto>> GetFleetCapacity(
            [FromQuery] string? category = null,
            [FromQuery] string? busType = null,
            [FromQuery] string? model = null)
        {
            var summary = await _salesService.GetFleetCapacitySummaryAsync(category, busType, model);
            return Ok(summary);
        }

        [AllowAnonymous]
        [HttpGet("daily-operations")]
        public async Task<ActionResult<PagedResultDto<SalesDailyOperationDto>>> GetDailyOperations(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null)
        {
            var result = await _salesService.GetDailyOperationsAsync(page, pageSize, fromDate, toDate);
            return Ok(result);
        }

        #endregion

        #region Upload + import history (Sales department or Admin only)

        [Authorize(Roles = "SALES,Admin")]
        [HttpGet("import-history")]
        public async Task<ActionResult<PagedResultDto<SalesImportBatchDto>>> GetImportHistory(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var result = await _salesService.GetImportHistoryAsync(page, pageSize);
            return Ok(result);
        }

        // Single upload endpoint for the one combined template (Sales_Department_Excel_Templates.xlsx,
        // 3 sheets: year-named customer roster, "Fleet Capacity", "Daily Operations"). Replaces the
        // three separate upload/customers, upload/fleet-capacity and upload/daily-operations endpoints
        // that used to require three separate files.
        [Authorize(Roles = "SALES,Admin")]
        [HttpPost("upload")]
        public Task<ActionResult<SalesImportResultDto>> Upload(IFormFile file) =>
            HandleUploadAsync(file, _salesService.BulkUploadCombinedAsync);

        private async Task<ActionResult<SalesImportResultDto>> HandleUploadAsync(
            IFormFile file,
            Func<Stream, byte[], string, string, Task<SalesImportResultDto>> uploadFunc)
        {
            if (file == null || file.Length == 0) return BadRequest("No file uploaded.");
            if (file.Length > MaxUploadBytes) return BadRequest($"File exceeds the maximum allowed size of {MaxUploadBytes / (1024 * 1024)} MB.");
            if (!ExcelCompatibility.IsSupportedExcelFile(file.FileName, file.ContentType))
                return BadRequest("Only .xlsx or .xls files are supported.");

            var user = await _userManager.GetUserAsync(User);
            var uploadedByUserId = user?.Id ?? "system";

            try
            {
                using var rawMemory = new MemoryStream();
                await using (var uploadStream = file.OpenReadStream())
                {
                    await uploadStream.CopyToAsync(rawMemory);
                }
                var rawBytes = rawMemory.ToArray();

                using var sourceStream = new MemoryStream(rawBytes);
                using var xlsxStream = ExcelCompatibility.EnsureXlsxStream(sourceStream);

                var result = await uploadFunc(xlsxStream, rawBytes, file.FileName, uploadedByUserId);

                if (result.Status == SalesImportStatus.Failed.ToString())
                {
                    // Validation/parsing failure - the request itself was well-formed, so this is a
                    // 422 (unprocessable), never a raw server exception leaking internals.
                    return UnprocessableEntity(result);
                }

                return Ok(result);
            }
            catch (Exception)
            {
                // Never leak internal exception details/paths to the client - the failure is already
                // logged server-side by the service layer.
                return StatusCode(500, new { message = "An unexpected error occurred while processing the file." });
            }
        }

        #endregion
    }
}
