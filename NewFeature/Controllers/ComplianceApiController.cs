using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using NewFeature.Models;
using NewFeature.Services;

namespace NewFeature.Controllers
{
    [ApiController]
    [Route("api/compliance")]
    [AllowAnonymous]
    public class ComplianceApiController : ControllerBase
    {
        private readonly IComplianceService _complianceService;

        public ComplianceApiController(IComplianceService complianceService)
        {
            _complianceService = complianceService;
        }

        #region Dashboard KPIs
        [HttpGet("kpis")]
        public async Task<ActionResult<ComplianceKpisDto>> GetComplianceKpis()
        {
            var kpis = await _complianceService.GetComplianceKpisAsync();
            return Ok(kpis);
        }
        #endregion

        #region Departments
        [HttpGet("departments")]
        public async Task<ActionResult<IEnumerable<DepartmentDto>>> GetDepartments()
        {
            var result = await _complianceService.GetAllDepartmentsAsync();
            return Ok(result);
        }

        [HttpGet("departments/{id}")]
        public async Task<ActionResult<DepartmentDto>> GetDepartment(int id)
        {
            var result = await _complianceService.GetDepartmentByIdAsync(id);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpPost("departments")]
        public async Task<ActionResult<DepartmentDto>> CreateDepartment([FromBody] DepartmentDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var created = await _complianceService.CreateDepartmentAsync(dto);
            return CreatedAtAction(nameof(GetDepartment), new { id = created.Id }, created);
        }

        [HttpPut("departments/{id}")]
        public async Task<IActionResult> UpdateDepartment(int id, [FromBody] DepartmentDto dto)
        {
            if (id != dto.Id) return BadRequest("ID mismatch");
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var success = await _complianceService.UpdateDepartmentAsync(dto);
            if (!success) return NotFound();
            return NoContent();
        }

        [HttpDelete("departments/{id}")]
        public async Task<IActionResult> DeleteDepartment(int id)
        {
            var success = await _complianceService.DeleteDepartmentAsync(id);
            if (!success) return NotFound();
            return NoContent();
        }
        #endregion

        #region Classifications
        [HttpGet("classifications")]
        public async Task<ActionResult<IEnumerable<ViolationClassificationDto>>> GetClassifications()
        {
            var result = await _complianceService.GetAllClassificationsAsync();
            return Ok(result);
        }

        [HttpGet("classifications/{id}")]
        public async Task<ActionResult<ViolationClassificationDto>> GetClassification(int id)
        {
            var result = await _complianceService.GetClassificationByIdAsync(id);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpPost("classifications")]
        public async Task<ActionResult<ViolationClassificationDto>> CreateClassification([FromBody] ViolationClassificationDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var created = await _complianceService.CreateClassificationAsync(dto);
            return CreatedAtAction(nameof(GetClassification), new { id = created.Id }, created);
        }

        [HttpPut("classifications/{id}")]
        public async Task<IActionResult> UpdateClassification(int id, [FromBody] ViolationClassificationDto dto)
        {
            if (id != dto.Id) return BadRequest("ID mismatch");
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var success = await _complianceService.UpdateClassificationAsync(dto);
            if (!success) return NotFound();
            return NoContent();
        }

        [HttpDelete("classifications/{id}")]
        public async Task<IActionResult> DeleteClassification(int id)
        {
            var success = await _complianceService.DeleteClassificationAsync(id);
            if (!success) return NotFound();
            return NoContent();
        }
        #endregion

        #region Violations
        [HttpGet("violations")]
        public async Task<ActionResult<IEnumerable<ViolationDto>>> GetViolations()
        {
            var result = await _complianceService.GetAllViolationsAsync();
            return Ok(result);
        }

        [HttpGet("violations/{id}")]
        public async Task<ActionResult<ViolationDto>> GetViolation(int id)
        {
            var result = await _complianceService.GetViolationByIdAsync(id);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpPost("violations")]
        public async Task<ActionResult<ViolationDto>> CreateViolation([FromBody] ViolationDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var created = await _complianceService.CreateViolationAsync(dto);
            return CreatedAtAction(nameof(GetViolation), new { id = created.Id }, created);
        }

        [HttpPut("violations/{id}")]
        public async Task<IActionResult> UpdateViolation(int id, [FromBody] ViolationDto dto)
        {
            if (id != dto.Id) return BadRequest("ID mismatch");
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var success = await _complianceService.UpdateViolationAsync(dto);
            if (!success) return NotFound();
            return NoContent();
        }

        [HttpDelete("violations/{id}")]
        public async Task<IActionResult> DeleteViolation(int id)
        {
            var success = await _complianceService.DeleteViolationAsync(id);
            if (!success) return NotFound();
            return NoContent();
        }
        #endregion

        #region Internal Audits
        [HttpGet("audits")]
        public async Task<ActionResult<IEnumerable<InternalAuditDto>>> GetAudits()
        {
            var result = await _complianceService.GetAllAuditsAsync();
            return Ok(result);
        }

        [HttpGet("audits/{id}")]
        public async Task<ActionResult<InternalAuditDto>> GetAudit(int id)
        {
            var result = await _complianceService.GetAuditByIdAsync(id);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpPost("audits")]
        public async Task<ActionResult<InternalAuditDto>> CreateAudit([FromBody] InternalAuditDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var created = await _complianceService.CreateAuditAsync(dto);
            return CreatedAtAction(nameof(GetAudit), new { id = created.Id }, created);
        }

        [HttpPut("audits/{id}")]
        public async Task<IActionResult> UpdateAudit(int id, [FromBody] InternalAuditDto dto)
        {
            if (id != dto.Id) return BadRequest("ID mismatch");
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var success = await _complianceService.UpdateAuditAsync(dto);
            if (!success) return NotFound();
            return NoContent();
        }

        [HttpDelete("audits/{id}")]
        public async Task<IActionResult> DeleteAudit(int id)
        {
            var success = await _complianceService.DeleteAuditAsync(id);
            if (!success) return NotFound();
            return NoContent();
        }
        #endregion

        #region Improvement Actions
        [HttpGet("improvements")]
        public async Task<ActionResult<IEnumerable<ImprovementActionDto>>> GetImprovements()
        {
            var result = await _complianceService.GetAllImprovementsAsync();
            return Ok(result);
        }

        [HttpGet("improvements/{id}")]
        public async Task<ActionResult<ImprovementActionDto>> GetImprovement(int id)
        {
            var result = await _complianceService.GetImprovementByIdAsync(id);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpPost("improvements")]
        public async Task<ActionResult<ImprovementActionDto>> CreateImprovement([FromBody] ImprovementActionDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var created = await _complianceService.CreateImprovementAsync(dto);
            return CreatedAtAction(nameof(GetImprovement), new { id = created.Id }, created);
        }

        [HttpPut("improvements/{id}")]
        public async Task<IActionResult> UpdateImprovement(int id, [FromBody] ImprovementActionDto dto)
        {
            if (id != dto.Id) return BadRequest("ID mismatch");
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var success = await _complianceService.UpdateImprovementAsync(dto);
            if (!success) return NotFound();
            return NoContent();
        }

        [HttpDelete("improvements/{id}")]
        public async Task<IActionResult> DeleteImprovement(int id)
        {
            var success = await _complianceService.DeleteImprovementAsync(id);
            if (!success) return NotFound();
            return NoContent();
        }
        #endregion
    }
}
