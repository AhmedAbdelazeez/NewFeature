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
    [AllowAnonymous]
    public class FinanceController : ControllerBase
    {
        private readonly IFinanceService _financeService;

        public FinanceController(IFinanceService financeService)
        {
            _financeService = financeService;
        }

        #region Transactions
        [HttpGet("transactions")]
        public async Task<ActionResult<IEnumerable<FinanceTransactionDto>>> GetTransactions()
        {
            var items = await _financeService.GetAllTransactionsAsync();
            return Ok(items);
        }

        [HttpGet("transactions/{id}")]
        public async Task<ActionResult<FinanceTransactionDto>> GetTransaction(int id)
        {
            var item = await _financeService.GetTransactionByIdAsync(id);
            if (item == null) return NotFound();
            return Ok(item);
        }

        [HttpPost("transactions")]
        public async Task<ActionResult<FinanceTransactionDto>> CreateTransaction([FromBody] FinanceTransactionDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var created = await _financeService.CreateTransactionAsync(dto);
            return CreatedAtAction(nameof(GetTransaction), new { id = created.Id }, created);
        }

        [HttpPut("transactions/{id}")]
        public async Task<IActionResult> UpdateTransaction(int id, [FromBody] FinanceTransactionDto dto)
        {
            if (id != dto.Id) return BadRequest("ID mismatch");
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var success = await _financeService.UpdateTransactionAsync(dto);
            if (!success) return NotFound();
            return NoContent();
        }

        [HttpDelete("transactions/{id}")]
        public async Task<IActionResult> DeleteTransaction(int id)
        {
            var success = await _financeService.DeleteTransactionAsync(id);
            if (!success) return NotFound();
            return NoContent();
        }
        #endregion

        #region Budgets
        [HttpGet("budgets")]
        public async Task<ActionResult<IEnumerable<FinanceBudgetDto>>> GetBudgets()
        {
            var items = await _financeService.GetAllBudgetsAsync();
            return Ok(items);
        }

        [HttpGet("budgets/{id}")]
        public async Task<ActionResult<FinanceBudgetDto>> GetBudget(int id)
        {
            var item = await _financeService.GetBudgetByIdAsync(id);
            if (item == null) return NotFound();
            return Ok(item);
        }

        [HttpPost("budgets")]
        public async Task<ActionResult<FinanceBudgetDto>> CreateBudget([FromBody] FinanceBudgetDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var created = await _financeService.CreateBudgetAsync(dto);
            return CreatedAtAction(nameof(GetBudget), new { id = created.Id }, created);
        }

        [HttpPut("budgets/{id}")]
        public async Task<IActionResult> UpdateBudget(int id, [FromBody] FinanceBudgetDto dto)
        {
            if (id != dto.Id) return BadRequest("ID mismatch");
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var success = await _financeService.UpdateBudgetAsync(dto);
            if (!success) return NotFound();
            return NoContent();
        }

        [HttpDelete("budgets/{id}")]
        public async Task<IActionResult> DeleteBudget(int id)
        {
            var success = await _financeService.DeleteBudgetAsync(id);
            if (!success) return NotFound();
            return NoContent();
        }
        #endregion

        #region KPIs
        [HttpGet("kpis")]
        public async Task<ActionResult<FinanceKpisDto>> GetKpis()
        {
            var kpis = await _financeService.GetFinanceKpisAsync();
            return Ok(kpis);
        }
        #endregion
    }
}
