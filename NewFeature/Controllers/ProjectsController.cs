using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using NewFeature.Models;
using NewFeature.Services;

namespace NewFeature.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProjectsController : ControllerBase
    {
        private readonly IProjectService _projectService;

        public ProjectsController(IProjectService projectService)
        {
            _projectService = projectService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProjectDto>>> GetProjects()
        {
            var projects = await _projectService.GetAllProjectsAsync();
            return Ok(projects);
        }

        // Paginated + searchable listing used by the Project Management page's table. GetProjects()
        // above is left untouched since it returns a plain array other callers may still expect.
        [HttpGet("paged")]
        public async Task<ActionResult<PagedResultDto<ProjectDto>>> GetProjectsPaged(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? search = null)
        {
            var result = await _projectService.GetProjectsPagedAsync(page, pageSize, search);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ProjectDto>> GetProject(int id)
        {
            var project = await _projectService.GetProjectByIdAsync(id);
            if (project == null) return NotFound();
            return Ok(project);
        }

        [HttpGet("{id}/details")]
        public async Task<ActionResult<ProjectDetailsDto>> GetProjectDetails(int id)
        {
            var details = await _projectService.GetProjectDetailsAsync(id);
            if (details == null) return NotFound();
            return Ok(details);
        }

        [HttpPost]
        public async Task<ActionResult<ProjectDto>> CreateProject([FromBody] ProjectDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var created = await _projectService.CreateProjectAsync(dto);
            return CreatedAtAction(nameof(GetProject), new { id = created.Id }, created);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProject(int id, [FromBody] ProjectDto dto)
        {
            if (id != dto.Id) return BadRequest("ID mismatch");
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var success = await _projectService.UpdateProjectAsync(dto);
            if (!success) return NotFound();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProject(int id)
        {
            var success = await _projectService.DeleteProjectAsync(id);
            if (!success) return NotFound();
            return NoContent();
        }

        // Bulk upload of the approved single-sheet Projects register template.
        [HttpPost("bulk-upload")]
        public async Task<IActionResult> BulkUpload(Microsoft.AspNetCore.Http.IFormFile file)
        {
            if (file == null || file.Length == 0) return BadRequest("No file uploaded.");
            if (!ExcelCompatibility.IsSupportedExcelFile(file.FileName, file.ContentType))
                return BadRequest("Only .xlsx or .xls files are supported.");

            using var rawStream = file.OpenReadStream();
            using var stream = ExcelCompatibility.EnsureXlsxStream(rawStream);
            var result = await _projectService.BulkUploadProjectsAsync(stream);

            // A template/header problem is the uploader's mistake, not a server fault - report it
            // as 422 so the page can surface the message instead of a generic failure.
            if (!result.Success) return UnprocessableEntity(result);
            return Ok(result);
        }
    }
}
