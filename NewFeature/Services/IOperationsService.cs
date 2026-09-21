using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NewFeature.Models;

namespace NewFeature.Services
{
    public interface IOperationsService
    {
        // KPIs, computed from the approved dispatch log alone.
        Task<OperationsKpisDto> GetOperationsKpisAsync();

        // Index + CRUD over the uploaded dispatch log, for the Operations page.
        Task<PagedResultDto<OperationsDispatchRecordDto>> GetDispatchRecordsPagedAsync(int page, int pageSize, string? search, DateTime? fromDate, DateTime? toDate);
        Task<OperationsDispatchRecordDto?> GetDispatchRecordAsync(int id);
        Task<CrudResult<OperationsDispatchRecordDto>> CreateDispatchRecordAsync(OperationsDispatchRecordDto dto);
        Task<CrudResult<OperationsDispatchRecordDto>> UpdateDispatchRecordAsync(int id, OperationsDispatchRecordDto dto);
        Task<bool> DeleteDispatchRecordAsync(int id);

        // The department's single bulk upload: the approved Operations dispatch template.
        Task<ExcelImportResultDto> BulkUploadDispatchLogAsync(System.IO.Stream excelStream);
    }
}
