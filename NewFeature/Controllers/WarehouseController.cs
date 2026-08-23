using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NewFeature.Models;
using NewFeature.Services;

namespace NewFeature.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [AllowAnonymous]
    public class WarehouseController : ControllerBase
    {
        private readonly IWarehouseService _warehouseService;

        public WarehouseController(IWarehouseService warehouseService)
        {
            _warehouseService = warehouseService;
        }

        [HttpGet("items")]
        public async Task<ActionResult<PagedResultDto<InventoryItemDto>>> GetItems(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null)
        {
            var result = await _warehouseService.GetInventoryItemsAsync(page, pageSize, fromDate, toDate);
            return Ok(result);
        }

        [HttpGet("items/{id}")]
        public async Task<ActionResult<InventoryItemDto>> GetItem(int id)
        {
            var item = await _warehouseService.GetInventoryItemByIdAsync(id);
            if (item == null) return NotFound();
            return Ok(item);
        }

        [HttpPost("items")]
        public async Task<ActionResult<InventoryItemDto>> CreateItem([FromBody] InventoryItemDto dto)
        {
            var created = await _warehouseService.CreateInventoryItemAsync(dto);
            return CreatedAtAction(nameof(GetItem), new { id = created.Id }, created);
        }

        [HttpPut("items/{id}")]
        public async Task<IActionResult> UpdateItem(int id, [FromBody] InventoryItemDto dto)
        {
            if (id != dto.Id) return BadRequest("ID mismatch");
            var success = await _warehouseService.UpdateInventoryItemAsync(dto);
            if (!success) return NotFound();
            return NoContent();
        }

        [HttpDelete("items/{id}")]
        public async Task<IActionResult> DeleteItem(int id)
        {
            var success = await _warehouseService.DeleteInventoryItemAsync(id);
            if (!success) return NotFound();
            return NoContent();
        }

        [HttpGet("kpis")]
        public async Task<ActionResult<WarehouseKpisDto>> GetKpis()
        {
            var kpis = await _warehouseService.GetWarehouseKpisAsync();
            return Ok(kpis);
        }

        [HttpPost("bulk-upload")]
        public async Task<IActionResult> BulkUpload(IFormFile file)
        {
            if (file == null || file.Length == 0) return BadRequest("No file uploaded.");
            if (!ExcelCompatibility.IsSupportedExcelFile(file.FileName, file.ContentType))
                return BadRequest("Only .xlsx or .xls files are supported.");

            using var rawStream = file.OpenReadStream();
            using var stream = ExcelCompatibility.EnsureXlsxStream(rawStream);
            var result = await _warehouseService.BulkUploadInventoryAsync(stream);

            return Ok(new { successCount = result.SuccessCount, errors = result.Errors });
        }
    }
}
