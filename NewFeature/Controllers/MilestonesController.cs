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
