using System;
using System.IO;
using System.Threading.Tasks;
using NewFeature.Models;

namespace NewFeature.Services
{
    public interface ISalesService
    {
        // Bulk uploads. Each returns a result carrying the import batch id, row counts and any
        // per-row validation errors. The stream must already be a valid .xlsx stream (see
        // ExcelCompatibility.EnsureXlsxStream) and the caller is responsible for disposing it.
        Task<SalesImportResultDto> BulkUploadCustomerRosterAsync(Stream excelStream, byte[] rawBytes, string fileName, string uploadedByUserId);
        Task<SalesImportResultDto> BulkUploadFleetCapacityAsync(Stream excelStream, byte[] rawBytes, string fileName, string uploadedByUserId);
        Task<SalesImportResultDto> BulkUploadDailyOperationsAsync(Stream excelStream, byte[] rawBytes, string fileName, string uploadedByUserId);

        // Single-template entry point: runs all three parsers above against the same workbook. Each
        // parser already locates its own target sheet by header content (not position), so a combined
        // workbook containing all three sheet shapes just works - no parser changes needed.
        Task<SalesImportResultDto> BulkUploadCombinedAsync(Stream excelStream, byte[] rawBytes, string fileName, string uploadedByUserId);

        // Best-effort structural detection of which of the three known Sales workbook shapes a file
        // matches, based on sheet names/headers - never the filename alone (see PART 3 item 17).
        Task<SalesFileType?> DetectFileTypeAsync(Stream excelStream);

        Task<PagedResultDto<SalesImportBatchDto>> GetImportHistoryAsync(int page, int pageSize);

        Task<SalesCustomersOverviewDto> GetCustomersOverviewAsync(int? year);
        Task<SalesFleetCapacitySummaryDto> GetFleetCapacitySummaryAsync(string? category, string? busType, string? model);
        Task<PagedResultDto<SalesDailyOperationDto>> GetDailyOperationsAsync(int page, int pageSize, DateTime? fromDate, DateTime? toDate);

        Task<SalesKpisDto> GetSalesKpisAsync();
    }
}
