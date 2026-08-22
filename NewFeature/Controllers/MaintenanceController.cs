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
    public class MaintenanceController : ControllerBase
    {
        private readonly IMaintenanceService _maintenanceService;

        public MaintenanceController(IMaintenanceService maintenanceService)
        {
            _maintenanceService = maintenanceService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MaintenanceWorkOrderDto>>> GetWorkOrders()
        {
            var orders = await _maintenanceService.GetAllWorkOrdersAsync();
            return Ok(orders);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<MaintenanceWorkOrderDto>> GetWorkOrder(int id)
        {
            var order = await _maintenanceService.GetWorkOrderByIdAsync(id);
            if (order == null) return NotFound();
            return Ok(order);
        }

        [HttpPost]
        public async Task<ActionResult<MaintenanceWorkOrderDto>> CreateWorkOrder([FromBody] MaintenanceWorkOrderDto dto)
        {
            var created = await _maintenanceService.CreateWorkOrderAsync(dto);
            return CreatedAtAction(nameof(GetWorkOrder), new { id = created.Id }, created);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateWorkOrder(int id, [FromBody] MaintenanceWorkOrderDto dto)
        {
            if (id != dto.Id) return BadRequest("ID mismatch");
            var success = await _maintenanceService.UpdateWorkOrderAsync(dto);
            if (!success) return NotFound();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteWorkOrder(int id)
        {
            var success = await _maintenanceService.DeleteWorkOrderAsync(id);
            if (!success) return NotFound();
            return NoContent();
        }

        [HttpGet("kpis")]
        public async Task<ActionResult<MaintenanceKpisDto>> GetMaintenanceKpis()
        {
            var kpis = await _maintenanceService.GetMaintenanceKpisAsync();
            return Ok(kpis);
        }

        [HttpPost("bulk-upload")]
        public async Task<IActionResult> BulkUpload(Microsoft.AspNetCore.Http.IFormFile file, [FromQuery] string branchName = "الورشة المركزية")
        {
            if (file == null || file.Length == 0) return BadRequest("No file uploaded.");
            if (!file.FileName.EndsWith(".xlsx", System.StringComparison.OrdinalIgnoreCase))
                return BadRequest("Only .xlsx files are supported.");

            using var stream = file.OpenReadStream();
            var result = await _maintenanceService.BulkUploadWorkshopLogsAsync(stream, branchName);
            
            return Ok(new { successCount = result.SuccessCount, errors = result.Errors });
        }
    }
}
