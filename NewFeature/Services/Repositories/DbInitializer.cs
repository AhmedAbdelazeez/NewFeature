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
        }
    }
}
