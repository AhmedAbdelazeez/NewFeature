using System.Collections.Generic;

namespace NewFeature.Services.ExcelImport
{
    // The approved single-sheet upload templates for the four back-office departments
    // (Compliance, Project Management, HR, Finance). Each one is deliberately small: one sheet,
    // under ten columns, every column something a department coordinator can fill in by hand
    // without pulling a report out of another system. The KPIs each department shows on the
    // executive dashboard are computed only from what these sheets actually carry.
    //
    // The generated .xlsx files that match these definitions live in Templates/ and are served
    // to users from wwwroot/templates/.
    public static class DepartmentTemplates
    {
        // ─────────── Compliance: one violations log ───────────
        public const string ComplianceDescription = "description";
        public const string ComplianceDepartment = "department";
        public const string ComplianceClassification = "classification";
        public const string ComplianceSeverity = "severity";
        public const string ComplianceStatus = "status";
        public const string ComplianceDetectionDate = "detectionDate";
        public const string ComplianceClosureDate = "closureDate";
        public const string ComplianceFine = "fine";

        public static ExcelTemplateDefinition Compliance { get; } = new()
        {
            TemplateName = "Compliance Violations Log (سجل مخالفات الامتثال)",
            IdentityColumnKey = ComplianceDescription,
            Columns = new List<ExcelColumnDefinition>
            {
                new() { Key = ComplianceDescription, DisplayName = "وصف المخالفة", Required = true,
                        HeaderAliases = new[] { "وصف المخالفة", "violation description" } },
                new() { Key = ComplianceDepartment, DisplayName = "الإدارة", Required = true,
                        HeaderAliases = new[] { "الإدارة", "الادارة", "department" } },
                new() { Key = ComplianceClassification, DisplayName = "التصنيف",
                        HeaderAliases = new[] { "التصنيف", "classification" } },
                new() { Key = ComplianceSeverity, DisplayName = "درجة الخطورة",
                        HeaderAliases = new[] { "الخطورة", "severity" } },
                new() { Key = ComplianceStatus, DisplayName = "الحالة", Required = true,
                        HeaderAliases = new[] { "الحالة", "status" } },
                new() { Key = ComplianceDetectionDate, DisplayName = "تاريخ الرصد", Required = true,
                        HeaderAliases = new[] { "تاريخ الرصد", "detection date" } },
                new() { Key = ComplianceClosureDate, DisplayName = "تاريخ الإغلاق",
                        HeaderAliases = new[] { "تاريخ الإغلاق", "تاريخ الاغلاق", "closure date" } },
                new() { Key = ComplianceFine, DisplayName = "قيمة الغرامة",
                        HeaderAliases = new[] { "الغرامة", "fine" } }
            }
        };

        // ─────────── Project Management: one projects register ───────────
        public const string ProjectName = "projectName";
        public const string ProjectClient = "client";
        public const string ProjectStartDate = "startDate";
        public const string ProjectEndDate = "endDate";
        public const string ProjectStatus = "status";
        public const string ProjectContractValue = "contractValue";
        public const string ProjectVehicles = "vehicles";
        public const string ProjectTrips = "trips";

        public static ExcelTemplateDefinition Projects { get; } = new()
        {
            TemplateName = "Projects Register (سجل المشاريع)",
            IdentityColumnKey = ProjectName,
            Columns = new List<ExcelColumnDefinition>
            {
                new() { Key = ProjectName, DisplayName = "اسم المشروع", Required = true,
                        HeaderAliases = new[] { "اسم المشروع", "project name" } },
                new() { Key = ProjectClient, DisplayName = "العميل", Required = true,
                        HeaderAliases = new[] { "العميل", "client" } },
                new() { Key = ProjectStartDate, DisplayName = "تاريخ البداية", Required = true,
                        HeaderAliases = new[] { "تاريخ البداية", "start date" } },
                new() { Key = ProjectEndDate, DisplayName = "تاريخ النهاية", Required = true,
                        HeaderAliases = new[] { "تاريخ النهاية", "end date" } },
                new() { Key = ProjectStatus, DisplayName = "الحالة", Required = true,
                        HeaderAliases = new[] { "الحالة", "status" } },
                new() { Key = ProjectContractValue, DisplayName = "قيمة العقد",
                        HeaderAliases = new[] { "قيمة العقد", "contract value" } },
                new() { Key = ProjectVehicles, DisplayName = "عدد الحافلات",
                        HeaderAliases = new[] { "عدد الحافلات", "vehicles" } },
                new() { Key = ProjectTrips, DisplayName = "عدد الرحلات",
                        HeaderAliases = new[] { "عدد الرحلات", "trips" } }
            }
        };

        // ─────────── Human Resources: one employee roster ───────────
        public const string HrEmployeeName = "employeeName";
        public const string HrDepartment = "department";
        public const string HrJobTitle = "jobTitle";
        public const string HrPhone = "phone";
        public const string HrJoinDate = "joinDate";
        public const string HrNationality = "nationality";
        public const string HrEmploymentStatus = "employmentStatus";
        public const string HrSalary = "salary";
        public const string HrRating = "rating";

        public static ExcelTemplateDefinition Hr { get; } = new()
        {
            TemplateName = "Employee Roster (كشف الموظفين)",
            IdentityColumnKey = HrEmployeeName,
            Columns = new List<ExcelColumnDefinition>
            {
                new() { Key = HrEmployeeName, DisplayName = "اسم الموظف", Required = true,
                        HeaderAliases = new[] { "اسم الموظف", "employee name" } },
                new() { Key = HrDepartment, DisplayName = "القسم", Required = true,
                        HeaderAliases = new[] { "القسم", "الإدارة", "الادارة", "department" } },
                new() { Key = HrJobTitle, DisplayName = "المسمى الوظيفي",
                        HeaderAliases = new[] { "المسمى", "job title" } },
                new() { Key = HrPhone, DisplayName = "رقم الجوال",
                        HeaderAliases = new[] { "الجوال", "phone" } },
                // JoinDate is what makes the retention rate computable, so it is mandatory.
                new() { Key = HrJoinDate, DisplayName = "تاريخ التعيين", Required = true,
                        HeaderAliases = new[] { "تاريخ التعيين", "join date", "hire date" } },
                new() { Key = HrNationality, DisplayName = "الجنسية",
                        HeaderAliases = new[] { "الجنسية", "nationality" } },
                new() { Key = HrEmploymentStatus, DisplayName = "حالة الموظف", Required = true,
                        HeaderAliases = new[] { "حالة الموظف", "employment status" } },
                new() { Key = HrSalary, DisplayName = "الراتب",
                        HeaderAliases = new[] { "الراتب", "salary" } },
                new() { Key = HrRating, DisplayName = "التقييم",
                        HeaderAliases = new[] { "التقييم", "rating" } }
            }
        };

        // ─────────── Finance: two templates, the account tree and the monthly figures ───────────
        // The chart of accounts (COA) is reference data that changes a few times a year; the trial
        // balance arrives every month. They are uploaded as two separate files for exactly that
        // reason, each with its own template and its own page, and the account tree is what every
        // uploaded figure is classified through. FinanceChartOfAccounts/FinanceBalances below stay
        // sheet-targeted for the older single-workbook upload that carried both sheets at once;
        // FinanceChartOfAccountsFile/FinanceBalancesFile are the standalone ones, which also accept
        // a file whose sheet was renamed.
        public const string FinanceAccountNumber = "accountNumber";
        public const string FinanceAccountName = "accountName";
        public const string FinanceMapping = "mapping";
        public const string FinanceBsClassification = "bsClassification";
        public const string FinanceIsClassification = "isClassification";
        public const string FinanceRsmClassification = "rsmClassification";
        public const string FinanceManagementClassification = "managementClassification";
        public const string FinanceRevenueMainClassification = "revenueMainClassification";
        public const string FinanceRevenueSubClassification = "revenueSubClassification";

        // The account tree's columns, exactly as their own COA sheet spells them.
        private static List<ExcelColumnDefinition> ChartOfAccountColumns() => new()
        {
            new() { Key = FinanceAccountNumber, DisplayName = "رقم الحساب (Account #)", Required = true,
                    HeaderAliases = new[] { "Account #", "Account No", "Ledger account", "رقم الحساب" } },
            new() { Key = FinanceAccountName, DisplayName = "اسم الحساب (Account Name)", Required = true,
                    HeaderAliases = new[] { "Account Name", "اسم الحساب" } },
            // The Arabic grouping the balance-sheet KPI cards are built from ("نقد في الصندوق ولدى
            // البنوك", "ذمم مدينة بالصافي", "مصاريف مستحقة ومطلوبات اخرى"...).
            new() { Key = FinanceMapping, DisplayName = "التصنيف (Mapping)",
                    HeaderAliases = new[] { "Mapping" } },
            new() { Key = FinanceBsClassification, DisplayName = "تصنيف المركز المالي (BS Classification)",
                    HeaderAliases = new[] { "BS Classification" } },
            new() { Key = FinanceIsClassification, DisplayName = "تصنيف قائمة الدخل (IS Classification)",
                    HeaderAliases = new[] { "IS Classification" } },
            // The column every KPI bucket is derived from: its numeric prefix (5xxx assets, 6xxx
            // liabilities/equity, 7000 revenue, 71xx cost of sales, 72xx operating costs) is what
            // turns a raw balance into a revenue, a cost or a balance-sheet figure. Not marked
            // mandatory because their own COA leaves it blank on a few accounts that BS/IS
            // Classification still classifies, and the importer accepts any one of the three.
            new() { Key = FinanceRsmClassification, DisplayName = "تصنيف RSM (RSM Classification) - أو BS/IS Classification بدلاً منه",
                    HeaderAliases = new[] { "RSM Classification" } },
            new() { Key = FinanceManagementClassification, DisplayName = "التصنيف الإداري (Management Classification)",
                    HeaderAliases = new[] { "Management Classification" } },
            new() { Key = FinanceRevenueMainClassification, DisplayName = "التصنيف الرئيسي للإيرادات",
                    HeaderAliases = new[] { "Revenues Main Classification" } },
            new() { Key = FinanceRevenueSubClassification, DisplayName = "التصنيف الفرعي للإيرادات",
                    HeaderAliases = new[] { "Revenues Sub Classification" } }
        };

        public static ExcelTemplateDefinition FinanceChartOfAccounts { get; } = new()
        {
            TemplateName = "نموذج شجرة الحسابات (COA)",
            SheetNameAliases = new[] { "COA", "شجرة الحسابات" },
            SheetDisplayName = "COA",
            IdentityColumnKey = FinanceAccountNumber,
            Columns = ChartOfAccountColumns()
        };

        // The standalone COA upload: the same contract, but a file whose single sheet was renamed
        // (the accountant's own export often is) is read rather than rejected.
        public static ExcelTemplateDefinition FinanceChartOfAccountsFile { get; } = new()
        {
            TemplateName = "نموذج شجرة الحسابات (COA)",
            SheetNameAliases = new[] { "COA", "شجرة الحسابات" },
            SheetDisplayName = "COA",
            AllowFirstSheetFallback = true,
            IdentityColumnKey = FinanceAccountNumber,
            Columns = ChartOfAccountColumns()
        };

        public const string FinanceBalanceDate = "balanceDate";
        public const string FinanceBalanceBranch = "balanceBranch";
        public const string FinanceBalanceAccountNumber = "balanceAccountNumber";
        public const string FinanceBalanceAccountName = "balanceAccountName";
        public const string FinanceBalanceOpening = "balanceOpening";
        public const string FinanceBalanceDebit = "balanceDebit";
        public const string FinanceBalanceCredit = "balanceCredit";
        public const string FinanceBalanceAmount = "balance";

        // The monthly figures, mirroring the trial balance their accounting system exports: an
        // opening balance, the period's debits and credits. الحركة (Debit - Credit) and الرصيد
        // (BBF + الحركة) are computed on our side rather than typed, so a hand-edited total can
        // never contradict the debits and credits it was supposed to summarize. The Balance column
        // is still read when a file carries only a closing figure, which is what the first version
        // of this template asked for.
        private static List<ExcelColumnDefinition> BalanceColumns() => new()
        {
            new() { Key = FinanceBalanceDate, DisplayName = "تاريخ نهاية الفترة (Date)", Required = true,
                    HeaderAliases = new[] { "Date", "التاريخ" } },
            new() { Key = FinanceBalanceBranch, DisplayName = "الفرع (Branch) - يُترك فارغاً للفرع الواحد",
                    HeaderAliases = new[] { "Branch", "الفرع" } },
            new() { Key = FinanceBalanceAccountNumber, DisplayName = "رقم الحساب (Account No)", Required = true,
                    HeaderAliases = new[] { "Account No", "Account #", "Ledger account", "رقم الحساب" } },
            new() { Key = FinanceBalanceAccountName, DisplayName = "اسم الحساب (Account Name) - يُملأ من الشجرة إن تُرك فارغاً",
                    HeaderAliases = new[] { "Account Name", "اسم الحساب" } },
            new() { Key = FinanceBalanceOpening, DisplayName = "الرصيد الافتتاحي (BBF)",
                    HeaderAliases = new[] { "BBF", "Opening Balance", "الرصيد الافتتاحي" } },
            new() { Key = FinanceBalanceDebit, DisplayName = "مدين (Debit)",
                    HeaderAliases = new[] { "Debit", "مدين" } },
            new() { Key = FinanceBalanceCredit, DisplayName = "دائن (Credit)",
                    HeaderAliases = new[] { "Credit", "دائن" } },
            new() { Key = FinanceBalanceAmount, DisplayName = "الرصيد (Balance) - يُحسب تلقائياً إن تُرك فارغاً",
                    HeaderAliases = new[] { "Balance", "الرصيد" } }
        };

        public static ExcelTemplateDefinition FinanceBalances { get; } = new()
        {
            TemplateName = "نموذج الأرصدة والحركات المالية",
            SheetNameAliases = new[] { "الحركات المالية", "Balances", "Trial Balance", "TB" },
            SheetDisplayName = "الحركات المالية",
            IdentityColumnKey = FinanceBalanceAccountNumber,
            Columns = BalanceColumns()
        };

        // The standalone monthly upload.
        public static ExcelTemplateDefinition FinanceBalancesFile { get; } = new()
        {
            TemplateName = "نموذج الأرصدة والحركات المالية",
            SheetNameAliases = new[] { "الحركات المالية", "Balances", "Trial Balance", "TB" },
            SheetDisplayName = "الحركات المالية",
            AllowFirstSheetFallback = true,
            IdentityColumnKey = FinanceBalanceAccountNumber,
            Columns = BalanceColumns()
        };

        // ─────────── Sales: three templates, one page each ───────────
        // The customer roster (one sheet per fiscal year, the sheet name IS the year), the fleet
        // capacity snapshot and the daily operations log. Each is uploaded on its own page from its
        // own file; the combined workbook (all three sheets in one file) is still accepted. Headers
        // are the English ones Sales already uses in its approved workbook; the first alias is what
        // the downloadable template writes, the others are the Arabic spellings of their older AX /
        // operations-room exports. SalesService resolves them by exact name first, then by keyword.
        public const string SalesCustomerCode = "customerCode";
        public const string SalesCustomerName = "customerName";
        public const string SalesCustomerGroup = "customerGroup";
        public const string SalesCurrency = "currency";

        public static ExcelTemplateDefinition SalesCustomers { get; } = new()
        {
            TemplateName = "نموذج قائمة العملاء (Customer Roster)",
            IdentityColumnKey = SalesCustomerCode,
            Columns = new List<ExcelColumnDefinition>
            {
                new() { Key = SalesCustomerCode, DisplayName = "رقم حساب العميل (Order Account)", Required = true,
                        HeaderAliases = new[] { "Order Account", "حساب العميل", "رقم الحساب" } },
                new() { Key = SalesCustomerName, DisplayName = "اسم العميل (Name)", Required = true,
                        HeaderAliases = new[] { "Name", "اسم العميل", "أسم العميل", "Customer Name" } },
                new() { Key = SalesCustomerGroup, DisplayName = "فئة العميل (Customer Group) مثل LCLCONT / FRNHAJ",
                        HeaderAliases = new[] { "Customer Group", "فئة العميل", "مجموعة العملاء" } },
                new() { Key = SalesCurrency, DisplayName = "العملة (Currency)",
                        HeaderAliases = new[] { "Currency", "العملة" } }
            }
        };

        public const string SalesBusCode = "busCode";
        public const string SalesBusType = "busType";
        public const string SalesCategory = "category";
        public const string SalesModelYear = "modelYear";
        public const string SalesNumberOfBuses = "numberOfBuses";
        public const string SalesSeatsPerBus = "seatsPerBus";
        public const string SalesTotalSeats = "totalSeats";

        public static ExcelTemplateDefinition SalesFleetCapacity { get; } = new()
        {
            TemplateName = "نموذج الطاقة الاستيعابية للأسطول (Fleet Capacity)",
            SheetDisplayName = "Fleet Capacity",
            IdentityColumnKey = SalesBusType,
            Columns = new List<ExcelColumnDefinition>
            {
                new() { Key = SalesBusCode, DisplayName = "كود الحافلة (Bus Code)",
                        HeaderAliases = new[] { "Bus Code", "كود الحافلة", "الحافلة" } },
                new() { Key = SalesBusType, DisplayName = "نوع الحافلة / الشركة المصنعة (Bus Type)", Required = true,
                        HeaderAliases = new[] { "Bus Type", "نوع الحافلة" } },
                new() { Key = SalesCategory, DisplayName = "الفئة (Category) مثل Coach / VIP",
                        HeaderAliases = new[] { "Category", "الفئة" } },
                new() { Key = SalesModelYear, DisplayName = "سنة الموديل (Model Year)",
                        HeaderAliases = new[] { "Model Year", "الموديل" } },
                new() { Key = SalesNumberOfBuses, DisplayName = "عدد الحافلات (Number of Buses)", Required = true,
                        HeaderAliases = new[] { "Number of Buses", "عدد الحافلات" } },
                new() { Key = SalesSeatsPerBus, DisplayName = "عدد المقاعد للحافلة (Seats per Bus)", Required = true,
                        HeaderAliases = new[] { "Seats per Bus", "عدد المقاعد" } },
                new() { Key = SalesTotalSeats, DisplayName = "إجمالي المقاعد (Total Seats) - يُحسب تلقائياً إن تُرك فارغاً",
                        HeaderAliases = new[] { "Total Seats", "اجمالي المقاعد", "إجمالي المقاعد" } }
            }
        };

        public const string SalesRentalOrderNumber = "rentalOrderNumber";
        public const string SalesConfirmationNumber = "confirmationNumber";
        public const string SalesRequestType = "requestType";
        public const string SalesOpsCustomerName = "customerName";
        public const string SalesExecutionPoint = "executionPoint";
        public const string SalesDirection = "direction";
        public const string SalesBusTypeCode = "busTypeCode";
        public const string SalesOperationalCount = "operationalCount";
        public const string SalesScheduledBuses = "scheduledBuses";
        public const string SalesExecutionDate = "executionDate";
        public const string SalesExecutionTime = "executionTime";
        public const string SalesNotes = "notes";

        public static ExcelTemplateDefinition SalesDailyOperations { get; } = new()
        {
            TemplateName = "نموذج التشغيل اليومي (Daily Operations)",
            SheetDisplayName = "Daily Operations",
            IdentityColumnKey = SalesOpsCustomerName,
            Columns = new List<ExcelColumnDefinition>
            {
                new() { Key = SalesRentalOrderNumber, DisplayName = "رقم أمر الإيجار (Rental Order Number)",
                        HeaderAliases = new[] { "Rental Order Number", "رقم أمر الايجار", "رقم امر الايجار", "Rental Order" } },
                new() { Key = SalesConfirmationNumber, DisplayName = "رقم التعميد (Confirmation Number)",
                        HeaderAliases = new[] { "Confirmation Number", "رقم التعميد" } },
                new() { Key = SalesRequestType, DisplayName = "نوع الطلب (Request Type) خارجي / داخلي",
                        HeaderAliases = new[] { "Request Type", "طلب العميل" } },
                new() { Key = SalesOpsCustomerName, DisplayName = "اسم العميل (Customer Name)", Required = true,
                        HeaderAliases = new[] { "Customer Name", "أسم العميل", "اسم العميل" } },
                new() { Key = SalesExecutionPoint, DisplayName = "المنفذ (Execution Point)",
                        HeaderAliases = new[] { "Execution Point", "المنفذ" } },
                new() { Key = SalesDirection, DisplayName = "الاتجاه (Direction)",
                        HeaderAliases = new[] { "Direction", "الاتجاه", "كود الاتجاة" } },
                new() { Key = SalesBusTypeCode, DisplayName = "نوع الحافلة (Bus Type Code)",
                        HeaderAliases = new[] { "Bus Type Code", "نوع الحافلة" } },
                new() { Key = SalesOperationalCount, DisplayName = "عدد الحافلات المطلوبة (Operational Count)", Required = true,
                        HeaderAliases = new[] { "Operational Count", "العدد التشغيلى", "العدد التشغيلي" } },
                new() { Key = SalesScheduledBuses, DisplayName = "الحافلات المجدولة (Scheduled Buses) - يُترك فارغاً إن لم تُجدول",
                        Required = true,
                        HeaderAliases = new[] { "Scheduled Buses", "الحافلات المجدولة" } },
                new() { Key = SalesExecutionDate, DisplayName = "تاريخ التنفيذ (Execution Date)", Required = true,
                        HeaderAliases = new[] { "Execution Date", "تاريخ التنفيذ" } },
                new() { Key = SalesExecutionTime, DisplayName = "وقت التنفيذ (Execution Time) مثل 06:00",
                        HeaderAliases = new[] { "Execution Time", "وقت التنفيذ" } },
                new() { Key = SalesNotes, DisplayName = "ملاحظات (Notes)",
                        HeaderAliases = new[] { "Notes", "ملاحظات" } }
            }
        };

        // ─────────── Maintenance: one internal work-orders log ───────────
        // Mirrors the workshop's own "Internal work orders" sheet exactly. Technicians 2-4 are the
        // extra hands assigned to the same job and are left optional, because most work orders are
        // closed by one technician and forcing three more names would only push coordinators into
        // typing filler.
        public const string MaintenanceWorkOrderNumber = "workOrderNumber";
        public const string MaintenanceBusNumber = "busNumber";
        public const string MaintenanceOdometer = "odometer";
        public const string MaintenanceBreakdownDescription = "breakdownDescription";
        public const string MaintenanceDateIn = "dateIn";
        public const string MaintenanceTimeIn = "timeIn";
        public const string MaintenanceDateOut = "dateOut";
        public const string MaintenanceTimeOut = "timeOut";
        public const string MaintenanceTechnician1 = "technician1";
        public const string MaintenanceTechnician2 = "technician2";
        public const string MaintenanceTechnician3 = "technician3";
        public const string MaintenanceTechnician4 = "technician4";
        public const string MaintenanceStatus = "status";
        public const string MaintenanceSupervisor = "supervisor";
        public const string MaintenanceNotes = "notes";

        public static ExcelTemplateDefinition Maintenance { get; } = new()
        {
            TemplateName = "سجل أوامر العمل الداخلية (Internal Work Orders)",
            SheetNameAliases = new[] { "Internal work orders", "أوامر العمل" },
            SheetDisplayName = "Internal work orders",
            IdentityColumnKey = MaintenanceWorkOrderNumber,
            Columns = new List<ExcelColumnDefinition>
            {
                new() { Key = MaintenanceWorkOrderNumber, DisplayName = "رقم امر العمل", Required = true,
                        HeaderAliases = new[] { "رقم امر العمل", "رقم أمر العمل", "work order" } },
                new() { Key = MaintenanceBusNumber, DisplayName = "رقم الحافلة", Required = true,
                        HeaderAliases = new[] { "رقم الحافلة", "رقم الحافغلة", "bus number", "bus no" } },
                new() { Key = MaintenanceOdometer, DisplayName = "عداد كم الحالي",
                        HeaderAliases = new[] { "عداد", "odometer", "km" } },
                new() { Key = MaintenanceBreakdownDescription, DisplayName = "وصف العطل", Required = true,
                        HeaderAliases = new[] { "وصف العطل", "breakdown", "fault" } },
                new() { Key = MaintenanceDateIn, DisplayName = "التاريخ الدخول", Required = true,
                        HeaderAliases = new[] { "التاريخ الدخول", "تاريخ الدخول", "date in" } },
                new() { Key = MaintenanceTimeIn, DisplayName = "ساعة الدخول",
                        HeaderAliases = new[] { "ساعة الدخول", "time in" } },
                new() { Key = MaintenanceDateOut, DisplayName = "تاريخ الخروج",
                        HeaderAliases = new[] { "تاريخ الخروج", "التاريخ الخروج", "date out" } },
                new() { Key = MaintenanceTimeOut, DisplayName = "ساعة الخروج",
                        HeaderAliases = new[] { "ساعة الخروج", "time out" } },
                new() { Key = MaintenanceTechnician1, DisplayName = "اسم الفنى 1", Required = true,
                        HeaderAliases = new[] { "اسم الفنى 1", "اسم الفني 1", "technician 1" } },
                new() { Key = MaintenanceTechnician2, DisplayName = "اسم الفنى 2",
                        HeaderAliases = new[] { "اسم الفنى 2", "اسم الفني 2", "technician 2" } },
                new() { Key = MaintenanceTechnician3, DisplayName = "اسم الفنى 3",
                        HeaderAliases = new[] { "اسم الفنى 3", "اسم الفني 3", "technician 3" } },
                new() { Key = MaintenanceTechnician4, DisplayName = "اسم الفنى 4",
                        HeaderAliases = new[] { "اسم الفنى 4", "اسم الفني 4", "technician 4" } },
                new() { Key = MaintenanceStatus, DisplayName = "حالة", Required = true,
                        HeaderAliases = new[] { "حالة", "الحالة", "status" } },
                new() { Key = MaintenanceSupervisor, DisplayName = "اسم المشرف",
                        HeaderAliases = new[] { "اسم المشرف", "المشرف", "supervisor" } },
                new() { Key = MaintenanceNotes, DisplayName = "ملاحظات",
                        HeaderAliases = new[] { "ملاحظات", "ملاحظة", "notes", "remarks" } }
            }
        };

        // ─────────── Operations: one dispatch/rental-order log ───────────
        // One row per bus assigned to a rental order on a given day - the sheet the operations
        // room already produces. Every Operations KPI on the executive dashboard is a count, a
        // distinct-count or a sum over exactly these columns.
        public const string OperationsDirection = "direction";
        public const string OperationsDirectionName = "directionName";
        public const string OperationsRentOrder = "rentOrder";
        public const string OperationsCustomerAccount = "customerAccount";
        public const string OperationsCustomerName = "customerName";
        public const string OperationsBusType = "busType";
        public const string OperationsBusNumber = "busNumber";
        public const string OperationsDeliveryDate = "deliveryDate";
        public const string OperationsExecuteTime = "executeTime";
        public const string OperationsPlannedStart = "plannedStart";
        public const string OperationsPlannedEnd = "plannedEnd";
        public const string OperationsDriverNo = "driverNo";
        public const string OperationsDriverName = "driverName";
        public const string OperationsAddDriverNo = "addDriverNo";
        public const string OperationsAddDriverName = "addDriverName";
        public const string OperationsLocation = "location";
        public const string OperationsFromLocation = "fromLocation";
        public const string OperationsToLocation = "toLocation";
        public const string OperationsActualKm = "actualKm";
        public const string OperationsDiesel = "diesel";
        public const string OperationsPlannedKm = "plannedKm";
        public const string OperationsCompletion = "completion";

        public static ExcelTemplateDefinition Operations { get; } = new()
        {
            TemplateName = "سجل أوامر التشغيل (Operations Dispatch Log)",
            IdentityColumnKey = OperationsRentOrder,
            Columns = new List<ExcelColumnDefinition>
            {
                // Several headers in this sheet are prefixes of others ("Direction" / "Direction
                // Name", "Location" / "From Location", "Driver Name" / "ADD. Driver Name"). The
                // engine resolves those by exact header name first, which is why each alias list
                // below spells the header out in full rather than relying on a keyword.
                new() { Key = OperationsDirection, DisplayName = "Direction (كود الخط)", Required = true,
                        HeaderAliases = new[] { "Direction", "كود الخط" } },
                new() { Key = OperationsDirectionName, DisplayName = "Direction Name (اسم الخط)",
                        HeaderAliases = new[] { "Direction Name", "اسم الخط" } },
                new() { Key = OperationsRentOrder, DisplayName = "Rent Order (أمر الإيجار)", Required = true,
                        HeaderAliases = new[] { "Rent Order", "أمر الايجار", "امر الايجار" } },
                new() { Key = OperationsCustomerAccount, DisplayName = "Customer account (حساب العميل)",
                        HeaderAliases = new[] { "Customer account", "حساب العميل" } },
                new() { Key = OperationsCustomerName, DisplayName = "Name (اسم العميل)", Required = true,
                        HeaderAliases = new[] { "Name", "اسم العميل" } },
                new() { Key = OperationsBusType, DisplayName = "Bus Type (نوع الحافلة)",
                        HeaderAliases = new[] { "Bus Type", "نوع الحافلة" } },
                new() { Key = OperationsBusNumber, DisplayName = "Bus number (رقم الحافلة)", Required = true,
                        HeaderAliases = new[] { "Bus number", "Bus No", "رقم الحافلة" } },
                new() { Key = OperationsDeliveryDate, DisplayName = "DELV. Date (تاريخ التنفيذ)", Required = true,
                        HeaderAliases = new[] { "DELV. Date", "DELV Date", "تاريخ التنفيذ" } },
                new() { Key = OperationsExecuteTime, DisplayName = "Execute Time (وقت التنفيذ)",
                        HeaderAliases = new[] { "Execute Time", "وقت التنفيذ" } },
                new() { Key = OperationsPlannedStart, DisplayName = "PlannedStart (بداية مخططة)",
                        HeaderAliases = new[] { "PlannedStart", "Planned Start" } },
                new() { Key = OperationsPlannedEnd, DisplayName = "PlannedEnd (نهاية مخططة)",
                        HeaderAliases = new[] { "PlannedEnd", "Planned End" } },
                new() { Key = OperationsDriverNo, DisplayName = "Driver NO. (رقم السائق)",
                        HeaderAliases = new[] { "Driver NO.", "Driver NO", "رقم السائق" } },
                new() { Key = OperationsDriverName, DisplayName = "Driver Name (اسم السائق)", Required = true,
                        HeaderAliases = new[] { "Driver Name", "اسم السائق" } },
                new() { Key = OperationsAddDriverNo, DisplayName = "ADD. Driver (رقم السائق المساعد)",
                        HeaderAliases = new[] { "ADD. Driver", "رقم السائق المساعد" } },
                new() { Key = OperationsAddDriverName, DisplayName = "ADD. Driver Name (اسم السائق المساعد)",
                        HeaderAliases = new[] { "ADD. Driver Name", "اسم السائق المساعد" } },
                new() { Key = OperationsLocation, DisplayName = "Location (الموقع)",
                        HeaderAliases = new[] { "Location" } },
                new() { Key = OperationsFromLocation, DisplayName = "From Location (من)",
                        HeaderAliases = new[] { "From Location" } },
                new() { Key = OperationsToLocation, DisplayName = "To Location (إلى)",
                        HeaderAliases = new[] { "To Location" } },
                new() { Key = OperationsActualKm, DisplayName = "ActualKM (كم فعلي)",
                        HeaderAliases = new[] { "ActualKM", "Actual KM" } },
                new() { Key = OperationsDiesel, DisplayName = "Desiel (الديزل)",
                        HeaderAliases = new[] { "Desiel", "Diesel" } },
                new() { Key = OperationsPlannedKm, DisplayName = "PlannedKM (كم مخطط)",
                        HeaderAliases = new[] { "PlannedKM", "Planned KM" } },
                new() { Key = OperationsCompletion, DisplayName = "Completeion (حالة التنفيذ)", Required = true,
                        HeaderAliases = new[] { "Completeion", "Completion", "حالة التنفيذ" } }
            }
        };
    }
}
