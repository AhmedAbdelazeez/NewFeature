using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NewFeature.Models;
using NewFeature.Services;

namespace NewFeature.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Microsoft.AspNetCore.Authorization.AllowAnonymous]
    public class ProcurementController : ControllerBase
    {
        private readonly IProcurementService _procService;

        public ProcurementController(IProcurementService procService)
        {
            _procService = procService;
        }

        #region Requests
        [HttpGet("requests")]
        public async Task<IActionResult> GetRequests()
        {
            var requests = await _procService.GetAllRequestsAsync();
            return Ok(requests);
        }

        [HttpGet("requests/{id}")]
        public async Task<IActionResult> GetRequest(int id)
        {
            var req = await _procService.GetRequestByIdAsync(id);
            if (req == null) return NotFound();
            return Ok(req);
        }

        [HttpPost("requests")]
        public async Task<IActionResult> CreateRequest([FromBody] ProcurementRequestDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var created = await _procService.CreateRequestAsync(dto);
            return CreatedAtAction(nameof(GetRequest), new { id = created.Id }, created);
        }

        [HttpPut("requests/{id}")]
        public async Task<IActionResult> UpdateRequest(int id, [FromBody] ProcurementRequestDto dto)
        {
            if (id != dto.Id) return BadRequest("ID mismatch");
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var success = await _procService.UpdateRequestAsync(dto);
            if (!success) return NotFound();
            return NoContent();
        }

        [HttpDelete("requests/{id}")]
        public async Task<IActionResult> DeleteRequest(int id)
        {
            var success = await _procService.DeleteRequestAsync(id);
            if (!success) return NotFound();
            return NoContent();
        }
        #endregion

        #region Inventory
        [HttpGet("inventory")]
        public async Task<IActionResult> GetInventory()
        {
            var items = await _procService.GetAllInventoryItemsAsync();
            return Ok(items);
        }

        [HttpGet("inventory/{id}")]
        public async Task<IActionResult> GetInventoryItem(int id)
        {
            var item = await _procService.GetInventoryItemByIdAsync(id);
            if (item == null) return NotFound();
            return Ok(item);
        }

        [HttpPost("inventory")]
        public async Task<IActionResult> CreateInventoryItem([FromBody] InventoryItemDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var created = await _procService.CreateInventoryItemAsync(dto);
            return CreatedAtAction(nameof(GetInventoryItem), new { id = created.Id }, created);
        }

        [HttpPut("inventory/{id}")]
        public async Task<IActionResult> UpdateInventoryItem(int id, [FromBody] InventoryItemDto dto)
        {
            if (id != dto.Id) return BadRequest("ID mismatch");
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var success = await _procService.UpdateInventoryItemAsync(dto);
            if (!success) return NotFound();
            return NoContent();
        }

        [HttpDelete("inventory/{id}")]
        public async Task<IActionResult> DeleteInventoryItem(int id)
        {
            var success = await _procService.DeleteInventoryItemAsync(id);
            if (!success) return NotFound();
            return NoContent();
        }
        #endregion

        #region KPIs
        [HttpGet("kpis")]
        public async Task<IActionResult> GetKpis()
        {
            var kpis = await _procService.GetProcurementKpisAsync();
            return Ok(kpis);
        }
        #endregion
    }
}
