using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NewFeature.Models;
using NewFeature.Services.Repositories;

namespace NewFeature.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MilestonesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public MilestonesController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProjectMilestone>>> GetMilestones()
        {
            return await _context.ProjectMilestones.ToListAsync();
        }

        // Paginated + searchable listing used by the Project Management page's per-project
        // Milestones tab. GetMilestones() above is left untouched since it returns a plain array
        // other callers may still expect. projectId lets the tab keep scoping to the currently open
        // project. This controller has no dedicated service layer (it talks to the DbContext
        // directly), so paging/search/order is done in-memory here, matching
        // FleetService.GetVehiclesPagedAsync's approach elsewhere in this codebase.
        [HttpGet("paged")]
        public async Task<ActionResult<PagedResultDto<ProjectMilestone>>> GetMilestonesPaged(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? search = null,
            [FromQuery] int? projectId = null)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 20;
            if (pageSize > 200) pageSize = 200;

            var milestones = (await _context.ProjectMilestones.ToListAsync()).AsEnumerable();

            if (projectId.HasValue)
            {
                milestones = milestones.Where(m => m.ProjectId == projectId.Value);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                milestones = milestones.Where(m =>
                    Contains(m.TitleEn, term) ||
                    Contains(m.TitleAr, term) ||
                    Contains(m.DescriptionEn, term) ||
                    Contains(m.DescriptionAr, term));
            }

            var ordered = milestones.OrderByDescending(m => m.Id).ToList();
            var totalCount = ordered.Count;
            var items = ordered.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            return Ok(new PagedResultDto<ProjectMilestone>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            });
        }

        private static bool Contains(string? haystack, string term) =>
            !string.IsNullOrEmpty(haystack) && haystack.IndexOf(term, System.StringComparison.OrdinalIgnoreCase) >= 0;

        [HttpGet("{id}")]
        public async Task<ActionResult<ProjectMilestone>> GetMilestone(int id)
        {
            var milestone = await _context.ProjectMilestones.FindAsync(id);
            if (milestone == null) return NotFound();
            return Ok(milestone);
        }

        [HttpPost]
        public async Task<ActionResult<ProjectMilestone>> CreateMilestone([FromBody] ProjectMilestone milestone)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            
            _context.ProjectMilestones.Add(milestone);
            await _context.SaveChangesAsync();
            
            return CreatedAtAction(nameof(GetMilestone), new { id = milestone.Id }, milestone);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateMilestone(int id, [FromBody] ProjectMilestone milestone)
        {
            if (id != milestone.Id) return BadRequest("ID mismatch");
            if (!ModelState.IsValid) return BadRequest(ModelState);

            _context.Entry(milestone).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.ProjectMilestones.Any(e => e.Id == id)) return NotFound();
                throw;
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMilestone(int id)
        {
            var milestone = await _context.ProjectMilestones.FindAsync(id);
            if (milestone == null) return NotFound();

            _context.ProjectMilestones.Remove(milestone);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
