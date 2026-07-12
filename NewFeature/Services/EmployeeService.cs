using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NewFeature.Models;
using NewFeature.Services.Repositories;

namespace NewFeature.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly ApplicationDbContext _context;

        public EmployeeService(ApplicationDbContext context)
        {
            _context = context;
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

            double saudization = 0;
            double retention = 0;
            double avgRating = 0;
            double avgEvaluation = 0;
            decimal avgSalary = 0;
            double avgTasks = 0;

            if (totalEmployees > 0)
            {
                int saudiCount = employees.Count(e => e.IsSaudi);
                saudization = ((double)saudiCount / totalEmployees) * 100;

                int activeCount = employees.Count(e => e.IsActive);
                retention = ((double)activeCount / totalEmployees) * 100;

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
                AvgTasksPerEmployeeTarget = 4.0 // Target Average Tasks per Employee: 4
            };
        }
        #endregion

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
    }
}
