using Microsoft.AspNetCore.Authorization;
using System;
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
        public async Task<ActionResult<FinanceKpisDto>> GetKpis([FromQuery] string? branch = null)
        {
            var kpis = await _financeService.GetFinanceKpisAsync(branch);
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

        // The الأرصدة page's index: search, a date range, the branch, and the account tree's own
        // groupings (Mapping / RSM / Management Classification) as dropdown filters.
        [HttpGet("balances")]
        public async Task<ActionResult<PagedResultDto<FinanceAccountBalanceDto>>> GetBalances(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? search = null,
            [FromQuery] System.DateTime? asOfDate = null,
            [FromQuery] string? branch = null,
            [FromQuery] System.DateTime? fromDate = null,
            [FromQuery] System.DateTime? toDate = null,
            [FromQuery] string? mapping = null,
            [FromQuery] string? rsmClassification = null,
            [FromQuery] string? managementClassification = null)
        {
            var filter = new FinanceBalanceFilter
            {
                Branch = branch,
                FromDate = fromDate,
                ToDate = toDate,
                Mapping = mapping,
                RsmClassification = rsmClassification,
                ManagementClassification = managementClassification
            };

            var result = await _financeService.GetAccountBalancesPagedAsync(page, pageSize, search, asOfDate, filter);
            return Ok(result);
        }

        [HttpGet("balances/filter-options")]
        public async Task<ActionResult<FinanceBalanceFilterOptionsDto>> GetBalanceFilterOptions() =>
            Ok(await _financeService.GetBalanceFilterOptionsAsync());

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

        // The شجرة الحسابات page's index: search plus one dropdown per classification column, so
        // the tree can be read the way the finance team reads it ("كل الحسابات المصنّفة ذمم مدينة
        // بالصافي").
        [HttpGet("chart-of-accounts/paged")]
        public async Task<ActionResult<PagedResultDto<ChartOfAccountDto>>> GetChartOfAccountsPaged(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? search = null,
            [FromQuery] string? mapping = null,
            [FromQuery] string? bsClassification = null,
            [FromQuery] string? isClassification = null,
            [FromQuery] string? rsmClassification = null,
            [FromQuery] string? managementClassification = null,
            [FromQuery] string? revenueMainClassification = null,
            [FromQuery] string? revenueSubClassification = null)
        {
            var filter = new ChartOfAccountFilter
            {
                Mapping = mapping,
                BsClassification = bsClassification,
                IsClassification = isClassification,
                RsmClassification = rsmClassification,
                ManagementClassification = managementClassification,
                RevenueMainClassification = revenueMainClassification,
                RevenueSubClassification = revenueSubClassification
            };

            return Ok(await _financeService.GetChartOfAccountsPagedAsync(page, pageSize, search, filter));
        }

        [HttpGet("chart-of-accounts/filter-options")]
        public async Task<ActionResult<FinanceAccountFilterOptionsDto>> GetChartOfAccountFilterOptions() =>
            Ok(await _financeService.GetChartOfAccountFilterOptionsAsync());

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

        #region The two downloadable templates
        // Both templates are generated from the very definitions the importer validates against.
        // Generating one from the other is the point: a template that drifts from its own validation
        // rules is what produces "I used your file and it was rejected".

        // Template 1 - the account tree. It ships pre-filled with the chart of accounts currently on
        // file, so the accountant edits the tree the KPIs are actually classified through instead of
        // rebuilding it from memory.
        [HttpGet("chart-of-accounts/template")]
        public async Task<IActionResult> DownloadChartOfAccountsTemplate()
        {
            var accounts = await _financeService.GetChartOfAccountsAsync();

            var bytes = ExcelTemplateWriter.Build(
                "نموذج شجرة الحسابات (COA)",
                new[]
                {
                    new ExcelTemplateSheetSpec
                    {
                        Definition = DepartmentTemplates.FinanceChartOfAccountsFile,
                        SheetName = "COA",
                        Rows = ChartOfAccountRows(accounts)
                    }
                },
                new[]
                {
                    "هذا الملف يُرفع من صفحة \"شجرة الحسابات\" ويُحدَّث عند إضافة حسابات جديدة فقط، وليس كل شهر.",
                    "",
                    "ملاحظات:",
                    "- الملف يحتوي على ورقة بيانات واحدة (COA). كل حساب مرة واحدة، ورقم الحساب هو ما يميّزه.",
                    "- عمود RSM Classification هو ما تُصنَّف به الحسابات: 7000 إيرادات، 71xx تكلفة إيرادات، 72xx مصروفات تشغيلية وعمومية، 74xx/75xx مصروفات أخرى، 5xxx موجودات، 6xxx مطلوبات، 69xx حقوق ملكية.",
                    "- عمود Mapping هو ما تُبنى عليه بطاقات المركز المالي (النقد، الذمم المدينة، الذمم الدائنة والمستحقات)، فاحرص على تعبئته.",
                    "- عمود Management Classification هو ما يُحسب منه \"أعلى بند مصروف\"، وعمود Revenues Main Classification هو ما يُحسب منه \"أكبر مصدر إيراد\".",
                    "- إعادة رفع الملف تُحدّث بيانات الحسابات الموجودة ولا تكررها.",
                    "- لا يمكن رفع الأرصدة قبل رفع هذا الملف، فكل رصيد يُصنَّف من خلاله."
                });

            return File(bytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "Finance_ChartOfAccounts_Template.xlsx");
        }

        // Template 2 - the monthly figures. Pre-filled with every account number and name on file,
        // so the accountant fills in the date, the branch and the figures rather than re-typing 500
        // account numbers by hand (the row order matches their own trial balance export).
        [HttpGet("balances/template")]
        public async Task<IActionResult> DownloadBalancesTemplate()
        {
            var accounts = await _financeService.GetChartOfAccountsAsync();

            var rows = accounts.Select(a => new Dictionary<string, string?>
            {
                [DepartmentTemplates.FinanceBalanceAccountNumber] = a.AccountNumber,
                [DepartmentTemplates.FinanceBalanceAccountName] = a.AccountName
            } as IReadOnlyDictionary<string, string?>).ToList();

            var bytes = ExcelTemplateWriter.Build(
                "نموذج الأرصدة والحركات المالية",
                new[]
                {
                    new ExcelTemplateSheetSpec
                    {
                        Definition = DepartmentTemplates.FinanceBalancesFile,
                        SheetName = "الحركات المالية",
                        Rows = rows
                    }
                },
                new[]
                {
                    "• إجمالي الإيرادات، تكلفة الإيرادات، مجمل الربح وهامشه",
                    "• المصروفات التشغيلية والعمومية، إجمالي المصروفات",
                    "• صافي الربح وهامشه، نسبة المصروفات إلى الإيرادات",
                    "• النقد وما في حكمه، الذمم المدينة، الذمم الدائنة والمستحقات",
                    "• أكبر مصدر إيراد، أعلى بند مصروف",
                    "",
                    "ملاحظات:",
                    "- الملف يحتوي على ورقة بيانات واحدة، وقد تم تعبئة أرقام الحسابات وأسمائها مسبقاً من شجرة الحسابات.",
                    "- املأ تاريخ نهاية الفترة (مثال: 30/09/2026) والفرع في كل صف. يمكن سحب الخلية لأسفل لتعبئة باقي الصفوف.",
                    "- اترك عمود الفرع فارغاً إذا كانت الشركة بفرع واحد، ويُسجل تلقائياً باسم \"فرع 1\".",
                    "- املأ المدين (Debit) والدائن (Credit) لحركة الفترة، والرصيد الافتتاحي (BBF) إن وُجد. الحركة = مدين − دائن، والرصيد = الافتتاحي + الحركة، ويُحسبان تلقائياً.",
                    "- عمود الرصيد (Balance) اختياري: يُستخدم فقط إذا لم تتوفر أرقام المدين والدائن.",
                    "- كل صف يُميَّز بـ (التاريخ + الفرع + رقم الحساب): إعادة رفع نفس الشهر تُحدّث الأرقام ولا تكررها.",
                    "- كل رقم حساب يجب أن يكون موجوداً في شجرة الحسابات، وإلا يُرفض الصف مع بيان رقم الحساب.",
                    "- مؤشرات الربحية تُجمع لكل الأشهر المرفوعة من السنة المالية (من بداية السنة حتى آخر شهر مرفوع)، لذلك يجب أن يحمل كل رفع حركة شهره فقط لا مجموعاً تراكمياً.",
                    "- مؤشرات المركز المالي (النقد، الذمم، المطلوبات) تُقرأ من أرصدة آخر تاريخ مرفوع، لأن المركز المالي لقطة في تاريخ محدد."
                });

            return File(bytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "Finance_Balances_Template.xlsx");
        }

        // The previous single-workbook template (COA + الحركات المالية in one file), kept for a
        // coordinator who still works from the old file.
        [HttpGet("template")]
        public async Task<IActionResult> DownloadTemplate()
        {
            var accounts = await _financeService.GetChartOfAccountsAsync();

            var bytes = ExcelTemplateWriter.Build(
                "النموذج المالي - شجرة الحسابات والأرصدة",
                new[]
                {
                    new ExcelTemplateSheetSpec
                    {
                        Definition = DepartmentTemplates.FinanceChartOfAccounts,
                        SheetName = "COA",
                        Rows = ChartOfAccountRows(accounts)
                    },
                    new ExcelTemplateSheetSpec
                    {
                        Definition = DepartmentTemplates.FinanceBalances,
                        SheetName = "الحركات المالية"
                    }
                },
                new[]
                {
                    "- هذا هو النموذج القديم الذي يحمل الورقتين في ملف واحد. النموذجان المعتمدان الآن منفصلان:",
                    "  شجرة الحسابات من صفحة \"شجرة الحسابات\"، والأرصدة الشهرية من صفحة \"الأرصدة والحركات المالية\".",
                    "- لا تحذف أي ورقة من هذا الملف، فكل ورقة تُقرأ بعنوانها."
                });

            return File(bytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "Finance_Template.xlsx");
        }

        private static List<IReadOnlyDictionary<string, string?>> ChartOfAccountRows(IEnumerable<ChartOfAccountDto> accounts) =>
            accounts.Select(a => new Dictionary<string, string?>
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
        #endregion

        #region The two uploads
        [HttpPost("chart-of-accounts/bulk-upload")]
        public async Task<IActionResult> BulkUploadChartOfAccounts(Microsoft.AspNetCore.Http.IFormFile file) =>
            await RunUploadAsync(file, stream => _financeService.BulkUploadChartOfAccountsAsync(stream));

        [HttpPost("balances/bulk-upload")]
        public async Task<IActionResult> BulkUploadBalances(Microsoft.AspNetCore.Http.IFormFile file) =>
            await RunUploadAsync(file, stream => _financeService.BulkUploadBalancesAsync(stream));

        // The previous single-workbook upload, kept working alongside the two above.
        [HttpPost("bulk-upload")]
        public async Task<IActionResult> BulkUpload(Microsoft.AspNetCore.Http.IFormFile file) =>
            await RunUploadAsync(file, stream => _financeService.BulkUploadFinanceWorkbookAsync(stream));

        private async Task<IActionResult> RunUploadAsync(
            Microsoft.AspNetCore.Http.IFormFile file,
            Func<System.IO.Stream, Task<ExcelImportResultDto>> import)
        {
            if (file == null || file.Length == 0) return BadRequest("لم يتم اختيار أي ملف.");
            if (!ExcelCompatibility.IsSupportedExcelFile(file.FileName, file.ContentType))
                return BadRequest("الملفات المدعومة هي .xlsx و .xls فقط.");

            using var rawStream = file.OpenReadStream();
            using var stream = ExcelCompatibility.EnsureXlsxStream(rawStream);
            var result = await import(stream);

            // A template/header problem is the uploader's mistake, not a server fault - report it
            // as 422 so the page can surface the message instead of a generic failure.
            if (!result.Success) return UnprocessableEntity(result);
            return Ok(result);
        }
        #endregion
    }
}
