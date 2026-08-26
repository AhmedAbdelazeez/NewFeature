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
    public class VehiclesController : ControllerBase
    {
        private readonly IFleetService _fleetService;

        public VehiclesController(IFleetService fleetService)
        {
            _fleetService = fleetService;
        }

        // Reads stay anonymous: this is how the CEO Dashboard project's unauthenticated
        // server-to-server client reaches KPI/listing data, matching every other department's
        // read endpoints in this codebase (Warehouse, Maintenance, Sales).
        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<VehicleDto>>> GetVehicles()
        {
            var vehicles = await _fleetService.GetAllVehiclesAsync();
            return Ok(vehicles);
        }

        // Paginated + searchable listing used by the Vehicles management page's table (the fleet
        // register this table is seeded from has 700+ vehicles - too many for the old single
        // unpaginated GetVehicles() call above to render reasonably). GetVehicles() above is left
        // untouched: Trips and Maintenance both call it directly to populate a vehicle dropdown and
        // expect a plain array back.
        [AllowAnonymous]
        [HttpGet("paged")]
        public async Task<ActionResult<PagedResultDto<VehicleDto>>> GetVehiclesPaged(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? search = null)
        {
            var result = await _fleetService.GetVehiclesPagedAsync(page, pageSize, search);
            return Ok(result);
        }

        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<ActionResult<VehicleDto>> GetVehicle(int id)
        {
            var vehicle = await _fleetService.GetVehicleByIdAsync(id);
            if (vehicle == null) return NotFound();
            return Ok(vehicle);
        }

        // Vehicle Management is a protected write - only the VEHICLES department user or an
        // Admin may create/edit/delete vehicles or run a bulk upload. Previously these actions
        // carried no role restriction at all (any authenticated user, from any department, could
        // mutate the fleet), relying only on this app's global "must be logged in" fallback policy.
        [Authorize(Roles = "VEHICLES,Admin")]
        [HttpPost]
        public async Task<ActionResult<VehicleDto>> CreateVehicle([FromBody] VehicleDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var created = await _fleetService.CreateVehicleAsync(dto);
            return CreatedAtAction(nameof(GetVehicle), new { id = created.Id }, created);
        }

        [Authorize(Roles = "VEHICLES,Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateVehicle(int id, [FromBody] VehicleDto dto)
        {
            if (id != dto.Id) return BadRequest("ID mismatch");
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var success = await _fleetService.UpdateVehicleAsync(dto);
            if (!success) return NotFound();
            return NoContent();
        }

        [Authorize(Roles = "VEHICLES,Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteVehicle(int id)
        {
            var success = await _fleetService.DeleteVehicleAsync(id);
            if (!success) return NotFound();
            return NoContent();
        }

        [AllowAnonymous]
        [HttpGet("kpis")]
        public async Task<ActionResult<FleetKpisDto>> GetFleetKpis()
        {
            var kpis = await _fleetService.GetFleetKpisAsync();
            return Ok(kpis);
        }

        [Authorize(Roles = "VEHICLES,Admin")]
        [HttpPost("bulk-upload")]
        public async Task<IActionResult> BulkUpload(Microsoft.AspNetCore.Http.IFormFile file)
        {
            if (file == null || file.Length == 0) return BadRequest("No file uploaded.");
            if (!ExcelCompatibility.IsSupportedExcelFile(file.FileName, file.ContentType))
                return BadRequest("Only .xlsx or .xls files are supported.");

            using var rawStream = file.OpenReadStream();
            using var stream = ExcelCompatibility.EnsureXlsxStream(rawStream);
            var result = await _fleetService.BulkUploadVehiclesAsync(stream);

            return Ok(result);
        }
    }
}
