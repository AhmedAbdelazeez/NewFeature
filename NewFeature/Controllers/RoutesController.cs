using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using NewFeature.Models;
using NewFeature.Services;
using NewFeature.Services.ExcelImport;

namespace NewFeature.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RoutesController : ControllerBase
    {
        private readonly IFleetService _fleetService;
        private readonly IRouteOperationsService _routeOperationsService;

        public RoutesController(IFleetService fleetService, IRouteOperationsService routeOperationsService)
        {
            _fleetService = fleetService;
            _routeOperationsService = routeOperationsService;
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<RouteDto>>> GetRoutes()
        {
            var routes = await _fleetService.GetAllRoutesAsync();
            return Ok(routes);
        }

        // Paginated + searchable listing used by the Routes management page's table.
        // GetRoutes() above is left untouched in case other callers rely on the full list.
        [AllowAnonymous]
        [HttpGet("paged")]
        public async Task<ActionResult<PagedResultDto<RouteDto>>> GetRoutesPaged(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? search = null)
        {
            var result = await _fleetService.GetRoutesPagedAsync(page, pageSize, search);
            return Ok(result);
        }

        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<ActionResult<RouteDto>> GetRoute(int id)
        {
            var route = await _fleetService.GetRouteByIdAsync(id);
            if (route == null) return NotFound();
            return Ok(route);
        }

        [AllowAnonymous]
        [HttpGet("kpis")]
        public async Task<ActionResult<RouteOperationsKpisDto>> GetRouteOperationsKpis()
        {
            var kpis = await _routeOperationsService.GetRouteOperationsKpisAsync();
            return Ok(kpis);
        }

        [Authorize(Roles = "ROUTES,Admin")]
        [HttpPost]
        public async Task<ActionResult<RouteDto>> CreateRoute([FromBody] RouteDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (await _fleetService.RouteCodeExistsAsync(dto.Code, excludeId: null))
                return DuplicateCode(dto.Code);
            var created = await _fleetService.CreateRouteAsync(dto);
            return CreatedAtAction(nameof(GetRoute), new { id = created.Id }, created);
        }

        [Authorize(Roles = "ROUTES,Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRoute(int id, [FromBody] RouteDto dto)
        {
            if (id != dto.Id) return BadRequest("ID mismatch");
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (await _fleetService.RouteCodeExistsAsync(dto.Code, excludeId: id))
                return DuplicateCode(dto.Code);
            var success = await _fleetService.UpdateRouteAsync(dto);
            if (!success) return NotFound();
            return NoContent();
        }

        private ActionResult DuplicateCode(string code)
        {
            ModelState.AddModelError(nameof(RouteDto.Code), $"كود المسار \"{code.Trim()}\" مستخدم لمسار آخر. يجب أن يكون لكل مسار كود مختلف.");
            return ValidationProblem(ModelState);
        }

        [Authorize(Roles = "ROUTES,Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRoute(int id)
        {
            var success = await _fleetService.DeleteRouteAsync(id);
            if (!success) return NotFound();
            return NoContent();
        }

        // The downloadable template, generated from the very definition the importer validates
        // against - so it always carries the Item Code column the upload now requires.
        [HttpGet("template")]
        public IActionResult DownloadTemplate()
        {
            var bytes = ExcelTemplateWriter.Build(
                "نموذج المسارات - Route Operations",
                new[]
                {
                    new ExcelTemplateSheetSpec
                    {
                        Definition = RouteOperationsService.RouteTemplate,
                        SheetName = "Route Operations"
                    }
                },
                new[]
                {
                    "ملاحظات:",
                    "- Item Code هو كود المسار (مثل 1 أو L10) وهو إلزامي ولا يتكرر؛ وهو نفس الكود الوارد في عمود Direction بسجل أوامر التشغيل.",
                    "- إعادة رفع الملف تُحدّث المسار صاحب نفس الكود بدل تكراره.",
                    "- مواقع البداية والنهاية اختيارية، والمسافة صفر مقبولة للبنود التي لا تُقاس بالمسافة (مثل تأجير حافلة يومي)."
                });

            return File(bytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "Route_Operations_Template.xlsx");
        }

        [Authorize(Roles = "ROUTES,Admin")]
        [HttpPost("bulk-upload")]
        public async Task<IActionResult> BulkUpload(Microsoft.AspNetCore.Http.IFormFile file)
        {
            if (file == null || file.Length == 0) return BadRequest("لم يتم اختيار أي ملف.");
            if (!ExcelCompatibility.IsSupportedExcelFile(file.FileName, file.ContentType))
                return BadRequest("الملفات المدعومة هي .xlsx و .xls فقط.");

            using var rawStream = file.OpenReadStream();
            using var stream = ExcelCompatibility.EnsureXlsxStream(rawStream);
            var result = await _routeOperationsService.BulkUploadRoutesAsync(stream);

            // A template/header problem is the uploader's mistake - 422 so the page shows it as a
            // failure with its message, never as a green "success" with zero rows.
            if (!result.Success) return UnprocessableEntity(result);
            return Ok(result);
        }
    }
}
