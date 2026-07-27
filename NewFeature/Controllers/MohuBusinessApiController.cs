using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using NewFeature.Models;
using NewFeature.Services;

namespace NewFeature.Controllers
{
    [ApiController]
    [Route("api/mohu-business")]
    [AllowAnonymous]
    public class MohuBusinessApiController : ControllerBase
    {
        private readonly IMohuStandardsService _service;

        public MohuBusinessApiController(IMohuStandardsService service)
        {
            _service = service;
        }

        // Pilgrim Groups
        [HttpGet("groups")]
        public async Task<ActionResult<List<MohuPilgrimGroupDto>>> GetGroups()
        {
            return Ok(await _service.GetAllPilgrimGroupsAsync());
        }

        [HttpPost("groups")]
        public async Task<ActionResult<MohuPilgrimGroupDto>> CreateGroup([FromBody] MohuPilgrimGroupDto dto)
        {
            var created = await _service.CreatePilgrimGroupAsync(dto);
            return Ok(created);
        }

        [HttpPut("groups/{id}")]
        public async Task<IActionResult> UpdateGroup(int id, [FromBody] MohuPilgrimGroupDto dto)
        {
            var updated = await _service.UpdatePilgrimGroupAsync(id, dto);
            if (!updated) return NotFound();
            return Ok(new { message = "Updated successfully." });
        }

        [HttpDelete("groups/{id}")]
        public async Task<IActionResult> DeleteGroup(int id)
        {
            await _service.DeletePilgrimGroupAsync(id);
            return Ok(new { message = "Deleted successfully." });
        }

        // Feedbacks
        [HttpGet("feedbacks")]
        public async Task<ActionResult<List<MohuFeedbackDto>>> GetFeedbacks()
        {
            return Ok(await _service.GetAllFeedbacksAsync());
        }

        [HttpPost("feedbacks")]
        public async Task<ActionResult<MohuFeedbackDto>> CreateFeedback([FromBody] MohuFeedbackDto dto)
        {
            var created = await _service.CreateFeedbackAsync(dto);
            return Ok(created);
        }

        [HttpPut("feedbacks/{id}")]
        public async Task<IActionResult> UpdateFeedback(int id, [FromBody] MohuFeedbackDto dto)
        {
            var updated = await _service.UpdateFeedbackAsync(id, dto);
            if (!updated) return NotFound();
            return Ok(new { message = "Updated successfully." });
        }

        [HttpDelete("feedbacks/{id}")]
        public async Task<IActionResult> DeleteFeedback(int id)
        {
            await _service.DeleteFeedbackAsync(id);
            return Ok(new { message = "Deleted successfully." });
        }

        // Violations
        [HttpGet("violations")]
        public async Task<ActionResult<List<MohuViolationRecordDto>>> GetViolations()
        {
            return Ok(await _service.GetAllViolationsAsync());
        }

        [HttpPost("violations")]
        public async Task<ActionResult<MohuViolationRecordDto>> CreateViolation([FromBody] MohuViolationRecordDto dto)
        {
            var created = await _service.CreateViolationAsync(dto);
            return Ok(created);
        }

        [HttpPut("violations/{id}")]
        public async Task<IActionResult> UpdateViolation(int id, [FromBody] MohuViolationRecordDto dto)
        {
            var updated = await _service.UpdateViolationAsync(id, dto);
            if (!updated) return NotFound();
            return Ok(new { message = "Updated successfully." });
        }

        [HttpDelete("violations/{id}")]
        public async Task<IActionResult> DeleteViolation(int id)
        {
            await _service.DeleteViolationAsync(id);
            return Ok(new { message = "Deleted successfully." });
        }

        // Permits
        [HttpGet("permits")]
        public async Task<ActionResult<List<MohuPermitLogDto>>> GetPermits()
        {
            return Ok(await _service.GetAllPermitsAsync());
        }

        [HttpPost("permits")]
        public async Task<ActionResult<MohuPermitLogDto>> CreatePermit([FromBody] MohuPermitLogDto dto)
        {
            var created = await _service.CreatePermitAsync(dto);
            return Ok(created);
        }

        [HttpPut("permits/{id}")]
        public async Task<IActionResult> UpdatePermit(int id, [FromBody] MohuPermitLogDto dto)
        {
            var updated = await _service.UpdatePermitAsync(id, dto);
            if (!updated) return NotFound();
            return Ok(new { message = "Updated successfully." });
        }

        [HttpDelete("permits/{id}")]
        public async Task<IActionResult> DeletePermit(int id)
        {
            await _service.DeletePermitAsync(id);
            return Ok(new { message = "Deleted successfully." });
        }

        // Seed Data
        [HttpPost("seed")]
        public async Task<IActionResult> SeedData()
        {
            // Clear existing data first
            var existingGroups = await _service.GetAllPilgrimGroupsAsync();
            foreach (var g in existingGroups) await _service.DeletePilgrimGroupAsync(g.Id);

            var existingFeedbacks = await _service.GetAllFeedbacksAsync();
            foreach (var f in existingFeedbacks) await _service.DeleteFeedbackAsync(f.Id);

            var existingViolations = await _service.GetAllViolationsAsync();
            foreach (var v in existingViolations) await _service.DeleteViolationAsync(v.Id);

            var existingPermits = await _service.GetAllPermitsAsync();
            foreach (var p in existingPermits) await _service.DeletePermitAsync(p.Id);

            // Seed 5 Groups
            for (int i = 1; i <= 5; i++)
            {
                var group = await _service.CreatePilgrimGroupAsync(new MohuPilgrimGroupDto
                {
                    GroupNumber = $"GRP-2026-{1000 + i}",
                    Nationality = i % 2 == 0 ? "إندونيسي" : "باكستاني",
                    AgeGroup = i % 2 == 0 ? "بالغين" : "كبار السن",
                    PackageCategory = i % 2 == 0 ? "مميزة (VIP)" : "اقتصادية",
                    PackagePrice = 5000 + (i * 1000),
                    IsNewPilgrim = true,
                    ArrivalDate = System.DateTime.UtcNow.AddDays(-i),
                    ArrivalPort = "مطار الملك عبدالعزيز",
                    PilgrimCount = 45
                });

                // Seed Feedback for each group
                await _service.CreateFeedbackAsync(new MohuFeedbackDto
                {
                    MohuPilgrimGroupId = group.Id,
                    Rating = i,
                    ServiceType = i % 2 == 0 ? "سكن" : "نقل",
                    HasComplaint = i < 3,
                    ComplaintDetails = i < 3 ? "تأخير في استلام الغرف" : "",
                    WaitingTimeMinutes = 15 * i,
                    FeedbackDate = System.DateTime.UtcNow.AddDays(-i + 1)
                });
            }

            // Seed 5 Violations
            for (int i = 1; i <= 5; i++)
            {
                await _service.CreateViolationAsync(new MohuViolationRecordDto
                {
                    ViolationType = i % 2 == 0 ? "تأخير التفويج" : "غياب المرشد",
                    PenaltyAmount = 1000 * i,
                    IsClosed = i % 2 == 0,
                    AffectedPilgrimsCount = 45,
                    CommitteeEvaluationScore = 80 + i,
                    InspectionDate = System.DateTime.UtcNow.AddDays(-i)
                });
            }

            // Seed 5 Permits
            for (int i = 1; i <= 5; i++)
            {
                await _service.CreatePermitAsync(new MohuPermitLogDto
                {
                    PermitNumber = $"PRM-MOHU-{9000 + i}",
                    IsEntryPortMatched = true,
                    IsEntryDateMatched = i % 2 != 0,
                    IsHousingMatched = true,
                    VerificationDate = System.DateTime.UtcNow.AddDays(-i)
                });
            }

            return Ok(new { message = "Seeded 5 Arabic records successfully for all entities." });
        }
    }
}
