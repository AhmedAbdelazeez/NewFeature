using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NewFeature.Models;
using NewFeature.Services;
using NewFeature.Services.ExcelImport;

namespace NewFeature.Controllers
{
    // Sales department API: three templates (customers, fleet capacity, daily operations), each
    // with a download, an upload and a records index with add/edit/delete, plus the KPIs the
    // executive dashboard (project repo) reads server-to-server with no token - which is why the
    // read endpoints are anonymous. Everything that changes data needs a Sales (or admin) login,
    // the same roles the Sales pages themselves allow.
    [ApiController]
    [Route("api/sales")]
    public class SalesController : ControllerBase
    {
        private const string WriterRoles = "Admin,CEO,Administrator,SALES";
        private const long MaxUploadBytes = 50 * 1024 * 1024;
        private const string XlsxContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

        private readonly ISalesService _sales;
        private readonly UserManager<ApplicationUser> _userManager;

        public SalesController(ISalesService sales, UserManager<ApplicationUser> userManager)
        {
            _sales = sales;
            _userManager = userManager;
        }

        [AllowAnonymous]
        [HttpGet("kpis")]
        public async Task<ActionResult<SalesKpisDto>> GetKpis() => Ok(await _sales.GetSalesKpisAsync());

        [Authorize(Roles = WriterRoles)]
        [HttpGet("import-history")]
        public async Task<ActionResult<PagedResultDto<SalesImportBatchDto>>> GetImportHistory(
            [FromQuery] int page = 1, [FromQuery] int pageSize = 20) =>
            Ok(await _sales.GetImportHistoryAsync(page, pageSize));

        #region Templates
        // Generated from the same definitions the upload reads, so the file can't drift from the
        // parser. The customer and fleet templates come pre-filled with what is on file: both are
        // "replace" uploads, so editing the current list and re-uploading it is the normal workflow.

        [AllowAnonymous]
        [HttpGet("customers/template")]
        public async Task<IActionResult> CustomersTemplate()
        {
            var customers = await _sales.GetLatestYearCustomersAsync();
            var year = customers.FirstOrDefault()?.FiscalYear ?? DateTime.Today.Year;
            var rows = customers.Select(c => Row(
                (DepartmentTemplates.SalesCustomerCode, c.CustomerCode),
                (DepartmentTemplates.SalesCustomerName, c.CustomerName),
                (DepartmentTemplates.SalesCustomerGroup, c.CustomerGroup),
                (DepartmentTemplates.SalesCurrency, c.Currency))).ToList();

            var bytes = ExcelTemplateWriter.Build(
                "نموذج قائمة العملاء (Customer Roster)",
                new[] { new ExcelTemplateSheetSpec { Definition = DepartmentTemplates.SalesCustomers, SheetName = year.ToString(), Rows = rows } },
                new[]
                {
                    "• عدد العملاء النشطين في آخر سنة مالية",
                    "• العملاء الجدد مقارنة بالسنة السابقة",
                    "• أكبر فئة عملاء ونسبتها",
                    "",
                    "ملاحظات:",
                    $"- اسم الورقة هو السنة المالية ({year}). لرفع سنة أخرى غيّر اسم الورقة إلى تلك السنة، ويمكن وضع أكثر من سنة في الملف (ورقة لكل سنة).",
                    "- الملف مُعبأ مسبقاً بعملاء آخر سنة مرفوعة: أضف أو احذف أو عدّل ثم ارفعه.",
                    "- رفع سنة يستبدل كل عملاء نفس السنة السابقين، فلا يتكرر أي عميل.",
                    "- رقم الحساب (Order Account) لا يتكرر داخل نفس السنة."
                });
            return File(bytes, XlsxContentType, "Sales_Customers_Template.xlsx");
        }

        [AllowAnonymous]
        [HttpGet("fleet-capacity/template")]
        public async Task<IActionResult> FleetTemplate()
        {
            var fleet = await _sales.GetAllFleetAsync();
            var rows = fleet.Select(f => Row(
                (DepartmentTemplates.SalesBusCode, f.BusCode),
                (DepartmentTemplates.SalesBusType, f.BusType),
                (DepartmentTemplates.SalesCategory, f.Category),
                (DepartmentTemplates.SalesModelYear, f.ModelYear?.ToString()),
                (DepartmentTemplates.SalesNumberOfBuses, f.NumberOfBuses?.ToString()),
                (DepartmentTemplates.SalesSeatsPerBus, f.SeatsPerBus?.ToString()),
                (DepartmentTemplates.SalesTotalSeats, f.TotalSeats?.ToString()))).ToList();

            var bytes = ExcelTemplateWriter.Build(
                "نموذج الطاقة الاستيعابية للأسطول (Fleet Capacity)",
                new[] { new ExcelTemplateSheetSpec { Definition = DepartmentTemplates.SalesFleetCapacity, Rows = rows } },
                new[]
                {
                    "• إجمالي عدد الحافلات وإجمالي المقاعد",
                    "• نسبة استغلال الأسطول (الحافلات المجدولة ÷ عدد الحافلات)",
                    "",
                    "ملاحظات:",
                    "- الملف مُعبأ مسبقاً بالأسطول الحالي: عدّل الأعداد ثم ارفعه.",
                    "- كل رفع يستبدل بيانات الأسطول السابقة بالكامل (صورة حالية وليست حركة).",
                    "- إجمالي المقاعد يُحسب تلقائياً (عدد الحافلات × عدد المقاعد) إن تُرك فارغاً."
                });
            return File(bytes, XlsxContentType, "Sales_FleetCapacity_Template.xlsx");
        }

        [AllowAnonymous]
        [HttpGet("daily-operations/template")]
        public IActionResult DailyOperationsTemplate()
        {
            var bytes = ExcelTemplateWriter.Build(
                "نموذج التشغيل اليومي (Daily Operations)",
                new[] { new ExcelTemplateSheetSpec { Definition = DepartmentTemplates.SalesDailyOperations } },
                new[]
                {
                    "• عدد الطلبات والحافلات المطلوبة والمجدولة في آخر يوم تشغيل",
                    "• نسبة تغطية الجدولة (المجدولة ÷ المطلوبة)",
                    "",
                    "ملاحظات:",
                    "- صف لكل أمر إيجار. التاريخ مثل 2026-08-20 والوقت مثل 06:00.",
                    "- الحافلات المجدولة تُترك فارغة إذا لم تُجدول بعد (تُحسب صفراً).",
                    "- رفع ملف يستبدل بيانات نفس الأيام الموجودة فيه فقط، وباقي الأيام تبقى كما هي."
                });
            return File(bytes, XlsxContentType, "Sales_DailyOperations_Template.xlsx");
        }

        private static IReadOnlyDictionary<string, string?> Row(params (string Key, string? Value)[] cells) =>
            cells.ToDictionary(c => c.Key, c => c.Value);

        #endregion

        #region Uploads

        [Authorize(Roles = WriterRoles)]
        [HttpPost("customers/bulk-upload")]
        public Task<IActionResult> UploadCustomers(IFormFile file) =>
            RunUploadAsync(file, (bytes, name, user) => _sales.BulkUploadCustomersAsync(new MemoryStream(bytes), name, user));

        [Authorize(Roles = WriterRoles)]
        [HttpPost("fleet-capacity/bulk-upload")]
        public Task<IActionResult> UploadFleet(IFormFile file) =>
            RunUploadAsync(file, (bytes, name, user) => _sales.BulkUploadFleetCapacityAsync(new MemoryStream(bytes), name, user));

        [Authorize(Roles = WriterRoles)]
        [HttpPost("daily-operations/bulk-upload")]
        public Task<IActionResult> UploadDailyOperations(IFormFile file) =>
            RunUploadAsync(file, (bytes, name, user) => _sales.BulkUploadDailyOperationsAsync(new MemoryStream(bytes), name, user));

        // The combined workbook: all three sheets in one file.
        [Authorize(Roles = WriterRoles)]
        [HttpPost("upload")]
        public Task<IActionResult> UploadCombined(IFormFile file) =>
            RunUploadAsync(file, (bytes, name, user) => _sales.BulkUploadCombinedAsync(bytes, name, user));

        private async Task<IActionResult> RunUploadAsync(IFormFile file, Func<byte[], string, string, Task<ExcelImportResultDto>> import)
        {
            if (file == null || file.Length == 0) return BadRequest(new ExcelImportResultDto { Message = "لم يتم اختيار أي ملف." });
            if (file.Length > MaxUploadBytes) return BadRequest(new ExcelImportResultDto { Message = $"حجم الملف أكبر من {MaxUploadBytes / (1024 * 1024)} ميجابايت." });
            if (!ExcelCompatibility.IsSupportedExcelFile(file.FileName, file.ContentType))
                return BadRequest(new ExcelImportResultDto { Message = "الملفات المدعومة هي .xlsx و .xls فقط." });

            var user = await _userManager.GetUserAsync(User);

            byte[] xlsx;
            await using (var raw = file.OpenReadStream())
            using (var converted = ExcelCompatibility.EnsureXlsxStream(raw))
            using (var buffer = new MemoryStream())
            {
                await converted.CopyToAsync(buffer);
                xlsx = buffer.ToArray();
            }

            var result = await import(xlsx, file.FileName, user?.Email ?? user?.Id ?? "system");
            return result.Success ? Ok(result) : UnprocessableEntity(result);
        }

        #endregion

        #region Customers

        [AllowAnonymous]
        [HttpGet("customers")]
        public async Task<ActionResult<PagedResultDto<SalesCustomerDto>>> GetCustomers(
            [FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? search = null,
            [FromQuery] int? fiscalYear = null, [FromQuery] string? customerGroup = null) =>
            Ok(await _sales.GetCustomersPagedAsync(page, pageSize, search, fiscalYear, customerGroup));

        [AllowAnonymous]
        [HttpGet("customers/filter-options")]
        public async Task<ActionResult<SalesCustomerFilterOptionsDto>> GetCustomerFilterOptions() =>
            Ok(await _sales.GetCustomerFilterOptionsAsync());

        [AllowAnonymous]
        [HttpGet("customers/{id:int}")]
        public async Task<IActionResult> GetCustomer(int id) =>
            await _sales.GetCustomerAsync(id) is { } item ? Ok(item) : NotFound();

        [Authorize(Roles = WriterRoles)]
        [HttpPost("customers")]
        public async Task<IActionResult> CreateCustomer([FromBody] SalesCustomerDto dto) =>
            CreatedOrInvalid(await _sales.CreateCustomerAsync(dto));

        [Authorize(Roles = WriterRoles)]
        [HttpPut("customers/{id:int}")]
        public async Task<IActionResult> UpdateCustomer(int id, [FromBody] SalesCustomerDto dto) =>
            UpdatedOrInvalid(await _sales.UpdateCustomerAsync(id, dto));

        [Authorize(Roles = WriterRoles)]
        [HttpDelete("customers/{id:int}")]
        public async Task<IActionResult> DeleteCustomer(int id) =>
            await _sales.DeleteCustomerAsync(id) ? NoContent() : NotFound();

        #endregion

        #region Fleet capacity

        [AllowAnonymous]
        [HttpGet("fleet-capacity")]
        public async Task<ActionResult<PagedResultDto<SalesFleetCapacityDto>>> GetFleet(
            [FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? search = null,
            [FromQuery] string? category = null, [FromQuery] string? busType = null) =>
            Ok(await _sales.GetFleetPagedAsync(page, pageSize, search, category, busType));

        [AllowAnonymous]
        [HttpGet("fleet-capacity/filter-options")]
        public async Task<ActionResult<SalesFleetFilterOptionsDto>> GetFleetFilterOptions() =>
            Ok(await _sales.GetFleetFilterOptionsAsync());

        [AllowAnonymous]
        [HttpGet("fleet-capacity/{id:int}")]
        public async Task<IActionResult> GetFleetItem(int id) =>
            await _sales.GetFleetItemAsync(id) is { } item ? Ok(item) : NotFound();

        [Authorize(Roles = WriterRoles)]
        [HttpPost("fleet-capacity")]
        public async Task<IActionResult> CreateFleetItem([FromBody] SalesFleetCapacityDto dto) =>
            CreatedOrInvalid(await _sales.CreateFleetItemAsync(dto));

        [Authorize(Roles = WriterRoles)]
        [HttpPut("fleet-capacity/{id:int}")]
        public async Task<IActionResult> UpdateFleetItem(int id, [FromBody] SalesFleetCapacityDto dto) =>
            UpdatedOrInvalid(await _sales.UpdateFleetItemAsync(id, dto));

        [Authorize(Roles = WriterRoles)]
        [HttpDelete("fleet-capacity/{id:int}")]
        public async Task<IActionResult> DeleteFleetItem(int id) =>
            await _sales.DeleteFleetItemAsync(id) ? NoContent() : NotFound();

        #endregion

        #region Daily operations

        [AllowAnonymous]
        [HttpGet("daily-operations")]
        public async Task<ActionResult<PagedResultDto<SalesDailyOperationDto>>> GetDailyOperations(
            [FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? search = null,
            [FromQuery] DateTime? fromDate = null, [FromQuery] DateTime? toDate = null,
            [FromQuery] string? requestType = null, [FromQuery] string? executionPoint = null) =>
            Ok(await _sales.GetDailyOperationsPagedAsync(page, pageSize, search, fromDate, toDate, requestType, executionPoint));

        [AllowAnonymous]
        [HttpGet("daily-operations/filter-options")]
        public async Task<ActionResult<SalesOperationFilterOptionsDto>> GetOperationFilterOptions() =>
            Ok(await _sales.GetOperationFilterOptionsAsync());

        [AllowAnonymous]
        [HttpGet("daily-operations/{id:int}")]
        public async Task<IActionResult> GetDailyOperation(int id) =>
            await _sales.GetDailyOperationAsync(id) is { } item ? Ok(item) : NotFound();

        [Authorize(Roles = WriterRoles)]
        [HttpPost("daily-operations")]
        public async Task<IActionResult> CreateDailyOperation([FromBody] SalesDailyOperationDto dto) =>
            CreatedOrInvalid(await _sales.CreateDailyOperationAsync(dto));

        [Authorize(Roles = WriterRoles)]
        [HttpPut("daily-operations/{id:int}")]
        public async Task<IActionResult> UpdateDailyOperation(int id, [FromBody] SalesDailyOperationDto dto) =>
            UpdatedOrInvalid(await _sales.UpdateDailyOperationAsync(id, dto));

        [Authorize(Roles = WriterRoles)]
        [HttpDelete("daily-operations/{id:int}")]
        public async Task<IActionResult> DeleteDailyOperation(int id) =>
            await _sales.DeleteDailyOperationAsync(id) ? NoContent() : NotFound();

        #endregion

        private IActionResult CreatedOrInvalid<T>(CrudResult<T> result) =>
            result.Success ? StatusCode(StatusCodes.Status201Created, result.Item) : BadRequest(result.ToErrorBody());

        private IActionResult UpdatedOrInvalid<T>(CrudResult<T> result) =>
            result.NotFound ? NotFound() : result.Success ? Ok(result.Item) : BadRequest(result.ToErrorBody());
    }
}
