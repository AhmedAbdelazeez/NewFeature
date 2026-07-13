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
        }
    }
}
