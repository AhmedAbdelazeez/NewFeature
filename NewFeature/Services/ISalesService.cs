using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using NewFeature.Models;

namespace NewFeature.Services
{
    public interface ISalesService
    {
        // The three uploads, one per Sales page. Each stream must already be a valid .xlsx stream
        // (see ExcelCompatibility.EnsureXlsxStream). Re-uploading replaces what the file covers - a
        // fiscal year's roster, the whole fleet snapshot, an execution date's operations - so a
        // corrected file never double counts.
        Task<ExcelImportResultDto> BulkUploadCustomersAsync(Stream excelStream, string fileName, string uploadedByUserId);
        Task<ExcelImportResultDto> BulkUploadFleetCapacityAsync(Stream excelStream, string fileName, string uploadedByUserId);
        Task<ExcelImportResultDto> BulkUploadDailyOperationsAsync(Stream excelStream, string fileName, string uploadedByUserId);

        // The combined workbook (all three sheets in one file): runs the three uploads above over it.
        Task<ExcelImportResultDto> BulkUploadCombinedAsync(byte[] xlsxBytes, string fileName, string uploadedByUserId);

        Task<PagedResultDto<SalesImportBatchDto>> GetImportHistoryAsync(int page, int pageSize);

        // Customers (العملاء)
        Task<PagedResultDto<SalesCustomerDto>> GetCustomersPagedAsync(int page, int pageSize, string? search, int? fiscalYear, string? customerGroup);
        Task<SalesCustomerFilterOptionsDto> GetCustomerFilterOptionsAsync();
        Task<List<SalesCustomerDto>> GetLatestYearCustomersAsync();
        Task<SalesCustomerDto?> GetCustomerAsync(int id);
        Task<CrudResult<SalesCustomerDto>> CreateCustomerAsync(SalesCustomerDto dto);
        Task<CrudResult<SalesCustomerDto>> UpdateCustomerAsync(int id, SalesCustomerDto dto);
        Task<bool> DeleteCustomerAsync(int id);

        // Fleet capacity (الطاقة الاستيعابية)
        Task<PagedResultDto<SalesFleetCapacityDto>> GetFleetPagedAsync(int page, int pageSize, string? search, string? category, string? busType);
        Task<SalesFleetFilterOptionsDto> GetFleetFilterOptionsAsync();
        Task<List<SalesFleetCapacityDto>> GetAllFleetAsync();
        Task<SalesFleetCapacityDto?> GetFleetItemAsync(int id);
        Task<CrudResult<SalesFleetCapacityDto>> CreateFleetItemAsync(SalesFleetCapacityDto dto);
        Task<CrudResult<SalesFleetCapacityDto>> UpdateFleetItemAsync(int id, SalesFleetCapacityDto dto);
        Task<bool> DeleteFleetItemAsync(int id);

        // Daily operations (التشغيل اليومي)
        Task<PagedResultDto<SalesDailyOperationDto>> GetDailyOperationsPagedAsync(int page, int pageSize, string? search, DateTime? fromDate, DateTime? toDate, string? requestType, string? executionPoint);
        Task<SalesOperationFilterOptionsDto> GetOperationFilterOptionsAsync();
        Task<SalesDailyOperationDto?> GetDailyOperationAsync(int id);
        Task<CrudResult<SalesDailyOperationDto>> CreateDailyOperationAsync(SalesDailyOperationDto dto);
        Task<CrudResult<SalesDailyOperationDto>> UpdateDailyOperationAsync(int id, SalesDailyOperationDto dto);
        Task<bool> DeleteDailyOperationAsync(int id);

        Task<SalesKpisDto> GetSalesKpisAsync();
    }
}
