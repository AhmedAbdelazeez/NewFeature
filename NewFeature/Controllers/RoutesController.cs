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
            var created = await _fleetService.CreateRouteAsync(dto);
            return CreatedAtAction(nameof(GetRoute), new { id = created.Id }, created);
        }

        [Authorize(Roles = "ROUTES,Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRoute(int id, [FromBody] RouteDto dto)
        {
            if (id != dto.Id) return BadRequest("ID mismatch");
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var success = await _fleetService.UpdateRouteAsync(dto);
            if (!success) return NotFound();
            return NoContent();
        }

        [Authorize(Roles = "ROUTES,Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRoute(int id)
        {
            var success = await _fleetService.DeleteRouteAsync(id);
            if (!success) return NotFound();
            return NoContent();
        }

        [Authorize(Roles = "ROUTES,Admin")]
        [HttpPost("bulk-upload")]
        public async Task<IActionResult> BulkUpload(Microsoft.AspNetCore.Http.IFormFile file)
        {
            if (file == null || file.Length == 0) return BadRequest("No file uploaded.");
            if (!ExcelCompatibility.IsSupportedExcelFile(file.FileName, file.ContentType))
                return BadRequest("Only .xlsx or .xls files are supported.");

            using var rawStream = file.OpenReadStream();
            using var stream = ExcelCompatibility.EnsureXlsxStream(rawStream);
            var result = await _routeOperationsService.BulkUploadRoutesAsync(stream);

            return Ok(result);
        }
    }
}
