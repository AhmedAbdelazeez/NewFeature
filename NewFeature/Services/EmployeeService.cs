using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NewFeature.Models;
using Microsoft.Extensions.Logging;
using NewFeature.Services.ExcelImport;
using NewFeature.Services.Repositories;

namespace NewFeature.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<EmployeeService> _logger;

        public EmployeeService(ApplicationDbContext context, ILogger<EmployeeService> logger)
        {
            _context = context;
            _logger = logger;
        }

        #region Employee CRUD
        public async Task<IEnumerable<EmployeeDto>> GetAllEmployeesAsync()
        {
            var employees = await _context.Employees
                .Include(e => e.Department)
                .Include(e => e.User)
                .ThenInclude(u => u != null ? u.AssignedTasks : null)
                .ToListAsync();

            return employees.Select(e => MapToEmployeeDto(e));
        }

        // Backs the Employees management page's roster table. Materializes once via the existing
        // EF query, then filters/orders/pages in memory - matching the house style used by
        // FleetService.GetVehiclesPagedAsync elsewhere in this codebase.
        public async Task<PagedResultDto<EmployeeDto>> GetEmployeesPagedAsync(int page, int pageSize, string? search)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 20;
            if (pageSize > 200) pageSize = 200;

            var employees = await _context.Employees
                .Include(e => e.Department)
                .Include(e => e.User)
                .ThenInclude(u => u != null ? u.AssignedTasks : null)
                .ToListAsync();

            var dtos = employees.Select(e => MapToEmployeeDto(e)).AsEnumerable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                dtos = dtos.Where(e =>
                    Contains(e.FullNameEn, term) ||
                    Contains(e.FullNameAr, term) ||
                    Contains(e.PhoneNumber, term) ||
                    Contains(e.Role, term) ||
                    Contains(e.DepartmentNameEn, term) ||
                    Contains(e.DepartmentNameAr, term));
            }

            var ordered = dtos.OrderByDescending(e => e.Id).ToList();
            var totalCount = ordered.Count;

            var items = ordered.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            return new PagedResultDto<EmployeeDto>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<EmployeeDto?> GetEmployeeByIdAsync(int id)
        {
            var employee = await _context.Employees
                .Include(e => e.Department)
                .Include(e => e.User)
                .ThenInclude(u => u != null ? u.AssignedTasks : null)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (employee == null) return null;
            return MapToEmployeeDto(employee);
        }

        public async Task<EmployeeDto> CreateEmployeeAsync(EmployeeDto dto)
        {
            var employee = new Employee
            {
                FullNameEn = dto.FullNameEn,
                FullNameAr = dto.FullNameAr,
                PhoneNumber = dto.PhoneNumber,
                Role = dto.Role,
                DepartmentId = dto.DepartmentId,
                JoinDate = dto.JoinDate,
                Salary = dto.Salary,
                Rating = dto.Rating,
                IsSaudi = dto.IsSaudi,
                IsActive = dto.IsActive,
                UserId = string.IsNullOrEmpty(dto.UserId) ? null : dto.UserId
            };

            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();

            // Reload to fetch navigation properties
            return await GetEmployeeByIdAsync(employee.Id) ?? dto;
        }

        public async Task<bool> UpdateEmployeeAsync(EmployeeDto dto)
        {
            var employee = await _context.Employees.FindAsync(dto.Id);
            if (employee == null) return false;

            employee.FullNameEn = dto.FullNameEn;
            employee.FullNameAr = dto.FullNameAr;
            employee.PhoneNumber = dto.PhoneNumber;
            employee.Role = dto.Role;
            employee.DepartmentId = dto.DepartmentId;
            employee.JoinDate = dto.JoinDate;
            employee.Salary = dto.Salary;
            employee.Rating = dto.Rating;
            employee.IsSaudi = dto.IsSaudi;
            employee.IsActive = dto.IsActive;
            employee.UserId = string.IsNullOrEmpty(dto.UserId) ? null : dto.UserId;

            _context.Entry(employee).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteEmployeeAsync(int id)
        {
            var employee = await _context.Employees.FindAsync(id);
            if (employee == null) return false;

            _context.Employees.Remove(employee);
            await _context.SaveChangesAsync();
            return true;
        }
        #endregion

        #region Employee Evaluation CRUD
        public async Task<IEnumerable<EmployeeEvaluationDto>> GetAllEvaluationsAsync()
        {
            var evaluations = await _context.EmployeeEvaluations
                .Include(ee => ee.Employee)
                .ToListAsync();

            return evaluations.Select(ee => MapToEvaluationDto(ee));
        }

        // Backs the Employees page's Evaluations tab table.
        public async Task<PagedResultDto<EmployeeEvaluationDto>> GetEvaluationsPagedAsync(int page, int pageSize, string? search)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 20;
            if (pageSize > 200) pageSize = 200;

            var evaluations = await _context.EmployeeEvaluations
                .Include(ee => ee.Employee)
                .ToListAsync();

            var dtos = evaluations.Select(ee => MapToEvaluationDto(ee)).AsEnumerable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                dtos = dtos.Where(ee =>
                    Contains(ee.EmployeeName, term) ||
                    Contains(ee.NotesEn, term) ||
                    Contains(ee.NotesAr, term));
            }

            var ordered = dtos.OrderByDescending(ee => ee.Id).ToList();
            var totalCount = ordered.Count;

            var items = ordered.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            return new PagedResultDto<EmployeeEvaluationDto>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<EmployeeEvaluationDto?> GetEvaluationByIdAsync(int id)
        {
            var evaluation = await _context.EmployeeEvaluations
                .Include(ee => ee.Employee)
                .FirstOrDefaultAsync(ee => ee.Id == id);

            if (evaluation == null) return null;
            return MapToEvaluationDto(evaluation);
        }

        public async Task<EmployeeEvaluationDto> CreateEvaluationAsync(EmployeeEvaluationDto dto)
        {
            var evaluation = new EmployeeEvaluation
            {
                EmployeeId = dto.EmployeeId,
                EvaluationDate = dto.EvaluationDate,
                EvaluationScore = dto.EvaluationScore,
                NotesEn = dto.NotesEn,
                NotesAr = dto.NotesAr
            };

            _context.EmployeeEvaluations.Add(evaluation);
            await _context.SaveChangesAsync();

            // Reload to fetch navigation
            return await GetEvaluationByIdAsync(evaluation.Id) ?? dto;
        }

        public async Task<bool> UpdateEvaluationAsync(EmployeeEvaluationDto dto)
        {
            var evaluation = await _context.EmployeeEvaluations.FindAsync(dto.Id);
            if (evaluation == null) return false;

            evaluation.EmployeeId = dto.EmployeeId;
            evaluation.EvaluationDate = dto.EvaluationDate;
            evaluation.EvaluationScore = dto.EvaluationScore;
            evaluation.NotesEn = dto.NotesEn;
            evaluation.NotesAr = dto.NotesAr;

            _context.Entry(evaluation).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteEvaluationAsync(int id)
        {
            var evaluation = await _context.EmployeeEvaluations.FindAsync(id);
            if (evaluation == null) return false;

            _context.EmployeeEvaluations.Remove(evaluation);
            await _context.SaveChangesAsync();
            return true;
        }
        #endregion

        #region HR KPIs Calculator
        public async Task<HrKpisDto> GetHrKpisAsync()
        {
            var employees = await _context.Employees
                .Include(e => e.User)
                .ThenInclude(u => u != null ? u.AssignedTasks : null)
                .ToListAsync();

            var evaluations = await _context.EmployeeEvaluations.ToListAsync();

            int totalEmployees = employees.Count;
            int activeCount = employees.Count(e => e.IsActive);
            int leaversCount = totalEmployees - activeCount;

            double saudization = 0;
            double retention = 0;
            double avgRating = 0;
            double avgEvaluation = 0;
            decimal avgSalary = 0;
            double avgTasks = 0;

            // Employee Retention Rate (نسبة بقاء الموظفين): of the people who were already on the
            // payroll twelve months ago, what share is still on it today. Measuring it over that
            // cohort - rather than over everyone ever hired - is what keeps a hiring spree from
            // inflating the figure, since this year's new joiners had no chance to leave yet.
            // When the roster has nobody with a full year of service (a freshly uploaded sheet,
            // or a brand-new department), that cohort is empty and the rate falls back to the
            // plain active-headcount share so the card still reports something truthful.
            var retentionCohortStart = DateTime.UtcNow.Date.AddYears(-1);
            var retentionCohort = employees.Where(e => e.JoinDate.Date <= retentionCohortStart).ToList();

            if (retentionCohort.Count > 0)
            {
                retention = ((double)retentionCohort.Count(e => e.IsActive) / retentionCohort.Count) * 100;
            }
            else if (totalEmployees > 0)
            {
                retention = ((double)activeCount / totalEmployees) * 100;
            }

            if (totalEmployees > 0)
            {
                int saudiCount = employees.Count(e => e.IsSaudi);
                saudization = ((double)saudiCount / totalEmployees) * 100;

                avgRating = employees.Average(e => e.Rating);

                avgSalary = employees.Average(e => e.Salary);

                var userIds = employees.Where(e => !string.IsNullOrEmpty(e.UserId)).Select(e => e.UserId).ToList();
                int totalTasks = await _context.Tasks.CountAsync(t => userIds.Contains(t.AssignedToUserId));
                avgTasks = (double)totalTasks / totalEmployees;
            }

            if (evaluations.Count > 0)
            {
                avgEvaluation = evaluations.Average(ee => ee.EvaluationScore);
            }

            return new HrKpisDto
            {
                SaudizationRateActual = Math.Round(saudization, 1),
                SaudizationRateTarget = 35.0, // Target Saudization: 35%

                RetentionRateActual = Math.Round(retention, 1),
                RetentionRateTarget = 90.0, // Target Retention: 90%

                AvgRatingActual = Math.Round(avgRating, 1),
                AvgRatingTarget = 4.0, // Target Rating out of 5

                AvgEvaluationActual = Math.Round(avgEvaluation, 1),
                AvgEvaluationTarget = 80.0, // Target Evaluation Score: 80%

                TotalEmployeesActual = totalEmployees,
                TotalEmployeesTarget = 20, // Target workforce size: 20 employees

                AvgSalaryActual = Math.Round(avgSalary, 2),
                AvgSalaryTarget = 9000.00m, // Target Average Salary: 9000 SAR

                AvgTasksPerEmployeeActual = Math.Round(avgTasks, 1),
                AvgTasksPerEmployeeTarget = 4.0, // Target Average Tasks per Employee: 4

                ActiveEmployeesActual = activeCount,
                LeaversCountActual = leaversCount,

                // Turnover is retention's complement over the same cohort, so the two cards on the
                // dashboard always add up to 100% instead of telling two different stories.
                TurnoverRateActual = Math.Round(100.0 - retention, 1),
                TurnoverRateTarget = 10.0
            };
        }
        #endregion

        private static bool Contains(string? haystack, string term) =>
            !string.IsNullOrEmpty(haystack) && haystack.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0;

        #region Mappers
        private static EmployeeDto MapToEmployeeDto(Employee e)
        {
            var isAr = System.Globalization.CultureInfo.CurrentCulture.TwoLetterISOLanguageName == "ar";
            return new EmployeeDto
            {
                Id = e.Id,
                FullNameEn = e.FullNameEn,
                FullNameAr = e.FullNameAr,
                PhoneNumber = e.PhoneNumber,
                Role = e.Role,
                DepartmentId = e.DepartmentId,
                DepartmentName = e.Department != null ? (isAr ? e.Department.NameAr : e.Department.NameEn) : string.Empty,
                DepartmentNameEn = e.Department != null ? e.Department.NameEn : string.Empty,
                DepartmentNameAr = e.Department != null ? e.Department.NameAr : string.Empty,
                JoinDate = e.JoinDate,
                Salary = e.Salary,
                Rating = e.Rating,
                IsSaudi = e.IsSaudi,
                IsActive = e.IsActive,
                UserId = e.UserId,
                UserName = e.User != null ? e.User.UserName ?? string.Empty : string.Empty,
                AssignedTasksCount = e.User != null && e.User.AssignedTasks != null ? e.User.AssignedTasks.Count : 0,
                FullName = isAr ? e.FullNameAr : e.FullNameEn
            };
        }

        private static EmployeeEvaluationDto MapToEvaluationDto(EmployeeEvaluation ee)
        {
            var isAr = System.Globalization.CultureInfo.CurrentCulture.TwoLetterISOLanguageName == "ar";
            return new EmployeeEvaluationDto
            {
                Id = ee.Id,
                EmployeeId = ee.EmployeeId,
                EmployeeName = ee.Employee != null ? (isAr ? ee.Employee.FullNameAr : ee.Employee.FullNameEn) : string.Empty,
                EvaluationDate = ee.EvaluationDate,
                EvaluationScore = ee.EvaluationScore,
                NotesEn = ee.NotesEn,
                NotesAr = ee.NotesAr,
                Notes = isAr ? ee.NotesAr : ee.NotesEn
            };
        }
        #endregion
        #region Bulk upload (approved single-sheet employee roster)
        // Imports the approved HR template: one row per employee. Departments named in the sheet
        // are created on the fly, and an employee already on file (matched on Arabic name) is
        // updated rather than duplicated, so re-uploading a corrected roster is safe.
        public async Task<ExcelImportResultDto> BulkUploadEmployeesAsync(System.IO.Stream excelStream)
        {
            var departments = await _context.Departments.ToListAsync();
            var employees = await _context.Employees.ToListAsync();

            return await ExcelImportEngine.RunAsync(
                excelStream,
                DepartmentTemplates.Hr,
                async row =>
                {
                    var name = row.GetString(DepartmentTemplates.HrEmployeeName);
                    if (string.IsNullOrWhiteSpace(name))
                        return ExcelRowOutcomeResult.Skipped("اسم الموظف مطلوب.", "اسم الموظف");

                    // Without a join date the retention rate has no cohort to measure, so a row
                    // missing it is rejected instead of quietly defaulting to today.
                    var joinDate = row.GetDate(DepartmentTemplates.HrJoinDate);
                    if (joinDate == null)
                        return ExcelRowOutcomeResult.Skipped("تاريخ التعيين غير مقروء. استخدم الصيغة يوم/شهر/سنة.", "تاريخ التعيين");
                    if (joinDate.Value.Date > DateTime.UtcNow.Date)
                        return ExcelRowOutcomeResult.Skipped("تاريخ التعيين في المستقبل.", "تاريخ التعيين");

                    var departmentName = row.GetString(DepartmentTemplates.HrDepartment);
                    if (string.IsNullOrWhiteSpace(departmentName))
                        return ExcelRowOutcomeResult.Skipped("القسم مطلوب.", "القسم");

                    var department = departments.FirstOrDefault(d =>
                        string.Equals(d.NameAr, departmentName, StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(d.NameEn, departmentName, StringComparison.OrdinalIgnoreCase));
                    if (department == null)
                    {
                        department = new Department
                        {
                            NameAr = departmentName,
                            NameEn = departmentName,
                            Code = $"DEP-{departments.Count + 1:D3}",
                            IsCompliant = true
                        };
                        _context.Departments.Add(department);
                        departments.Add(department);
                    }

                    var role = row.GetString(DepartmentTemplates.HrJobTitle);
                    if (string.IsNullOrWhiteSpace(role)) role = "موظف";

                    var phone = row.GetString(DepartmentTemplates.HrPhone);
                    if (string.IsNullOrWhiteSpace(phone)) phone = "-";
                    if (phone.Length > 20) phone = phone.Substring(0, 20);

                    var salary = row.GetDecimal(DepartmentTemplates.HrSalary) ?? 0m;
                    if (salary < 0) salary = 0m;

                    // The entity constrains Rating to 1-5; a blank or out-of-range cell settles on
                    // the neutral middle score rather than failing the whole row.
                    var rating = row.GetInt(DepartmentTemplates.HrRating) ?? 3;
                    if (rating < 1) rating = 1;
                    if (rating > 5) rating = 5;

                    var isSaudi = ParseIsSaudi(row.GetString(DepartmentTemplates.HrNationality));
                    var isActive = ParseIsActive(row.GetString(DepartmentTemplates.HrEmploymentStatus));

                    var existing = employees.FirstOrDefault(e =>
                        string.Equals(e.FullNameAr, name, StringComparison.OrdinalIgnoreCase));

                    if (existing != null)
                    {
                        existing.FullNameEn = name;
                        existing.PhoneNumber = phone;
                        existing.Role = role;
                        existing.Department = department;
                        existing.JoinDate = joinDate.Value.Date;
                        existing.Salary = salary;
                        existing.Rating = rating;
                        existing.IsSaudi = isSaudi;
                        existing.IsActive = isActive;
                        return ExcelRowOutcomeResult.Updated();
                    }

                    var employee = new Employee
                    {
                        FullNameAr = name,
                        FullNameEn = name,
                        PhoneNumber = phone,
                        Role = role,
                        Department = department,
                        JoinDate = joinDate.Value.Date,
                        Salary = salary,
                        Rating = rating,
                        IsSaudi = isSaudi,
                        IsActive = isActive
                    };
                    _context.Employees.Add(employee);
                    employees.Add(employee);
                    await System.Threading.Tasks.Task.CompletedTask;
                    return ExcelRowOutcomeResult.Inserted();
                },
                () => _context.SaveChangesAsync(),
                _logger);
        }

        private static bool ParseIsSaudi(string? raw)
        {
            var value = (raw ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(value)) return false;
            // "غير سعودي" contains "سعودي", so the negation has to be checked first.
            if (value.Contains("غير") || value.IndexOf("non", StringComparison.OrdinalIgnoreCase) >= 0) return false;
            return value.Contains("سعودي") || value.IndexOf("saudi", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static bool ParseIsActive(string? raw)
        {
            var value = (raw ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(value)) return true;
            if (value.Contains("منته") || value.Contains("مستقيل") || value.Contains("مفصول") ||
                value.Contains("ترك") || value.Contains("مغادر") ||
                value.IndexOf("resigned", StringComparison.OrdinalIgnoreCase) >= 0 ||
                value.IndexOf("terminated", StringComparison.OrdinalIgnoreCase) >= 0 ||
                value.IndexOf("inactive", StringComparison.OrdinalIgnoreCase) >= 0 ||
                value.IndexOf("left", StringComparison.OrdinalIgnoreCase) >= 0)
                return false;
            return true;
        }
        #endregion
    }
}
