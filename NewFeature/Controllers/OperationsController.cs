using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NewFeature.Models;
using NewFeature.Services;
using NewFeature.Services.ExcelImport;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NewFeature.Controllers
{
    [ApiController]
    [Route("api/[controller]")]

    [AllowAnonymous]
    public class OperationsController : ControllerBase
    {
        private readonly IOperationsService _operationsService;

        public OperationsController(IOperationsService operationsService)
        {
            _operationsService = operationsService;
        }

        // The downloadable template, generated from the very definition the importer validates
        // against - so a file the user downloads here can never be rejected for a column the
        // template itself produced.
        [HttpGet("template")]
        public IActionResult DownloadTemplate()
        {
            var bytes = ExcelTemplateWriter.Build(
                "نموذج أوامر التشغيل - إدارة العمليات",
                new[]
                {
                    new ExcelTemplateSheetSpec
                    {
                        Definition = DepartmentTemplates.Operations,
                        SheetName = "Operations"
                    }
                },
                new[]
                {
                    "• إجمالي أوامر التشغيل",
                    "• عدد أوامر الإيجار",
                    "• عدد العملاء المخدومين",
                    "• عدد الحافلات المشغّلة",
                    "• عدد السائقين المكلفين",
                    "• الأوامر المنفذة ونسبة الإنجاز",
                    "• إجمالي الكيلومترات المخططة",
                    "• إجمالي الكيلومترات الفعلية",
                    "• إجمالي الديزل (لتر)",
                    "• متوسط الأوامر اليومية",
                    "• أكثر الخطوط تشغيلاً",
                    "",
                    "ملاحظات:",
                    "- الصف يُميَّز بـ (Direction + Rent Order + Bus number + DELV. Date): إعادة رفع الشهر بعد التصحيح تُحدّث الصفوف بدل تكرارها.",
                    "- عمود Completeion هو ما تُحسب منه نسبة الإنجاز؛ أي قيمة تعني الإتمام (Completed / مكتمل / تم) تُحتسب منفذة.",
                    "- ADD. Driver و ADD. Driver Name اختياريان ويُتركان فارغين عند عدم وجود سائق مساعد."
                });

            return File(bytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "Operations_Template.xlsx");
        }

        // The Operations department's single bulk upload: the approved dispatch-log template
        // (one row per bus assigned to a rental order on a day). The separate drivers-roster and
        // route-scheduling uploads were removed - the department now works from one template.
        [HttpPost("bulk-upload")]
        public async Task<IActionResult> BulkUploadDispatchLog(Microsoft.AspNetCore.Http.IFormFile file)
        {
            if (file == null || file.Length == 0) return BadRequest("لم يتم اختيار أي ملف.");
            if (!ExcelCompatibility.IsSupportedExcelFile(file.FileName, file.ContentType))
                return BadRequest("الملفات المدعومة هي .xlsx و .xls فقط.");

            using var rawStream = file.OpenReadStream();
            using var stream = ExcelCompatibility.EnsureXlsxStream(rawStream);
            var result = await _operationsService.BulkUploadDispatchLogAsync(stream);

            // A template/header problem is the uploader's mistake, not a server fault - report it
            // as 422 so the page can surface the message instead of a generic failure.
            if (!result.Success) return UnprocessableEntity(result);
            return Ok(result);
        }

        #region Dispatch records: index + CRUD (the Operations page)
        [HttpGet("records")]
        public async Task<ActionResult<PagedResultDto<OperationsDispatchRecordDto>>> GetRecords(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? search = null,
            [FromQuery] System.DateTime? fromDate = null,
            [FromQuery] System.DateTime? toDate = null)
        {
            return Ok(await _operationsService.GetDispatchRecordsPagedAsync(page, pageSize, search, fromDate, toDate));
        }

        [HttpGet("records/{id:int}")]
        public async Task<ActionResult<OperationsDispatchRecordDto>> GetRecord(int id)
        {
            var record = await _operationsService.GetDispatchRecordAsync(id);
            return record == null ? NotFound() : Ok(record);
        }

        [HttpPost("records")]
        public async Task<IActionResult> CreateRecord([FromBody] OperationsDispatchRecordDto dto)
        {
            var result = await _operationsService.CreateDispatchRecordAsync(dto);
            if (!result.Success) return BadRequest(result.ToErrorBody());
            return CreatedAtAction(nameof(GetRecord), new { id = result.Item!.Id }, result.Item);
        }

        [HttpPut("records/{id:int}")]
        public async Task<IActionResult> UpdateRecord(int id, [FromBody] OperationsDispatchRecordDto dto)
        {
            var result = await _operationsService.UpdateDispatchRecordAsync(id, dto);
            if (result.NotFound) return NotFound();
            if (!result.Success) return BadRequest(result.ToErrorBody());
            return Ok(result.Item);
        }

        [HttpDelete("records/{id:int}")]
        public async Task<IActionResult> DeleteRecord(int id)
        {
            return await _operationsService.DeleteDispatchRecordAsync(id) ? NoContent() : NotFound();
        }
        #endregion

        #region KPIs (read by the executive dashboard)
        [HttpGet("kpis")]
        public async Task<ActionResult<OperationsKpisDto>> GetOperationsKpis()
        {
            var kpis = await _operationsService.GetOperationsKpisAsync();
            return Ok(kpis);
        }

        #endregion
    }
}
