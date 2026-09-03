using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using NewFeature.Models;
using NewFeature.Services;

namespace NewFeature.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TripsController : ControllerBase
    {
        private readonly IFleetService _fleetService;

        public TripsController(IFleetService fleetService)
        {
            _fleetService = fleetService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TripDto>>> GetTrips()
        {
            var trips = await _fleetService.GetAllTripsAsync();
            return Ok(trips);
        }

        [HttpGet("paged")]
        public async Task<ActionResult<PagedResultDto<TripDto>>> GetTripsPaged(
            [FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? search = null)
        {
            var result = await _fleetService.GetTripsPagedAsync(page, pageSize, search);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TripDto>> GetTrip(int id)
        {
            var trip = await _fleetService.GetTripByIdAsync(id);
            if (trip == null) return NotFound();
            return Ok(trip);
        }

        [HttpPost]
        public async Task<ActionResult<TripDto>> CreateTrip([FromBody] TripDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var created = await _fleetService.CreateTripAsync(dto);
            return CreatedAtAction(nameof(GetTrip), new { id = created.Id }, created);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTrip(int id, [FromBody] TripDto dto)
        {
            if (id != dto.Id) return BadRequest("ID mismatch");
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var success = await _fleetService.UpdateTripAsync(dto);
            if (!success) return NotFound();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTrip(int id)
        {
            var success = await _fleetService.DeleteTripAsync(id);
            if (!success) return NotFound();
            return NoContent();
        }

        [HttpPost("bulk-upload")]
        public async Task<IActionResult> BulkUpload(Microsoft.AspNetCore.Http.IFormFile file)
        {
            if (file == null || file.Length == 0) return BadRequest("No file uploaded.");
            if (!ExcelCompatibility.IsSupportedExcelFile(file.FileName, file.ContentType))
                return BadRequest("Only .xlsx or .xls files are supported.");

            using var rawStream = file.OpenReadStream();
            using var stream = ExcelCompatibility.EnsureXlsxStream(rawStream);
            var result = await _fleetService.BulkUploadTripsAsync(stream);

            return Ok(new { successCount = result.SuccessCount, errors = result.Errors });
        }
    }
}
