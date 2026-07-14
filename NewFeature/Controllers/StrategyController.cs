using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NewFeature.Models;
using NewFeature.Services;

namespace NewFeature.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Microsoft.AspNetCore.Authorization.AllowAnonymous]
    public class StrategyController : ControllerBase
    {
        private readonly IStrategyService _stratService;

        public StrategyController(IStrategyService stratService)
        {
            _stratService = stratService;
        }

        #region Goals
        [HttpGet("goals")]
        public async Task<IActionResult> GetGoals()
        {
            var goals = await _stratService.GetAllGoalsAsync();
            return Ok(goals);
        }

        [HttpGet("goals/{id}")]
        public async Task<IActionResult> GetGoal(int id)
        {
            var goal = await _stratService.GetGoalByIdAsync(id);
            if (goal == null) return NotFound();
            return Ok(goal);
        }

        [HttpPost("goals")]
        public async Task<IActionResult> CreateGoal([FromBody] StrategicGoalDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var created = await _stratService.CreateGoalAsync(dto);
            return CreatedAtAction(nameof(GetGoal), new { id = created.Id }, created);
        }

        [HttpPut("goals/{id}")]
        public async Task<IActionResult> UpdateGoal(int id, [FromBody] StrategicGoalDto dto)
        {
            if (id != dto.Id) return BadRequest("ID mismatch");
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var success = await _stratService.UpdateGoalAsync(dto);
            if (!success) return NotFound();
            return NoContent();
        }

        [HttpDelete("goals/{id}")]
        public async Task<IActionResult> DeleteGoal(int id)
        {
            var success = await _stratService.DeleteGoalAsync(id);
            if (!success) return NotFound();
            return NoContent();
        }
        #endregion

        #region PMO Initiatives
        [HttpGet("initiatives")]
        public async Task<IActionResult> GetInitiatives()
        {
            var initiatives = await _stratService.GetAllInitiativesAsync();
            return Ok(initiatives);
        }

        [HttpGet("initiatives/{id}")]
        public async Task<IActionResult> GetInitiative(int id)
        {
            var init = await _stratService.GetInitiativeByIdAsync(id);
            if (init == null) return NotFound();
            return Ok(init);
        }

        [HttpPost("initiatives")]
        public async Task<IActionResult> CreateInitiative([FromBody] PmoInitiativeDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var created = await _stratService.CreateInitiativeAsync(dto);
            return CreatedAtAction(nameof(GetInitiative), new { id = created.Id }, created);
        }

        [HttpPut("initiatives/{id}")]
        public async Task<IActionResult> UpdateInitiative(int id, [FromBody] PmoInitiativeDto dto)
        {
            if (id != dto.Id) return BadRequest("ID mismatch");
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var success = await _stratService.UpdateInitiativeAsync(dto);
            if (!success) return NotFound();
            return NoContent();
        }

        [HttpDelete("initiatives/{id}")]
        public async Task<IActionResult> DeleteInitiative(int id)
        {
            var success = await _stratService.DeleteInitiativeAsync(id);
            if (!success) return NotFound();
            return NoContent();
        }
        #endregion

        #region KPIs
        [HttpGet("kpis")]
        public async Task<IActionResult> GetKpis()
        {
            var kpis = await _stratService.GetStrategyKpisAsync();
            return Ok(kpis);
        }
        #endregion
    }
}
