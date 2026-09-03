using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
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

        // Paged: the underlying IMohuStandardsService only exposes GetAllPilgrimGroupsAsync()
        // (no IQueryable/paged overload, and that shared service is owned by another feature),
        // so - matching the established house pattern (see FleetService.GetVehiclesPagedAsync) -
        // we materialize once here and search/order/page in memory, keeping this change scoped
        // to this controller instead of touching the shared standards service.
        [HttpGet("groups/paged")]
        public async Task<ActionResult<PagedResultDto<MohuPilgrimGroupDto>>> GetGroupsPaged(
            [FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? search = null)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 20;
            if (pageSize > 200) pageSize = 200;

            var groups = (await _service.GetAllPilgrimGroupsAsync()).AsEnumerable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                groups = groups.Where(g =>
                    Contains(g.GroupNumber, term) ||
                    Contains(g.Nationality, term) ||
                    Contains(g.AgeGroup, term) ||
                    Contains(g.PackageCategory, term) ||
                    Contains(g.ArrivalPort, term));
            }

            var ordered = groups.OrderByDescending(g => g.Id).ToList();
            var totalCount = ordered.Count;
            var items = ordered.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            return Ok(new PagedResultDto<MohuPilgrimGroupDto>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            });
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

        [HttpGet("feedbacks/paged")]
        public async Task<ActionResult<PagedResultDto<MohuFeedbackDto>>> GetFeedbacksPaged(
            [FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? search = null)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 20;
            if (pageSize > 200) pageSize = 200;

            var feedbacks = (await _service.GetAllFeedbacksAsync()).AsEnumerable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                feedbacks = feedbacks.Where(f =>
                    Contains(f.ServiceType, term) ||
                    Contains(f.ComplaintDetails, term) ||
                    Contains(f.MohuPilgrimGroupId.ToString(), term) ||
                    Contains(f.Rating.ToString(), term));
            }

            var ordered = feedbacks.OrderByDescending(f => f.Id).ToList();
            var totalCount = ordered.Count;
            var items = ordered.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            return Ok(new PagedResultDto<MohuFeedbackDto>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            });
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

        [HttpGet("violations/paged")]
        public async Task<ActionResult<PagedResultDto<MohuViolationRecordDto>>> GetViolationsPaged(
            [FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? search = null)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 20;
            if (pageSize > 200) pageSize = 200;

            var violations = (await _service.GetAllViolationsAsync()).AsEnumerable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                violations = violations.Where(v =>
                    Contains(v.ViolationType, term) ||
                    Contains(v.IsClosed ? "مغلقة" : "مفتوحة", term) ||
                    Contains(v.PenaltyAmount.ToString(), term) ||
                    Contains(v.CommitteeEvaluationScore.ToString(), term));
            }

            var ordered = violations.OrderByDescending(v => v.Id).ToList();
            var totalCount = ordered.Count;
            var items = ordered.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            return Ok(new PagedResultDto<MohuViolationRecordDto>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            });
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

        [HttpGet("permits/paged")]
        public async Task<ActionResult<PagedResultDto<MohuPermitLogDto>>> GetPermitsPaged(
            [FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? search = null)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 20;
            if (pageSize > 200) pageSize = 200;

            var permits = (await _service.GetAllPermitsAsync()).AsEnumerable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                permits = permits.Where(p => Contains(p.PermitNumber, term));
            }

            var ordered = permits.OrderByDescending(p => p.Id).ToList();
            var totalCount = ordered.Count;
            var items = ordered.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            return Ok(new PagedResultDto<MohuPermitLogDto>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            });
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

        private static bool Contains(string? haystack, string term) =>
            !string.IsNullOrEmpty(haystack) && haystack.IndexOf(term, System.StringComparison.OrdinalIgnoreCase) >= 0;
    }
}
