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

        // ─────────── Finance: one workbook, chart of accounts + monthly balances ───────────
        // Sheet 1 (COA) is the reference tree every balance is classified through; sheet 2
        // (الحركات المالية) carries one row per account balance per reporting date. Both live in
        // the same approved file, so a month's upload always arrives with the account tree that
        // explains it and no balance can land unclassified.
        public const string FinanceAccountNumber = "accountNumber";
        public const string FinanceAccountName = "accountName";
        public const string FinanceMapping = "mapping";
        public const string FinanceBsClassification = "bsClassification";
        public const string FinanceIsClassification = "isClassification";
        public const string FinanceRsmClassification = "rsmClassification";
        public const string FinanceManagementClassification = "managementClassification";
        public const string FinanceRevenueMainClassification = "revenueMainClassification";
        public const string FinanceRevenueSubClassification = "revenueSubClassification";

        public static ExcelTemplateDefinition FinanceChartOfAccounts { get; } = new()
        {
            TemplateName = "النموذج المالي (شجرة الحسابات والأرصدة)",
            SheetNameAliases = new[] { "COA", "شجرة الحسابات" },
            SheetDisplayName = "COA",
            IdentityColumnKey = FinanceAccountNumber,
            Columns = new List<ExcelColumnDefinition>
            {
                new() { Key = FinanceAccountNumber, DisplayName = "رقم الحساب (Account #)", Required = true,
                        HeaderAliases = new[] { "Account #", "Account No", "رقم الحساب" } },
                new() { Key = FinanceAccountName, DisplayName = "اسم الحساب (Account Name)", Required = true,
                        HeaderAliases = new[] { "Account Name", "اسم الحساب" } },
                new() { Key = FinanceMapping, DisplayName = "التصنيف (Mapping)",
                        HeaderAliases = new[] { "Mapping" } },
                new() { Key = FinanceBsClassification, DisplayName = "تصنيف المركز المالي (BS Classification)",
                        HeaderAliases = new[] { "BS Classification" } },
                new() { Key = FinanceIsClassification, DisplayName = "تصنيف قائمة الدخل (IS Classification)",
                        HeaderAliases = new[] { "IS Classification" } },
                // The one column every KPI bucket is derived from: its numeric prefix (5xxx assets,
                // 6xxx liabilities/equity, 7000 revenue, 71xx cost of sales, 72xx operating costs)
                // is what turns a raw balance into a revenue, a cost or a balance-sheet figure.
                new() { Key = FinanceRsmClassification, DisplayName = "تصنيف RSM (RSM Classification)", Required = true,
                        HeaderAliases = new[] { "RSM Classification" } },
                new() { Key = FinanceManagementClassification, DisplayName = "التصنيف الإداري (Management Classification)",
                        HeaderAliases = new[] { "Management Classification" } },
                new() { Key = FinanceRevenueMainClassification, DisplayName = "التصنيف الرئيسي للإيرادات",
                        HeaderAliases = new[] { "Revenues Main Classification" } },
                new() { Key = FinanceRevenueSubClassification, DisplayName = "التصنيف الفرعي للإيرادات",
                        HeaderAliases = new[] { "Revenues Sub Classification" } }
            }
        };

        public const string FinanceBalanceDate = "balanceDate";
        public const string FinanceBalanceAccountNumber = "balanceAccountNumber";
        public const string FinanceBalanceAccountName = "balanceAccountName";
        public const string FinanceBalanceAmount = "balance";

        public static ExcelTemplateDefinition FinanceBalances { get; } = new()
        {
            TemplateName = "النموذج المالي (شجرة الحسابات والأرصدة)",
            SheetNameAliases = new[] { "الحركات المالية", "Balances", "Trial Balance" },
            SheetDisplayName = "الحركات المالية",
            IdentityColumnKey = FinanceBalanceAccountNumber,
            Columns = new List<ExcelColumnDefinition>
            {
                new() { Key = FinanceBalanceDate, DisplayName = "التاريخ (Date)", Required = true,
                        HeaderAliases = new[] { "Date", "التاريخ" } },
                new() { Key = FinanceBalanceAccountNumber, DisplayName = "رقم الحساب (Account No)", Required = true,
                        HeaderAliases = new[] { "Account No", "Account #", "رقم الحساب" } },
                new() { Key = FinanceBalanceAccountName, DisplayName = "اسم الحساب (Account Name)",
                        HeaderAliases = new[] { "Account Name", "اسم الحساب" } },
                new() { Key = FinanceBalanceAmount, DisplayName = "الرصيد (Balance)", Required = true,
                        HeaderAliases = new[] { "Balance", "الرصيد" } }
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
