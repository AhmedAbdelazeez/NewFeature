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
    [AllowAnonymous] // allowing anonymous access to simplify dashboard integration (same as DashboardApiController)
    public class EmployeesController : ControllerBase
    {
        private readonly IEmployeeService _employeeService;

        public EmployeesController(IEmployeeService employeeService)
        {
            _employeeService = employeeService;
        }

        #region Employee endpoints
        [HttpGet]
        public async Task<ActionResult<IEnumerable<EmployeeDto>>> GetEmployees()
        {
            var employees = await _employeeService.GetAllEmployeesAsync();
            return Ok(employees);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<EmployeeDto>> GetEmployee(int id)
        {
            var employee = await _employeeService.GetEmployeeByIdAsync(id);
            if (employee == null) return NotFound();
            return Ok(employee);
        }

        [HttpGet("kpis")]
        public async Task<ActionResult<HrKpisDto>> GetKpis()
        {
            var kpis = await _employeeService.GetHrKpisAsync();
            return Ok(kpis);
        }

        [HttpPost]
        public async Task<ActionResult<EmployeeDto>> CreateEmployee([FromBody] EmployeeDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var created = await _employeeService.CreateEmployeeAsync(dto);
            return CreatedAtAction(nameof(GetEmployee), new { id = created.Id }, created);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEmployee(int id, [FromBody] EmployeeDto dto)
        {
            if (id != dto.Id) return BadRequest("ID mismatch");
            if (!ModelState.IsValid) return BadRequest(ModelState);
            
            var success = await _employeeService.UpdateEmployeeAsync(dto);
            if (!success) return NotFound();
            
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEmployee(int id)
        {
            var success = await _employeeService.DeleteEmployeeAsync(id);
            if (!success) return NotFound();
            
            return NoContent();
        }
        #endregion

        #region Evaluation endpoints
        [HttpGet("evaluations")]
        public async Task<ActionResult<IEnumerable<EmployeeEvaluationDto>>> GetEvaluations()
        {
            var evaluations = await _employeeService.GetAllEvaluationsAsync();
            return Ok(evaluations);
        }

        [HttpGet("evaluations/{id}")]
        public async Task<ActionResult<EmployeeEvaluationDto>> GetEvaluation(int id)
        {
            var evaluation = await _employeeService.GetEvaluationByIdAsync(id);
            if (evaluation == null) return NotFound();
            return Ok(evaluation);
        }

        [HttpPost("evaluations")]
        public async Task<ActionResult<EmployeeEvaluationDto>> CreateEvaluation([FromBody] EmployeeEvaluationDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var created = await _employeeService.CreateEvaluationAsync(dto);
            return CreatedAtAction(nameof(GetEvaluation), new { id = created.Id }, created);
        }

        [HttpPut("evaluations/{id}")]
        public async Task<IActionResult> UpdateEvaluation(int id, [FromBody] EmployeeEvaluationDto dto)
        {
            if (id != dto.Id) return BadRequest("ID mismatch");
            if (!ModelState.IsValid) return BadRequest(ModelState);
            
            var success = await _employeeService.UpdateEvaluationAsync(dto);
            if (!success) return NotFound();
            
            return NoContent();
        }

        [HttpDelete("evaluations/{id}")]
        public async Task<IActionResult> DeleteEvaluation(int id)
        {
            var success = await _employeeService.DeleteEvaluationAsync(id);
            if (!success) return NotFound();
            
            return NoContent();
        }
        #endregion
    }
}
