using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NewFeature.Models;

namespace NewFeature.Services.Repositories
{
    public static class DbInitializer
    {
        public static async System.Threading.Tasks.Task SeedAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            string[] roleNames = { "Admin", "Driver", "Project Manager" };
            foreach (var roleName in roleNames)
            {
                var roleExist = await roleManager.RoleExistsAsync(roleName);
                if (!roleExist)
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            var adminEmail = "admin@company.com";
            var defaultAdmin = await userManager.FindByEmailAsync(adminEmail);
            if (defaultAdmin == null)
            {
                var adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FullNameEn = "System Admin",
                    FullNameAr = "مدير النظام",
                    IsActive = true,
                    EmailConfirmed = true
                };

                var createPowerUser = await userManager.CreateAsync(adminUser, "Admin@123");
                if (createPowerUser.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }

            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();

            // Seed Client, Contract, and ContractItems if none exist
            if (!context.Clients.Any())
            {
                var client = new Client
                {
                    NameEn = "SABIC",
                    NameAr = "سابك",
                    Code = "SABIC001",
                    Email = "sabic@company.com",
                    Phone = "+966112253366"
                };
                context.Clients.Add(client);
                context.SaveChanges();

                var contract = new Contract
                {
                    ClientId = client.Id,
                    ContractNumber = "CONT-2026-001",
                    TitleEn = "Enterprise Transport Agreement",
                    TitleAr = "اتفاقية النقل الشاملة",
                    StartDate = DateTime.UtcNow.AddMonths(-1),
                    EndDate = DateTime.UtcNow.AddMonths(11),
                    TotalAmount = 500000
                };
                context.Contracts.Add(contract);
                context.SaveChanges();

                var contractItems = new List<ContractItem>
                {
                    new ContractItem
                    {
                        ContractId = contract.Id,
                        DescriptionEn = "Regular Fleet Dispatching Service",
                        DescriptionAr = "خدمة جدولة وإرسال الأسطول اليومي",
                        Quantity = 12,
                        UnitPrice = 30000,
                        TotalPrice = 360000,
                        IsCompliant = true
                    },
                    new ContractItem
                    {
                        ContractId = contract.Id,
                        DescriptionEn = "On-Demand VIP Transport Services",
                        DescriptionAr = "خدمات النقل المتميزة عند الطلب",
                        Quantity = 20,
                        UnitPrice = 5000,
                        TotalPrice = 100000,
                        IsCompliant = true
                    },
                    new ContractItem
                    {
                        ContractId = contract.Id,
                        DescriptionEn = "Vehicle mechanical inspection reports",
                        DescriptionAr = "تقارير الفحص الميكانيكي الدوري للمركبات",
                        Quantity = 4,
                        UnitPrice = 10000,
                        TotalPrice = 40000,
                        IsCompliant = false
                    }
                };
                context.ContractItems.AddRange(contractItems);
                context.SaveChanges();
            }

            // Seed Departments if empty
            if (!context.Departments.Any())
            {
                var departments = new List<Department>
                {
                    new Department { NameEn = "Human Resources", NameAr = "الموارد البشرية", Code = "HR", IsCompliant = true },
                    new Department { NameEn = "Finance & Accounting", NameAr = "المالية والمحاسبة", Code = "FIN", IsCompliant = true },
                    new Department { NameEn = "Information Technology", NameAr = "تقنية المعلومات", Code = "IT", IsCompliant = true },
                    new Department { NameEn = "Operations & Logistics", NameAr = "العمليات واللوجستيات", Code = "OPS", IsCompliant = false },
                    new Department { NameEn = "Compliance & Legal", NameAr = "الامتثال والشؤون القانونية", Code = "COMP", IsCompliant = true }
                };
                context.Departments.AddRange(departments);
                context.SaveChanges();
            }

            // Seed classifications if empty
            if (!context.ViolationClassifications.Any())
            {
                var classifications = new List<ViolationClassification>
                {
                    new ViolationClassification { NameEn = "Regulatory / Government", NameAr = "تنظيمية / حكومية", Code = "REG" },
                    new ViolationClassification { NameEn = "Internal Policies", NameAr = "سياسات داخلية", Code = "POL" },
                    new ViolationClassification { NameEn = "Operational & Safety", NameAr = "تشغيلية وسلامة", Code = "OPS" }
                };
                context.ViolationClassifications.AddRange(classifications);
                context.SaveChanges();
            }

            // Seed Violations if empty
            if (!context.Violations.Any())
            {
                var hrDept = context.Departments.First(d => d.Code == "HR");
                var opsDept = context.Departments.First(d => d.Code == "OPS");
                var finDept = context.Departments.First(d => d.Code == "FIN");

                var regClass = context.ViolationClassifications.First(c => c.Code == "REG");
                var polClass = context.ViolationClassifications.First(c => c.Code == "POL");
                var opsClass = context.ViolationClassifications.First(c => c.Code == "OPS");

                var now = DateTime.UtcNow;

                var violations = new List<Violation>
                {
                    new Violation
                    {
                        TitleEn = "Delayed Municipality Permit Renewal",
                        TitleAr = "تأخير تجديد تصريح البلدية",
                        DescriptionEn = "Failure to renew municipal license before expiration date.",
                        DescriptionAr = "عدم تجديد رخصة البلدية قبل تاريخ الانتهاء.",
                        DetectionDate = now.AddDays(-15),
                        ResolutionDate = now.AddDays(-10),
                        Status = ViolationStatus.Closed,
                        Severity = ViolationSeverity.Major,
                        ClassificationId = regClass.Id,
                        DepartmentId = opsDept.Id,
                        FineAmount = 2000
                    },
                    new Violation
                    {
                        TitleEn = "Unapproved Software Installation",
                        TitleAr = "تثبيت برامج غير مصرحة",
                        DescriptionEn = "User installed unauthorized software on work laptop.",
                        DescriptionAr = "قام مستخدم بتثبيت برامج غير مصرح بها على كمبيوتر العمل.",
                        DetectionDate = now.AddDays(-10),
                        ResolutionDate = null,
                        Status = ViolationStatus.Open,
                        Severity = ViolationSeverity.Minor,
                        ClassificationId = polClass.Id,
                        DepartmentId = hrDept.Id,
                        FineAmount = 0
                    },
                    new Violation
                    {
                        TitleEn = "Safety Gear Non-compliance",
                        TitleAr = "عدم الالتزام بمعدات السلامة",
                        DescriptionEn = "Warehouse staff caught working without helmet and safety shoes.",
                        DescriptionAr = "تم رصد موظفي المستودع يعملون بدون خوذة وحذاء السلامة.",
                        DetectionDate = now.AddDays(-5),
                        ResolutionDate = now.AddDays(-3),
                        Status = ViolationStatus.Closed,
                        Severity = ViolationSeverity.Major,
                        ClassificationId = opsClass.Id,
                        DepartmentId = opsDept.Id,
                        FineAmount = 1000
                    },
                    new Violation
                    {
                        TitleEn = "Data Privacy Non-compliance",
                        TitleAr = "مخالفة سرية البيانات والمعلومات",
                        DescriptionEn = "Sensitive client data shared over unencrypted channels.",
                        DescriptionAr = "تم مشاركة بيانات عملاء حساسة عبر قنوات غير مشفرة.",
                        DetectionDate = now.AddDays(-20),
                        ResolutionDate = null,
                        Status = ViolationStatus.Open,
                        Severity = ViolationSeverity.Critical,
                        ClassificationId = regClass.Id,
                        DepartmentId = finDept.Id,
                        FineAmount = 5000
                    },
                    new Violation
                    {
                        TitleEn = "Failure to Submit Monthly Log",
                        TitleAr = "عدم تقديم السجل الشهري",
                        DescriptionEn = "Failure to submit the monthly operations reports on time.",
                        DescriptionAr = "تأخر في تسليم سجل التقارير الشهرية في الموعد المحدد.",
                        DetectionDate = now.AddDays(-12),
                        ResolutionDate = now.AddDays(-4),
                        Status = ViolationStatus.Closed,
                        Severity = ViolationSeverity.Minor,
                        ClassificationId = opsClass.Id,
                        DepartmentId = opsDept.Id,
                        FineAmount = 500
                    }
                };
                context.Violations.AddRange(violations);
                context.SaveChanges();
            }

            // Seed Audits if empty
            if (!context.InternalAudits.Any())
            {
                var hrDept = context.Departments.First(d => d.Code == "HR");
                var finDept = context.Departments.First(d => d.Code == "FIN");
                var opsDept = context.Departments.First(d => d.Code == "OPS");

                var now = DateTime.UtcNow;

                var audits = new List<InternalAudit>
                {
                    new InternalAudit
                    {
                        TitleEn = "HR Payroll Control Audit Q1",
                        TitleAr = "تدقيق ضوابط رواتب الموارد البشرية الربع الأول",
                        AuditDate = now.AddDays(-20),
                        DepartmentId = hrDept.Id,
                        TotalControlsAudited = 10,
                        PassedControlsCount = 10,
                        CriticalFindingsCount = 0
                    },
                    new InternalAudit
                    {
                        TitleEn = "Financial Reporting Audit Q1",
                        TitleAr = "تدقيق التقارير القوائم المالية الربع الأول",
                        AuditDate = now.AddDays(-15),
                        DepartmentId = finDept.Id,
                        TotalControlsAudited = 12,
                        PassedControlsCount = 11,
                        CriticalFindingsCount = 0
                    },
                    new InternalAudit
                    {
                        TitleEn = "Operations Vehicle Dispatch Audit",
                        TitleAr = "تدقيق جدولة وإرسال رحلات العمليات",
                        AuditDate = now.AddDays(-10),
                        DepartmentId = opsDept.Id,
                        TotalControlsAudited = 15,
                        PassedControlsCount = 12,
                        CriticalFindingsCount = 2
                    }
                };
                context.InternalAudits.AddRange(audits);
                context.SaveChanges();
            }

            // Seed Improvement Actions if empty
            if (!context.ImprovementActions.Any())
            {
                var finDept = context.Departments.First(d => d.Code == "FIN");
                var itDept = context.Departments.First(d => d.Code == "IT");
                var opsDept = context.Departments.First(d => d.Code == "OPS");

                var now = DateTime.UtcNow;

                var improvements = new List<ImprovementAction>
                {
                    new ImprovementAction
                    {
                        TitleEn = "Implement Automated Permit Renewal Alerts",
                        TitleAr = "تفعيل تنبيهات تجديد التصاريح التلقائية",
                        DescriptionEn = "Configure system alerts 30 days before municipal permits expire.",
                        DescriptionAr = "إعداد نظام التنبيهات قبل 30 يوم من انتهاء تصاريح البلدية.",
                        DepartmentId = itDept.Id,
                        Status = ImprovementStatus.Implemented,
                        DateProposed = now.AddDays(-30),
                        DateImplemented = now.AddDays(-10)
                    },
                    new ImprovementAction
                    {
                        TitleEn = "Safety Awareness Training",
                        TitleAr = "تدريب التوعية بالسلامة المهنية",
                        DescriptionEn = "Conduct safety gear compliance training for all operations staff.",
                        DescriptionAr = "عقد دورة تدريبية حول أهمية الالتزام بأدوات السلامة لجميع موظفي العمليات.",
                        DepartmentId = opsDept.Id,
                        Status = ImprovementStatus.Approved,
                        DateProposed = now.AddDays(-25),
                        DateImplemented = null
                    },
                    new ImprovementAction
                    {
                        TitleEn = "Update Financial Encryption Standards",
                        TitleAr = "تحديث معايير تشفير البيانات المالية",
                        DescriptionEn = "Adopt stronger encryption policies for file sharing.",
                        DescriptionAr = "اعتماد سياسات تشفير أقوى لنظام مشاركة الملفات المالية.",
                        DepartmentId = finDept.Id,
                        Status = ImprovementStatus.Proposed,
                        DateProposed = now.AddDays(-15),
                        DateImplemented = null
                    }
                };
                context.ImprovementActions.AddRange(improvements);
                context.SaveChanges();
            }

            // Seed Operational Audits if empty
            if (!context.OperationalAudits.Any())
            {
                var hrDept = context.Departments.First(d => d.Code == "HR");
                var finDept = context.Departments.First(d => d.Code == "FIN");
                var opsDept = context.Departments.First(d => d.Code == "OPS");
                var itDept = context.Departments.First(d => d.Code == "IT");
                var compDept = context.Departments.First(d => d.Code == "COMP");

                var now = DateTime.UtcNow;

                var operationalAudits = new List<OperationalAudit>
                {
                    new OperationalAudit
                    {
                        TitleEn = "IT Infrastructure Security Audit",
                        TitleAr = "تدقيق أمن البنية التحتية لتقنية المعلومات",
                        AuditDate = now.AddDays(-25),
                        DepartmentId = itDept.Id,
                        AuditedProcessCount = 20,
                        PassedProcessCount = 19,
                        CriticalFindingsCount = 0,
                        RecommendationsCount = 5,
                        RiskMitigationRate = 95.0,
                        Status = OperationalAuditStatus.Completed
                    },
                    new OperationalAudit
                    {
                        TitleEn = "Operations Driver Schedule Review",
                        TitleAr = "مراجعة جدولة سائقي العمليات",
                        AuditDate = now.AddDays(-18),
                        DepartmentId = opsDept.Id,
                        AuditedProcessCount = 25,
                        PassedProcessCount = 21,
                        CriticalFindingsCount = 1,
                        RecommendationsCount = 8,
                        RiskMitigationRate = 84.0,
                        Status = OperationalAuditStatus.FollowUp
                    },
                    new OperationalAudit
                    {
                        TitleEn = "Finance Payroll Control Review",
                        TitleAr = "مراجعة ضوابط رواتب الإدارة المالية",
                        AuditDate = now.AddDays(-10),
                        DepartmentId = finDept.Id,
                        AuditedProcessCount = 15,
                        PassedProcessCount = 15,
                        CriticalFindingsCount = 0,
                        RecommendationsCount = 2,
                        RiskMitigationRate = 100.0,
                        Status = OperationalAuditStatus.Completed
                    },
                    new OperationalAudit
                    {
                        TitleEn = "HR Performance Evaluation Process Review",
                        TitleAr = "مراجعة عملية تقييم أداء الموارد البشرية",
                        AuditDate = now.AddDays(-5),
                        DepartmentId = hrDept.Id,
                        AuditedProcessCount = 12,
                        PassedProcessCount = 10,
                        CriticalFindingsCount = 0,
                        RecommendationsCount = 4,
                        RiskMitigationRate = 90.0,
                        Status = OperationalAuditStatus.InProgress
                    },
                    new OperationalAudit
                    {
                        TitleEn = "Legal & Contractual Compliance Review",
                        TitleAr = "مراجعة الالتزام القانوني والتعاقدي للامتثال",
                        AuditDate = now.AddDays(5),
                        DepartmentId = compDept.Id,
                        AuditedProcessCount = 10,
                        PassedProcessCount = 0,
                        CriticalFindingsCount = 0,
                        RecommendationsCount = 0,
                        RiskMitigationRate = 0.0,
                        Status = OperationalAuditStatus.Planned
                    }
                };

                context.OperationalAudits.AddRange(operationalAudits);
                context.SaveChanges();
            }

            // Seed Employees and Evaluations if empty or containing old names
            if (!context.Employees.Any() || context.Employees.Any(e => e.FullNameEn == "John Doe"))
            {
                // Clear old records to replace with Saudi names
                var oldEvals = context.EmployeeEvaluations.ToList();
                context.EmployeeEvaluations.RemoveRange(oldEvals);
                var oldEmps = context.Employees.ToList();
                context.Employees.RemoveRange(oldEmps);
                context.SaveChanges();

                var hrDept = context.Departments.First(d => d.Code == "HR");
                var finDept = context.Departments.First(d => d.Code == "FIN");
                var opsDept = context.Departments.First(d => d.Code == "OPS");
                var itDept = context.Departments.First(d => d.Code == "IT");
                var compDept = context.Departments.First(d => d.Code == "COMP");

                var adminUser = context.Users.FirstOrDefault(u => u.Email == "admin@company.com");

                var employees = new List<Employee>
                {
                    new Employee
                    {
                        FullNameEn = "Ahmed Al-Harthi",
                        FullNameAr = "أحمد الحارثي",
                        PhoneNumber = "0501111111",
                        Role = "HR Director",
                        DepartmentId = hrDept.Id,
                        JoinDate = DateTime.UtcNow.AddYears(-3),
                        Salary = 15000.00m,
                        Rating = 5,
                        IsSaudi = true,
                        IsActive = true
                    },
                    new Employee
                    {
                        FullNameEn = "Yasser Al-Harbi",
                        FullNameAr = "ياسر الحربي",
                        PhoneNumber = "0502222222",
                        Role = "Senior Developer",
                        DepartmentId = itDept.Id,
                        JoinDate = DateTime.UtcNow.AddYears(-1),
                        Salary = 12000.00m,
                        Rating = 4,
                        IsSaudi = true,
                        IsActive = true
                    },
                    new Employee
                    {
                        FullNameEn = "Sarah Al-Otaibi",
                        FullNameAr = "سارة العتيبي",
                        PhoneNumber = "0503333333",
                        Role = "Logistical Coordinator",
                        DepartmentId = opsDept.Id,
                        JoinDate = DateTime.UtcNow.AddMonths(-6),
                        Salary = 8500.00m,
                        Rating = 4,
                        IsSaudi = true,
                        IsActive = true,
                        UserId = adminUser?.Id // Link to admin user so they have assigned tasks!
                    },
                    new Employee
                    {
                        FullNameEn = "Mohammed Al-Shammari",
                        FullNameAr = "محمد الشمري",
                        PhoneNumber = "0504444444",
                        Role = "Senior Accountant",
                        DepartmentId = finDept.Id,
                        JoinDate = DateTime.UtcNow.AddYears(-2),
                        Salary = 10500.00m,
                        Rating = 3,
                        IsSaudi = true,
                        IsActive = true
                    },
                    new Employee
                    {
                        FullNameEn = "Khalid Al-Dawsari",
                        FullNameAr = "خالد الدوسري",
                        PhoneNumber = "0505555555",
                        Role = "Compliance Officer",
                        DepartmentId = compDept.Id,
                        JoinDate = DateTime.UtcNow.AddYears(-4),
                        Salary = 9000.00m,
                        Rating = 5,
                        IsSaudi = true,
                        IsActive = false
                    }
                };

                context.Employees.AddRange(employees);
                context.SaveChanges();

                var employeeList = context.Employees.ToList();
                var empAhmed = employeeList.First(e => e.PhoneNumber == "0501111111");
                var empYasser = employeeList.First(e => e.PhoneNumber == "0502222222");
                var empSarah = employeeList.First(e => e.PhoneNumber == "0503333333");

                var evaluations = new List<EmployeeEvaluation>
                {
                    new EmployeeEvaluation
                    {
                        EmployeeId = empAhmed.Id,
                        EvaluationDate = DateTime.UtcNow.AddMonths(-1),
                        EvaluationScore = 92,
                        NotesEn = "Excellent leadership of the HR team and policies renewal.",
                        NotesAr = "قيادة ممتازة لفريق الموارد البشرية وتجديد السياسات."
                    },
                    new EmployeeEvaluation
                    {
                        EmployeeId = empYasser.Id,
                        EvaluationDate = DateTime.UtcNow.AddMonths(-2),
                        EvaluationScore = 84,
                        NotesEn = "Very good technical contribution in development sprints.",
                        NotesAr = "مساهمة تقنية جيدة جداً في دورات التطوير البرمجية."
                    },
                    new EmployeeEvaluation
                    {
                        EmployeeId = empSarah.Id,
                        EvaluationDate = DateTime.UtcNow.AddMonths(-1),
                        EvaluationScore = 88,
                        NotesEn = "Great performance in operations coordination.",
                        NotesAr = "أداء رائع في تنسيق وتنظيم العمليات اللوجستية."
                    }
                };

                context.EmployeeEvaluations.AddRange(evaluations);
                context.SaveChanges();
            }

            // Seed IT Data
            if (!context.ItTickets.Any())
            {
                context.ItTickets.AddRange(
                    new ItTicket 
                    { 
                        TitleEn = "Network Uptime Issue in Workshop", 
                        TitleAr = "مشكلة جاهزية الشبكة في الورشة", 
                        DescriptionEn = "Slow wireless connection and printer disconnection.", 
                        DescriptionAr = "بطء الاتصال اللاسلكي وفصل الطابعات المتكرر.",
                        Status = "Resolved", 
                        Priority = "High", 
                        CreatedAt = DateTime.UtcNow.AddDays(-5), 
                        ResolvedAt = DateTime.UtcNow.AddDays(-4), 
                        UserSatisfaction = 5 
                    },
                    new ItTicket 
                    { 
                        TitleEn = "ERP System Backup Delay", 
                        TitleAr = "تأخر النسخ الاحتياطي لنظام ERP", 
                        DescriptionEn = "Daily backup routine took longer than expected.", 
                        DescriptionAr = "استغرق إجراء النسخ الاحتياطي اليومي وقتًا أطول من المتوقع.",
                        Status = "Resolved", 
                        Priority = "Medium", 
                        CreatedAt = DateTime.UtcNow.AddDays(-2), 
                        ResolvedAt = DateTime.UtcNow.AddDays(-1), 
                        UserSatisfaction = 4 
                    },
                    new ItTicket 
                    { 
                        TitleEn = "Printers Connection Issue in HQ", 
                        TitleAr = "مشكلة اتصال الطابعات بالمقر الرئيسي", 
                        DescriptionEn = "HQ users cannot print PDF documents.", 
                        DescriptionAr = "لا يمكن لمستخدمي المقر الرئيسي طباعة ملفات PDF.",
                        Status = "In Progress", 
                        Priority = "Low", 
                        CreatedAt = DateTime.UtcNow.AddHours(-6) 
                    }
                );
                context.SaveChanges();
            }

            if (!context.ItSystems.Any())
            {
                context.ItSystems.AddRange(
                    new ItSystem { NameEn = "Fleet Management ERP", NameAr = "نظام إدارة الأسطول الموحد", UptimePercentage = 99.8, LastBackupStatus = true, Status = "Active", IsAutomated = true },
                    new ItSystem { NameEn = "Driver Dispatch Portal", NameAr = "بوابة توزيع رحلات السائقين", UptimePercentage = 99.5, LastBackupStatus = true, Status = "Active", IsAutomated = true },
                    new ItSystem { NameEn = "Workshop Operations Web", NameAr = "شبكة عمليات الورش الفنية", UptimePercentage = 98.2, LastBackupStatus = false, Status = "Active", IsAutomated = false }
                );
                context.SaveChanges();
            }

            // Seed HSE Data
            if (!context.HseIncidents.Any())
            {
                context.HseIncidents.AddRange(
                    new HseIncident 
                    { 
                        TitleEn = "Near-Miss bus collision in yard", 
                        TitleAr = "حادث وشيك لتصادم حافلة في الساحة", 
                        Type = "NearMiss", 
                        Date = DateTime.UtcNow.AddDays(-10), 
                        DescriptionEn = "Two buses backing up simultaneously nearly collided. Traffic routing adjusted.", 
                        DescriptionAr = "كادت حافلتان أن تتصادما أثناء الرجوع للخلف في نفس الوقت. تم تعديل مسارات السير.",
                        Severity = "Low", 
                        Location = "Main Bus Yard" 
                    },
                    new HseIncident 
                    { 
                        TitleEn = "Minor hand scratch during tire change", 
                        TitleAr = "خدش بسيط في اليد أثناء تغيير الإطارات", 
                        Type = "Injury", 
                        Date = DateTime.UtcNow.AddDays(-3), 
                        DescriptionEn = "Technician scratched hand on rusty rim. First aid applied.", 
                        DescriptionAr = "تعرض الفني لخدش في اليد بسبب حافة إطار صدئة. تم تقديم الإسعافات الأولية.",
                        Severity = "Medium", 
                        Location = "Workshop Bay 2" 
                    }
                );
                context.SaveChanges();
            }

            if (!context.HseInspections.Any())
            {
                context.HseInspections.AddRange(
                    new HseInspection 
                    { 
                        TitleEn = "Annual Safety Compliance Audit", 
                        TitleAr = "التدقيق السنوي للالتزام بالسلامة", 
                        InspectionDate = DateTime.UtcNow.AddMonths(-1), 
                        InspectorName = "Yousef Al-Ahmadi", 
                        Status = "Completed", 
                        ComplianceScore = 96.5, 
                        TrainingHours = 40.0 
                    },
                    new HseInspection 
                    { 
                        TitleEn = "Monthly Fire Extinguisher Check", 
                        TitleAr = "الفحص الشهري لطفايات الحريق", 
                        InspectionDate = DateTime.UtcNow.AddDays(-12), 
                        InspectorName = "Yousef Al-Ahmadi", 
                        Status = "Completed", 
                        ComplianceScore = 92.0, 
                        TrainingHours = 15.0 
                    },
                    new HseInspection 
                    { 
                        TitleEn = "Emergency Evacuation Drill", 
                        TitleAr = "تدريب على الإخلاء في حالات الطوارئ", 
                        InspectionDate = DateTime.UtcNow.AddDays(5), 
                        InspectorName = "HSE Committee", 
                        Status = "Planned", 
                        ComplianceScore = 0.0, 
                        TrainingHours = 0.0 
                    }
                );
                context.SaveChanges();
            }

            // Seed Procurement Data
            if (!context.ProcurementRequests.Any())
            {
                context.ProcurementRequests.AddRange(
                    new ProcurementRequest
                    {
                        TitleEn = "Spare parts procurement cycle",
                        TitleAr = "دورة توريد قطع غيار الصيانة",
                        RequesterName = "Ali Al-Fahad",
                        SupplierName = "Al-Rashed Auto Parts",
                        Amount = 25000,
                        Status = "Received",
                        RequestDate = DateTime.UtcNow.AddDays(-10),
                        DeliveryDate = DateTime.UtcNow.AddDays(-4),
                        BudgetCompliant = true
                    },
                    new ProcurementRequest
                    {
                        TitleEn = "New vehicle fleet software",
                        TitleAr = "نظام برمجة وجدولة الأسطول الجديد",
                        RequesterName = "Ali Al-Fahad",
                        SupplierName = "Smart Fleet Solutions",
                        Amount = 80000,
                        Status = "Approved",
                        RequestDate = DateTime.UtcNow.AddDays(-3),
                        BudgetCompliant = true
                    },
                    new ProcurementRequest
                    {
                        TitleEn = "Office stationery supply",
                        TitleAr = "توفير مستلزمات وقرطاسية المكاتب",
                        RequesterName = "Khaled Saeed",
                        SupplierName = "Jarir Bookstore",
                        Amount = 12000,
                        Status = "Requested",
                        RequestDate = DateTime.UtcNow.AddDays(-1),
                        BudgetCompliant = false
                    }
                );
                context.SaveChanges();
            }

            if (!context.InventoryItems.Any())
            {
                context.InventoryItems.AddRange(
                    new InventoryItem
                    {
                        ItemNameEn = "Tire 295/80R22.5",
                        ItemNameAr = "إطارات حافلات مقاس 295/80R22.5",
                        Category = "Spare Parts",
                        Quantity = 150,
                        ReorderLevel = 80,
                        UnitPrice = 1200,
                        LastAuditDate = DateTime.UtcNow.AddDays(-15),
                        DiscrepancyCount = 0
                    },
                    new InventoryItem
                    {
                        ItemNameEn = "Oil Filter LF16015",
                        ItemNameAr = "فلتر زيت محرك LF16015",
                        Category = "Spare Parts",
                        Quantity = 90,
                        ReorderLevel = 100,
                        UnitPrice = 150,
                        LastAuditDate = DateTime.UtcNow.AddDays(-15),
                        DiscrepancyCount = 0
                    },
                    new InventoryItem
                    {
                        ItemNameEn = "Brake Pads Set",
                        ItemNameAr = "مجموعة أقمشة فرامل خلفية",
                        Category = "Spare Parts",
                        Quantity = 60,
                        ReorderLevel = 40,
                        UnitPrice = 350,
                        LastAuditDate = DateTime.UtcNow.AddDays(-15),
                        DiscrepancyCount = 1
                    }
                );
                context.SaveChanges();
            }

            // Seed Strategy & Performance Data
            if (!context.StrategicGoals.Any())
            {
                context.StrategicGoals.AddRange(
                    new StrategicGoal
                    {
                        TitleEn = "Expand fleet to Makkah suburban routes",
                        TitleAr = "توسيع نطاق تشغيل الأسطول لضواحي مكة المكرمة",
                        Weight = 30.0,
                        Progress = 85.0,
                        Status = "OnTrack",
                        TargetDate = DateTime.UtcNow.AddYears(1)
                    },
                    new StrategicGoal
                    {
                        TitleEn = "Acquire 20% market share in VIP leasing",
                        TitleAr = "الاستحواذ على 20% من حصة التأجير الفاخر لكبار الشخصيات",
                        Weight = 25.0,
                        Progress = 75.0,
                        Status = "OnTrack",
                        TargetDate = DateTime.UtcNow.AddMonths(8)
                    },
                    new StrategicGoal
                    {
                        TitleEn = "Implement 100% digital ticketing solution",
                        TitleAr = "تطبيق نظام التذاكر والتشغيل الرقمي بالكامل بنسبة 100%",
                        Weight = 20.0,
                        Progress = 90.0,
                        Status = "Completed",
                        TargetDate = DateTime.UtcNow.AddMonths(-2)
                    }
                );
                context.SaveChanges();
            }

            if (!context.PmoInitiatives.Any())
            {
                context.PmoInitiatives.AddRange(
                    new PmoInitiative
                    {
                        TitleEn = "Unified Fleet Operation Center (FOC)",
                        TitleAr = "مركز العمليات والتحكم الموحد للأسطول FOC",
                        ManagerName = "Samer Al-Otaibi",
                        Progress = 80.0,
                        Budget = 5000000,
                        Status = "InProgress",
                        StartDate = DateTime.UtcNow.AddMonths(-3),
                        EndDate = DateTime.UtcNow.AddMonths(3),
                        GovernanceMaturityScore = 4.5,
                        MilestoneOnTime = true
                    },
                    new PmoInitiative
                    {
                        TitleEn = "Rawahel Mobile Application Launch",
                        TitleAr = "إطلاق تطبيق الهواتف الذكية لشركة رواحل المشاعر",
                        ManagerName = "Mona Al-Shahri",
                        Progress = 95.0,
                        Budget = 1200000,
                        Status = "InProgress",
                        StartDate = DateTime.UtcNow.AddMonths(-4),
                        EndDate = DateTime.UtcNow.AddMonths(1),
                        GovernanceMaturityScore = 4.8,
                        MilestoneOnTime = true
                    },
                    new PmoInitiative
                    {
                        TitleEn = "Eco-friendly EV Bus Trial",
                        TitleAr = "المشروع التجريبي لتشغيل الحافلات الكهربائية الصديقة للبيئة",
                        ManagerName = "Samer Al-Otaibi",
                        Progress = 30.0,
                        Budget = 3500000,
                        Status = "Delayed",
                        StartDate = DateTime.UtcNow.AddMonths(-2),
                        EndDate = DateTime.UtcNow.AddMonths(6),
                        GovernanceMaturityScore = 3.5,
                        MilestoneOnTime = false
                    }
                );
                context.SaveChanges();
            }

            // Seed Finance Transactions
            if (!context.FinanceTransactions.Any())
            {
                context.FinanceTransactions.AddRange(
                    new FinanceTransaction { DescriptionEn = "Monthly Passenger Ticket Revenues", DescriptionAr = "إيرادات بيع تذاكر الركاب الشهرية", Amount = 1200000m, Type = "Revenue", Date = DateTime.UtcNow.AddDays(-10), CategoryEn = "Operations", CategoryAr = "العمليات" },
                    new FinanceTransaction { DescriptionEn = "VIP Bus Rental Contract", DescriptionAr = "عقد تأجير حافلات كبار الشخصيات", Amount = 350000m, Type = "Revenue", Date = DateTime.UtcNow.AddDays(-5), CategoryEn = "Operations", CategoryAr = "العمليات" },
                    new FinanceTransaction { DescriptionEn = "Workshop Spare Parts Purchase", DescriptionAr = "شراء قطع غيار للورشة الفنية", Amount = 150000m, Type = "Expense", Date = DateTime.UtcNow.AddDays(-12), CategoryEn = "Fleet", CategoryAr = "الأسطول" },
                    new FinanceTransaction { DescriptionEn = "Staff Fuel Allowance", DescriptionAr = "مصروفات محروقات وديزل الحافلات", Amount = 80000m, Type = "Expense", Date = DateTime.UtcNow.AddDays(-8), CategoryEn = "Operations", CategoryAr = "العمليات" },
                    new FinanceTransaction { DescriptionEn = "Office Building Monthly Rent", DescriptionAr = "إيجار مبنى المكاتب الرئيسي", Amount = 45000m, Type = "Expense", Date = DateTime.UtcNow.AddDays(-15), CategoryEn = "General", CategoryAr = "عام" },
                    new FinanceTransaction { DescriptionEn = "Depreciation of Fleet Assets", DescriptionAr = "إهلاك أصول أسطول المركبات", Amount = 60000m, Type = "Expense", Date = DateTime.UtcNow.AddDays(-2), CategoryEn = "Depreciation", CategoryAr = "إهلاك" },
                    new FinanceTransaction { DescriptionEn = "Fixed Assets - New Buses Purchase", DescriptionAr = "أصول ثابتة - شراء حافلات جديدة", Amount = 5000000m, Type = "Asset", Date = DateTime.UtcNow.AddMonths(-1), CategoryEn = "Assets", CategoryAr = "الأصول" },
                    new FinanceTransaction { DescriptionEn = "Bank Loan Balance", DescriptionAr = "التزام القرض البنكي المتبقي", Amount = 2000000m, Type = "Liability", Date = DateTime.UtcNow.AddMonths(-2), CategoryEn = "Liabilities", CategoryAr = "الالتزامات" },
                    new FinanceTransaction { DescriptionEn = "Outstanding Client Receivables", DescriptionAr = "مستحقات معلقة تحت التحصيل من العملاء", Amount = 400000m, Type = "Receivables", Date = DateTime.UtcNow.AddDays(-1), CategoryEn = "Receivables", CategoryAr = "الذمم المدينة" }
                );
                context.SaveChanges();
            }

            // Seed Finance Budgets
            if (!context.FinanceBudgets.Any())
            {
                context.FinanceBudgets.AddRange(
                    new FinanceBudget { DepartmentNameEn = "Operations", DepartmentNameAr = "التشغيل والعمليات", AllocatedAmount = 1500000m, SpentAmount = 1420000m, Year = DateTime.UtcNow.Year },
                    new FinanceBudget { DepartmentNameEn = "Information Technology", DepartmentNameAr = "تقنية المعلومات", AllocatedAmount = 500000m, SpentAmount = 520000m, Year = DateTime.UtcNow.Year },
                    new FinanceBudget { DepartmentNameEn = "Human Resources", DepartmentNameAr = "الموارد البشرية", AllocatedAmount = 300000m, SpentAmount = 280000m, Year = DateTime.UtcNow.Year },
                    new FinanceBudget { DepartmentNameEn = "Fleet Maintenance", DepartmentNameAr = "صيانة الأسطول", AllocatedAmount = 800000m, SpentAmount = 790000m, Year = DateTime.UtcNow.Year }
                );
                context.SaveChanges();
            }

            // Seed Commercial Contracts
            if (!context.CommercialContracts.Any())
            {
                context.CommercialContracts.AddRange(
                    new CommercialContract { ClientNameEn = "Al-Mashair Pilgrims Co", ClientNameAr = "شركة حجاج المشاعر", ContractNumber = "B2B-2026-091", Value = 800000m, StartDate = DateTime.UtcNow.AddMonths(-3), EndDate = DateTime.UtcNow.AddMonths(9), PreparationDate = DateTime.UtcNow.AddMonths(-4), ActiveDate = DateTime.UtcNow.AddMonths(-3), Status = "Active", IsDisputed = false },
                    new CommercialContract { ClientNameEn = "Makkah Transport Authority", ClientNameAr = "هيئة النقل بمكة المكرمة", ContractNumber = "B2B-2026-092", Value = 1500000m, StartDate = DateTime.UtcNow.AddMonths(-1), EndDate = DateTime.UtcNow.AddMonths(11), PreparationDate = DateTime.UtcNow.AddMonths(-2), ActiveDate = DateTime.UtcNow.AddMonths(-1), Status = "Active", IsDisputed = false },
                    new CommercialContract { ClientNameEn = "Zamazem Water Company", ClientNameAr = "شركة زمازمة للمياه", ContractNumber = "B2B-2026-093", Value = 450000m, StartDate = DateTime.UtcNow.AddMonths(-6), EndDate = DateTime.UtcNow.AddMonths(-1), PreparationDate = DateTime.UtcNow.AddMonths(-7), ActiveDate = DateTime.UtcNow.AddMonths(-6), Status = "Renewed", IsDisputed = false },
                    new CommercialContract { ClientNameEn = "Hajj Service Alliance", ClientNameAr = "تحالف خدمات الحج والعمرة", ContractNumber = "B2B-2026-094", Value = 600000m, StartDate = DateTime.UtcNow.AddDays(-25), EndDate = DateTime.UtcNow.AddMonths(5), PreparationDate = DateTime.UtcNow.AddDays(-40), ActiveDate = DateTime.UtcNow.AddDays(-25), Status = "Active", IsDisputed = true },
                    new CommercialContract { ClientNameEn = "Rawafed Logistics Group", ClientNameAr = "مجموعة روافد للخدمات اللوجستية", ContractNumber = "B2B-2026-095", Value = 300000m, StartDate = DateTime.UtcNow.AddMonths(-12), EndDate = DateTime.UtcNow.AddMonths(-2), PreparationDate = DateTime.UtcNow.AddMonths(-13), ActiveDate = DateTime.UtcNow.AddMonths(-12), Status = "Expired", IsDisputed = false }
                );
                context.SaveChanges();
            }

            // Seed Commercial Leads
            if (!context.CommercialLeads.Any())
            {
                context.CommercialLeads.AddRange(
                    new CommercialLead { LeadName = "Umrah VIP Transport Deal", Source = "Campaign", Status = "Won", EstimatedValue = 500000m, AcquisitionCost = 25000m, CreatedAt = DateTime.UtcNow.AddMonths(-2) },
                    new CommercialLead { LeadName = "Jeddah Airport Shuttle Service", Source = "Referral", Status = "Won", EstimatedValue = 700000m, AcquisitionCost = 35000m, CreatedAt = DateTime.UtcNow.AddMonths(-1) },
                    new CommercialLead { LeadName = "Hajj Season Catering Bus Rental", Source = "Web", Status = "New", EstimatedValue = 300000m, AcquisitionCost = 5000m, CreatedAt = DateTime.UtcNow.AddDays(-5) },
                    new CommercialLead { LeadName = "Hotel Staff Daily Commute Contract", Source = "Direct", Status = "InProgress", EstimatedValue = 250000m, AcquisitionCost = 10000m, CreatedAt = DateTime.UtcNow.AddDays(-10) }
                );
                context.SaveChanges();
            }

            // Seed Tourism Hotel Bookings
            if (!context.TourismHotelBookings.Any())
            {
                context.TourismHotelBookings.AddRange(
                    new TourismHotelBooking { ClientNameAr = "شركة حجاج ماليزيا", ClientNameEn = "Malaysia Pilgrims Co", HotelNameAr = "شيراتون مكة", HotelNameEn = "Sheraton Makkah", RoomCount = 120, CheckInDate = DateTime.UtcNow.AddDays(-10), CheckOutDate = DateTime.UtcNow.AddDays(-3), Value = 250000m, Status = "Confirmed", GuestRating = 5 },
                    new TourismHotelBooking { ClientNameAr = "تحالف العمرة التركي", ClientNameEn = "Turkish Umrah Alliance", HotelNameAr = "هيلتون المدينة", HotelNameEn = "Hilton Madinah", RoomCount = 80, CheckInDate = DateTime.UtcNow.AddDays(-5), CheckOutDate = DateTime.UtcNow.AddDays(2), Value = 180000m, Status = "Confirmed", GuestRating = 4 },
                    new TourismHotelBooking { ClientNameAr = "مجموعة سياحة إندونيسيا", ClientNameEn = "Indonesia Tourism Group", HotelNameAr = "فيرمونت مكة", HotelNameEn = "Fairmont Makkah", RoomCount = 50, CheckInDate = DateTime.UtcNow.AddDays(1), CheckOutDate = DateTime.UtcNow.AddDays(6), Value = 120000m, Status = "Confirmed", GuestRating = null },
                    new TourismHotelBooking { ClientNameAr = "وكالة السفر المصرية", ClientNameEn = "Egypt Travel Agency", HotelNameAr = "بولمان زمزم", HotelNameEn = "Pulman Zamzam", RoomCount = 40, CheckInDate = DateTime.UtcNow.AddDays(3), CheckOutDate = DateTime.UtcNow.AddDays(8), Value = 90000m, Status = "Cancelled", GuestRating = null }
                );
                context.SaveChanges();
            }

            // Seed Tourism Tours
            if (!context.TourismTours.Any())
            {
                context.TourismTours.AddRange(
                    new TourismTour { TourNameAr = "جولة مكة التاريخية", TourNameEn = "Makkah Historic Sites", GuideNameAr = "أحمد السعيد", GuideNameEn = "Ahmad Al-Saeed", Date = DateTime.UtcNow.AddDays(-5), PassengerCount = 45, BookingLeadTimeHours = 24.0, Status = "Completed" },
                    new TourismTour { TourNameAr = "جولة المدينة المنورة التاريخية", TourNameEn = "Madinah Historic Sites", GuideNameAr = "خالد عبد الرحمن", GuideNameEn = "Khaled Abdulrahman", Date = DateTime.UtcNow.AddDays(-3), PassengerCount = 38, BookingLeadTimeHours = 36.0, Status = "Completed" },
                    new TourismTour { TourNameAr = "جولة اليوم الواحد في الطائف", TourNameEn = "Taif Day Tour", GuideNameAr = "ياسر محمود", GuideNameEn = "Yasir Mahmoud", Date = DateTime.UtcNow, PassengerCount = 25, BookingLeadTimeHours = 12.0, Status = "Scheduled" },
                    new TourismTour { TourNameAr = "جولة مكة التاريخية الثانية", TourNameEn = "Makkah Historic Sites Tour 2", GuideNameAr = "أحمد السعيد", GuideNameEn = "Ahmad Al-Saeed", Date = DateTime.UtcNow.AddDays(1), PassengerCount = 50, BookingLeadTimeHours = 48.0, Status = "Scheduled" }
                );
                context.SaveChanges();
            }

            // Seed Operations Daily Plans
            if (!context.OperationsDailyPlans.Any())
            {
                context.OperationsDailyPlans.AddRange(
                    new OperationsDailyPlan { Date = DateTime.UtcNow.AddDays(-2), ScheduledTripsCount = 120, CompletedTripsCount = 118, FuelEfficiencyIndex = 94.5, PassengerSatisfactionRate = 92.0, Status = "Completed" },
                    new OperationsDailyPlan { Date = DateTime.UtcNow.AddDays(-1), ScheduledTripsCount = 115, CompletedTripsCount = 112, FuelEfficiencyIndex = 93.8, PassengerSatisfactionRate = 91.5, Status = "Completed" },
                    new OperationsDailyPlan { Date = DateTime.UtcNow, ScheduledTripsCount = 125, CompletedTripsCount = 122, FuelEfficiencyIndex = 95.0, PassengerSatisfactionRate = 93.0, Status = "InProgress" }
                );
                context.SaveChanges();
            }

            // Seed Operations Incidents
            if (!context.OperationsIncidents.Any())
            {
                context.OperationsIncidents.AddRange(
                    new OperationsIncident { DescriptionAr = "عطل ميكانيكي في الحافلة رقم 4032", DescriptionEn = "Bus #4032 mechanical breakdown", Severity = "Medium", ResponseTimeMinutes = 42.0, Date = DateTime.UtcNow.AddDays(-5), Status = "Resolved" },
                    new OperationsIncident { DescriptionAr = "تأخر المسار بسبب الازدحام المروري على طريق الحرمين", DescriptionEn = "Route delay due to traffic at Haramain road", Severity = "Low", ResponseTimeMinutes = 15.0, Date = DateTime.UtcNow.AddDays(-3), Status = "Resolved" },
                    new OperationsIncident { DescriptionAr = "حادث بسيط للحافلة رقم 1029", DescriptionEn = "Bus #1029 minor accident", Severity = "High", ResponseTimeMinutes = 25.0, Date = DateTime.UtcNow, Status = "Resolved" }
                );
                context.SaveChanges();
            }
        }
    }
}
