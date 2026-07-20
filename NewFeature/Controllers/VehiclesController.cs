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

        [HttpGet]
        public async Task<ActionResult<IEnumerable<VehicleDto>>> GetVehicles()
        {
            var vehicles = await _fleetService.GetAllVehiclesAsync();
            return Ok(vehicles);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<VehicleDto>> GetVehicle(int id)
        {
            var vehicle = await _fleetService.GetVehicleByIdAsync(id);
            if (vehicle == null) return NotFound();
            return Ok(vehicle);
        }

        [HttpPost]
        public async Task<ActionResult<VehicleDto>> CreateVehicle([FromBody] VehicleDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var created = await _fleetService.CreateVehicleAsync(dto);
            return CreatedAtAction(nameof(GetVehicle), new { id = created.Id }, created);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateVehicle(int id, [FromBody] VehicleDto dto)
        {
            if (id != dto.Id) return BadRequest("ID mismatch");
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var success = await _fleetService.UpdateVehicleAsync(dto);
            if (!success) return NotFound();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteVehicle(int id)
        {
            var success = await _fleetService.DeleteVehicleAsync(id);
            if (!success) return NotFound();
            return NoContent();
        }

        [HttpPost("bulk-upload")]
        public async Task<IActionResult> BulkUpload(Microsoft.AspNetCore.Http.IFormFile file)
        {
            if (file == null || file.Length == 0) return BadRequest("No file uploaded.");
            if (!file.FileName.EndsWith(".xlsx", System.StringComparison.OrdinalIgnoreCase))
                return BadRequest("Only .xlsx files are supported.");

            using var stream = file.OpenReadStream();
            var result = await _fleetService.BulkUploadVehiclesAsync(stream);
            
            return Ok(new { successCount = result.SuccessCount, errors = result.Errors });
        }
    }
}
