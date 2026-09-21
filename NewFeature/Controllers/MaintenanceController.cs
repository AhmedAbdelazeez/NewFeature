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
    public class MaintenanceController : ControllerBase
    {
        private readonly IMaintenanceService _maintenanceService;

        public MaintenanceController(IMaintenanceService maintenanceService)
        {
            _maintenanceService = maintenanceService;
        }

        #region Work orders: index + CRUD (the Maintenance page)
        // Also read by the executive dashboard's work-orders table (without a search term).
        [HttpGet("workorders/paged")]
        public async Task<ActionResult<PagedResultDto<MaintenanceWorkOrderDto>>> GetWorkOrdersPaged(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] System.DateTime? fromDate = null,
            [FromQuery] System.DateTime? toDate = null,
            [FromQuery] string? search = null)
        {
            return Ok(await _maintenanceService.GetWorkOrdersPagedAsync(page, pageSize, fromDate, toDate, search));
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<MaintenanceWorkOrderDto>> GetWorkOrder(int id)
        {
            var order = await _maintenanceService.GetWorkOrderByIdAsync(id);
            return order == null ? NotFound() : Ok(order);
        }

        [HttpPost]
        public async Task<IActionResult> CreateWorkOrder([FromBody] MaintenanceWorkOrderDto dto)
        {
            var result = await _maintenanceService.CreateWorkOrderAsync(dto);
            if (!result.Success) return BadRequest(result.ToErrorBody());
            return CreatedAtAction(nameof(GetWorkOrder), new { id = result.Item!.Id }, result.Item);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateWorkOrder(int id, [FromBody] MaintenanceWorkOrderDto dto)
        {
            var result = await _maintenanceService.UpdateWorkOrderAsync(id, dto);
            if (result.NotFound) return NotFound();
            if (!result.Success) return BadRequest(result.ToErrorBody());
            return Ok(result.Item);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteWorkOrder(int id)
        {
            return await _maintenanceService.DeleteWorkOrderAsync(id) ? NoContent() : NotFound();
        }
        #endregion

        [HttpGet("kpis")]
        public async Task<ActionResult<MaintenanceKpisDto>> GetMaintenanceKpis()
        {
            var kpis = await _maintenanceService.GetMaintenanceKpisAsync();
            return Ok(kpis);
        }

        // The downloadable template, generated from the very definition the importer validates
        // against - so a file the user downloads here can never be rejected for a column the
        // template itself produced.
        [HttpGet("template")]
        public IActionResult DownloadTemplate()
        {
            var bytes = ExcelTemplateWriter.Build(
                "نموذج أوامر العمل الداخلية - إدارة الصيانة",
                new[]
                {
                    new ExcelTemplateSheetSpec
                    {
                        Definition = DepartmentTemplates.Maintenance,
                        Dropdowns = new Dictionary<string, string[]>
                        {
                            [DepartmentTemplates.MaintenanceStatus] =
                                new[] { "تم الانتهاء", "جاري العمل", "متوقف علي قطع غيار" }
                        }
                    }
                },
                new[]
                {
                    "• إجمالي أوامر العمل",
                    "• أوامر العمل المكتملة ونسبة الإنجاز",
                    "• متوسط زمن الإصلاح (MTTR) - يُحسب من تاريخ وساعة الدخول حتى تاريخ وساعة الخروج",
                    "• أوامر متوقفة على قطع غيار",
                    "• أوامر جاري العمل",
                    "• نسبة الأعمال المتراكمة",
                    "• عدد الحافلات التي دخلت الورشة",
                    "• عدد الفنيين المشاركين",
                    "• أكثر الحافلات تكراراً للأعطال",
                    "",
                    "ملاحظات:",
                    "- رقم أمر العمل هو ما يميز الصف: إعادة رفع نفس الملف بعد التصحيح تُحدّث الأوامر بدل تكرارها.",
                    "- اسم الفني 1 إلزامي، أما الفني 2 و3 و4 فاختيارية وتُترك فارغة عند عدم وجودهم.",
                    "- أمر العمل بحالة \"تم الانتهاء\" يجب أن يحتوي على تاريخ أو ساعة خروج، لأن متوسط زمن الإصلاح يُحسب منها."
                });

            return File(bytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "Maintenance_Template.xlsx");
        }

        // Bulk upload of the approved single-sheet "Internal work orders" template.
        [HttpPost("bulk-upload")]
        public async Task<IActionResult> BulkUpload(Microsoft.AspNetCore.Http.IFormFile file, [FromQuery] string branchName = "الورشة المركزية")
        {
            if (file == null || file.Length == 0) return BadRequest("لم يتم اختيار أي ملف.");
            if (!ExcelCompatibility.IsSupportedExcelFile(file.FileName, file.ContentType))
                return BadRequest("الملفات المدعومة هي .xlsx و .xls فقط.");

            using var rawStream = file.OpenReadStream();
            using var stream = ExcelCompatibility.EnsureXlsxStream(rawStream);
            var result = await _maintenanceService.BulkUploadWorkOrdersAsync(stream, branchName);

            // A template/header problem is the uploader's mistake, not a server fault - report it
            // as 422 so the page can surface the message instead of a generic failure.
            if (!result.Success) return UnprocessableEntity(result);
            return Ok(result);
        }
    }
}
