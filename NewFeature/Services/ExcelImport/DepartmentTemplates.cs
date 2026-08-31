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

        // ─────────── Finance: one revenue/expense ledger ───────────
        public const string FinanceDate = "date";
        public const string FinanceStatement = "statement";
        public const string FinanceType = "type";
        public const string FinanceCategory = "category";
        public const string FinanceAmount = "amount";

        public static ExcelTemplateDefinition Finance { get; } = new()
        {
            TemplateName = "Finance Ledger (سجل الحركات المالية)",
            IdentityColumnKey = FinanceStatement,
            Columns = new List<ExcelColumnDefinition>
            {
                new() { Key = FinanceDate, DisplayName = "التاريخ", Required = true,
                        HeaderAliases = new[] { "التاريخ", "date" } },
                new() { Key = FinanceStatement, DisplayName = "البيان", Required = true,
                        HeaderAliases = new[] { "البيان", "statement", "description" } },
                new() { Key = FinanceType, DisplayName = "النوع", Required = true,
                        HeaderAliases = new[] { "النوع", "type" } },
                new() { Key = FinanceCategory, DisplayName = "التصنيف",
                        HeaderAliases = new[] { "التصنيف", "category" } },
                new() { Key = FinanceAmount, DisplayName = "المبلغ", Required = true,
                        HeaderAliases = new[] { "المبلغ", "amount" } }
            }
        };
    }
}
