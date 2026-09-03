using System.Collections.Generic;
using System.Threading.Tasks;
using NewFeature.Models;

namespace NewFeature.Services
{
    public interface IEmployeeService
    {
        // Employee CRUD
        Task<IEnumerable<EmployeeDto>> GetAllEmployeesAsync();
        // Paginated + searchable listing for the Employees management page's roster table.
        // GetAllEmployeesAsync above is left untouched since the Evaluations tab's "select
        // employee" dropdown and other callers still expect the full unpaginated list.
        Task<PagedResultDto<EmployeeDto>> GetEmployeesPagedAsync(int page, int pageSize, string? search);
        Task<EmployeeDto?> GetEmployeeByIdAsync(int id);
        Task<EmployeeDto> CreateEmployeeAsync(EmployeeDto dto);
        Task<bool> UpdateEmployeeAsync(EmployeeDto dto);
        Task<bool> DeleteEmployeeAsync(int id);

        // Employee Evaluation CRUD
        Task<IEnumerable<EmployeeEvaluationDto>> GetAllEvaluationsAsync();
        // Paginated + searchable listing for the Employees page's Evaluations tab table.
        Task<PagedResultDto<EmployeeEvaluationDto>> GetEvaluationsPagedAsync(int page, int pageSize, string? search);
        Task<EmployeeEvaluationDto?> GetEvaluationByIdAsync(int id);
        Task<EmployeeEvaluationDto> CreateEvaluationAsync(EmployeeEvaluationDto dto);
        Task<bool> UpdateEvaluationAsync(EmployeeEvaluationDto dto);
        Task<bool> DeleteEvaluationAsync(int id);

        // HR KPIs
        Task<HrKpisDto> GetHrKpisAsync();

        // Bulk upload of the approved single-sheet employee roster (see DepartmentTemplates.Hr).
        // The stream must already be a readable .xlsx stream - see ExcelCompatibility.EnsureXlsxStream.
        Task<ExcelImportResultDto> BulkUploadEmployeesAsync(System.IO.Stream excelStream);
    }
}
