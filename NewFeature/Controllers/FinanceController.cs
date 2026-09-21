using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NewFeature.Models;
using NewFeature.Services;
using NewFeature.Services.ExcelImport;

namespace NewFeature.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [AllowAnonymous]
    public class FinanceController : ControllerBase
    {
        private readonly IFinanceService _financeService;

        public FinanceController(IFinanceService financeService)
        {
            _financeService = financeService;
        }

        #region Transactions
        [HttpGet("transactions")]
        public async Task<ActionResult<IEnumerable<FinanceTransactionDto>>> GetTransactions()
        {
            var items = await _financeService.GetAllTransactionsAsync();
            return Ok(items);
        }

        // Paginated + searchable listing used by the Finance page's Transactions table.
        [HttpGet("transactions/paged")]
        public async Task<ActionResult<PagedResultDto<FinanceTransactionDto>>> GetTransactionsPaged(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? search = null,
            [FromQuery] string? type = null)
        {
            var result = await _financeService.GetTransactionsPagedAsync(page, pageSize, search, type);
            return Ok(result);
        }

        [HttpGet("transactions/{id}")]
        public async Task<ActionResult<FinanceTransactionDto>> GetTransaction(int id)
        {
            var item = await _financeService.GetTransactionByIdAsync(id);
            if (item == null) return NotFound();
            return Ok(item);
        }

        [HttpPost("transactions")]
        public async Task<ActionResult<FinanceTransactionDto>> CreateTransaction([FromBody] FinanceTransactionDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var created = await _financeService.CreateTransactionAsync(dto);
            return CreatedAtAction(nameof(GetTransaction), new { id = created.Id }, created);
        }

        [HttpPut("transactions/{id}")]
        public async Task<IActionResult> UpdateTransaction(int id, [FromBody] FinanceTransactionDto dto)
        {
            if (id != dto.Id) return BadRequest("ID mismatch");
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var success = await _financeService.UpdateTransactionAsync(dto);
            if (!success) return NotFound();
            return NoContent();
        }

        [HttpDelete("transactions/{id}")]
        public async Task<IActionResult> DeleteTransaction(int id)
        {
            var success = await _financeService.DeleteTransactionAsync(id);
            if (!success) return NotFound();
            return NoContent();
        }
        #endregion

        #region Budgets
        [HttpGet("budgets")]
        public async Task<ActionResult<IEnumerable<FinanceBudgetDto>>> GetBudgets()
        {
            var items = await _financeService.GetAllBudgetsAsync();
            return Ok(items);
        }

        // Paginated + searchable listing used by the Finance page's Budgets table.
        [HttpGet("budgets/paged")]
        public async Task<ActionResult<PagedResultDto<FinanceBudgetDto>>> GetBudgetsPaged(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? search = null)
        {
            var result = await _financeService.GetBudgetsPagedAsync(page, pageSize, search);
            return Ok(result);
        }

        [HttpGet("budgets/{id}")]
        public async Task<ActionResult<FinanceBudgetDto>> GetBudget(int id)
        {
            var item = await _financeService.GetBudgetByIdAsync(id);
            if (item == null) return NotFound();
            return Ok(item);
        }

        [HttpPost("budgets")]
        public async Task<ActionResult<FinanceBudgetDto>> CreateBudget([FromBody] FinanceBudgetDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var created = await _financeService.CreateBudgetAsync(dto);
            return CreatedAtAction(nameof(GetBudget), new { id = created.Id }, created);
        }

        [HttpPut("budgets/{id}")]
        public async Task<IActionResult> UpdateBudget(int id, [FromBody] FinanceBudgetDto dto)
        {
            if (id != dto.Id) return BadRequest("ID mismatch");
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var success = await _financeService.UpdateBudgetAsync(dto);
            if (!success) return NotFound();
            return NoContent();
        }

        [HttpDelete("budgets/{id}")]
        public async Task<IActionResult> DeleteBudget(int id)
        {
            var success = await _financeService.DeleteBudgetAsync(id);
            if (!success) return NotFound();
            return NoContent();
        }
        #endregion

        #region KPIs
        [HttpGet("kpis")]
        public async Task<ActionResult<FinanceKpisDto>> GetKpis()
        {
            var kpis = await _financeService.GetFinanceKpisAsync();
            return Ok(kpis);
        }
        #endregion

        #region Chart of accounts + balances
        [HttpGet("chart-of-accounts")]
        public async Task<ActionResult<IEnumerable<ChartOfAccountDto>>> GetChartOfAccounts()
        {
            var accounts = await _financeService.GetChartOfAccountsAsync();
            return Ok(accounts);
        }

        [HttpGet("balances")]
        public async Task<ActionResult<PagedResultDto<FinanceAccountBalanceDto>>> GetBalances(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? search = null,
            [FromQuery] System.DateTime? asOfDate = null)
        {
            var result = await _financeService.GetAccountBalancesPagedAsync(page, pageSize, search, asOfDate);
            return Ok(result);
        }

        [HttpGet("balances/{id:int}")]
        public async Task<ActionResult<FinanceAccountBalanceDto>> GetBalance(int id)
        {
            var balance = await _financeService.GetBalanceAsync(id);
            return balance == null ? NotFound() : Ok(balance);
        }

        [HttpPost("balances")]
        public async Task<IActionResult> CreateBalance([FromBody] FinanceAccountBalanceDto dto)
        {
            var result = await _financeService.CreateBalanceAsync(dto);
            if (!result.Success) return BadRequest(result.ToErrorBody());
            return CreatedAtAction(nameof(GetBalance), new { id = result.Item!.Id }, result.Item);
        }

        [HttpPut("balances/{id:int}")]
        public async Task<IActionResult> UpdateBalance(int id, [FromBody] FinanceAccountBalanceDto dto)
        {
            var result = await _financeService.UpdateBalanceAsync(id, dto);
            if (result.NotFound) return NotFound();
            if (!result.Success) return BadRequest(result.ToErrorBody());
            return Ok(result.Item);
        }

        [HttpDelete("balances/{id:int}")]
        public async Task<IActionResult> DeleteBalance(int id)
        {
            return await _financeService.DeleteBalanceAsync(id) ? NoContent() : NotFound();
        }

        [HttpGet("chart-of-accounts/paged")]
        public async Task<ActionResult<PagedResultDto<ChartOfAccountDto>>> GetChartOfAccountsPaged(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? search = null)
        {
            return Ok(await _financeService.GetChartOfAccountsPagedAsync(page, pageSize, search));
        }

        [HttpGet("chart-of-accounts/{id:int}")]
        public async Task<ActionResult<ChartOfAccountDto>> GetAccount(int id)
        {
            var account = await _financeService.GetAccountAsync(id);
            return account == null ? NotFound() : Ok(account);
        }

        [HttpPost("chart-of-accounts")]
        public async Task<IActionResult> CreateAccount([FromBody] ChartOfAccountDto dto)
        {
            var result = await _financeService.CreateAccountAsync(dto);
            if (!result.Success) return BadRequest(result.ToErrorBody());
            return CreatedAtAction(nameof(GetAccount), new { id = result.Item!.Id }, result.Item);
        }

        [HttpPut("chart-of-accounts/{id:int}")]
        public async Task<IActionResult> UpdateAccount(int id, [FromBody] ChartOfAccountDto dto)
        {
            var result = await _financeService.UpdateAccountAsync(id, dto);
            if (result.NotFound) return NotFound();
            if (!result.Success) return BadRequest(result.ToErrorBody());
            return Ok(result.Item);
        }

        [HttpDelete("chart-of-accounts/{id:int}")]
        public async Task<IActionResult> DeleteAccount(int id)
        {
            var result = await _financeService.DeleteAccountAsync(id);
            if (result.NotFound) return NotFound();
            if (!result.Success) return BadRequest(result.ToErrorBody());
            return NoContent();
        }
        #endregion

        // The downloadable template, generated from the very definitions the importer validates
        // against. The COA sheet ships pre-filled with the chart of accounts currently on file, so
        // the accountant downloads the tree they are expected to report against and returns it with
        // the balances sheet completed - which is also what makes every uploaded balance
        // classifiable.
        [HttpGet("template")]
        public async Task<IActionResult> DownloadTemplate()
        {
            var accounts = await _financeService.GetChartOfAccountsAsync();

            var coaRows = accounts.Select(a => new Dictionary<string, string?>
            {
                [DepartmentTemplates.FinanceAccountNumber] = a.AccountNumber,
                [DepartmentTemplates.FinanceAccountName] = a.AccountName,
                [DepartmentTemplates.FinanceMapping] = a.Mapping,
                [DepartmentTemplates.FinanceBsClassification] = a.BsClassification,
                [DepartmentTemplates.FinanceIsClassification] = a.IsClassification,
                [DepartmentTemplates.FinanceRsmClassification] = a.RsmClassification,
                [DepartmentTemplates.FinanceManagementClassification] = a.ManagementClassification,
                [DepartmentTemplates.FinanceRevenueMainClassification] = a.RevenueMainClassification,
                [DepartmentTemplates.FinanceRevenueSubClassification] = a.RevenueSubClassification
            } as IReadOnlyDictionary<string, string?>).ToList();

            var bytes = ExcelTemplateWriter.Build(
                "النموذج المالي - شجرة الحسابات والأرصدة",
                new[]
                {
                    new ExcelTemplateSheetSpec
                    {
                        Definition = DepartmentTemplates.FinanceChartOfAccounts,
                        SheetName = "COA",
                        Rows = coaRows
                    },
                    new ExcelTemplateSheetSpec
                    {
                        Definition = DepartmentTemplates.FinanceBalances,
                        SheetName = "الحركات المالية"
                    }
                },
                new[]
                {
                    "• إجمالي الإيرادات",
                    "• تكلفة الإيرادات",
                    "• مجمل الربح وهامش مجمل الربح",
                    "• المصروفات التشغيلية والعمومية",
                    "• إجمالي المصروفات",
                    "• صافي الربح وهامش صافي الربح",
                    "• نسبة المصروفات إلى الإيرادات",
                    "• النقد وما في حكمه",
                    "• الذمم المدينة",
                    "• أعلى بند مصروف",
                    "",
                    "ملاحظات:",
                    "- الملف يحتوي على ورقتَي بيانات: COA (شجرة الحسابات) و\"الحركات المالية\" (الأرصدة). لا تحذف أياً منهما.",
                    "- عمود RSM Classification هو ما تُصنَّف به الحسابات (7000 إيرادات، 71xx تكلفة إيرادات، 72xx مصروفات تشغيلية، 5xxx موجودات، 6xxx مطلوبات وحقوق ملكية).",
                    "- كل رصيد يجب أن يكون رقم حسابه موجوداً في ورقة COA، وإلا يُرفض الصف مع بيان رقم الحساب.",
                    "- الرصيد يُميَّز بـ (التاريخ + رقم الحساب): إعادة رفع نفس الشهر تُحدّث الأرصدة بدل تكرارها.",
                    "- المؤشرات تصف آخر تاريخ مرفوع، لأن ميزان المراجعة لقطة في تاريخ محدد وليس مجموع الأشهر."
                });

            return File(bytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "Finance_Template.xlsx");
        }

        // Bulk upload of the approved Finance template (COA + الحركات المالية in one workbook).
        [HttpPost("bulk-upload")]
        public async Task<IActionResult> BulkUpload(Microsoft.AspNetCore.Http.IFormFile file)
        {
            if (file == null || file.Length == 0) return BadRequest("لم يتم اختيار أي ملف.");
            if (!ExcelCompatibility.IsSupportedExcelFile(file.FileName, file.ContentType))
                return BadRequest("الملفات المدعومة هي .xlsx و .xls فقط.");

            using var rawStream = file.OpenReadStream();
            using var stream = ExcelCompatibility.EnsureXlsxStream(rawStream);
            var result = await _financeService.BulkUploadFinanceWorkbookAsync(stream);

            // A template/header problem is the uploader's mistake, not a server fault - report it
            // as 422 so the page can surface the message instead of a generic failure.
            if (!result.Success) return UnprocessableEntity(result);
            return Ok(result);
        }
    }
}
