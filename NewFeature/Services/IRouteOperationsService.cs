using System.Threading.Tasks;
using NewFeature.Models;

namespace NewFeature.Services
{
    // Route Operations is deliberately its own small service, separate from FleetService, so its
    // Excel import and KPI logic can be reviewed, built, and tested in isolation rather than as
    // one more region inside a large shared class. Route CRUD (create/edit/delete a single route
    // from the UI) stays on IFleetService for now, since that part already works and isn't part of
    // this module's scope.
    public interface IRouteOperationsService
    {
        Task<ExcelImportResultDto> BulkUploadRoutesAsync(System.IO.Stream excelStream);
        Task<RouteOperationsKpisDto> GetRouteOperationsKpisAsync();
    }
}
