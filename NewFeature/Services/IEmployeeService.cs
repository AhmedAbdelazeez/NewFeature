using System.Collections.Generic;
using System.Threading.Tasks;
using NewFeature.Models;

namespace NewFeature.Services
{
    public interface IEmployeeService
    {
        // Employee CRUD
        Task<IEnumerable<EmployeeDto>> GetAllEmployeesAsync();
        Task<EmployeeDto?> GetEmployeeByIdAsync(int id);
        Task<EmployeeDto> CreateEmployeeAsync(EmployeeDto dto);
        Task<bool> UpdateEmployeeAsync(EmployeeDto dto);
        Task<bool> DeleteEmployeeAsync(int id);

        // Employee Evaluation CRUD
        Task<IEnumerable<EmployeeEvaluationDto>> GetAllEvaluationsAsync();
        Task<EmployeeEvaluationDto?> GetEvaluationByIdAsync(int id);
        Task<EmployeeEvaluationDto> CreateEvaluationAsync(EmployeeEvaluationDto dto);
        Task<bool> UpdateEvaluationAsync(EmployeeEvaluationDto dto);
        Task<bool> DeleteEvaluationAsync(int id);

        // HR KPIs
        Task<HrKpisDto> GetHrKpisAsync();
    }
}
