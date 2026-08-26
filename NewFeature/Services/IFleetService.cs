using System.Collections.Generic;
using System.Threading.Tasks;
using NewFeature.Models;

namespace NewFeature.Services
{
    public interface IFleetService
    {
        // Vehicles
        Task<IEnumerable<VehicleDto>> GetAllVehiclesAsync();
        // Paginated + search listing for the Vehicles management page (702+ real vehicles won't
        // fit in one client-side table). GetAllVehiclesAsync above is left untouched and still
        // returns the full unpaginated list, since Trips and Maintenance both depend on it to
        // populate a vehicle dropdown.
        Task<PagedResultDto<VehicleDto>> GetVehiclesPagedAsync(int page, int pageSize, string? search);
        Task<VehicleDto?> GetVehicleByIdAsync(int id);
        Task<VehicleDto> CreateVehicleAsync(VehicleDto dto);
        Task<bool> UpdateVehicleAsync(VehicleDto dto);
        Task<bool> DeleteVehicleAsync(int id);
        Task<ExcelImportResultDto> BulkUploadVehiclesAsync(System.IO.Stream excelStream);
        Task<FleetKpisDto> GetFleetKpisAsync();

        // Routes (single-record CRUD only - bulk upload and KPIs moved to IRouteOperationsService
        // as its own isolated module; see RouteOperationsService.cs)
        Task<IEnumerable<RouteDto>> GetAllRoutesAsync();
        Task<RouteDto?> GetRouteByIdAsync(int id);
        Task<RouteDto> CreateRouteAsync(RouteDto dto);
        Task<bool> UpdateRouteAsync(RouteDto dto);
        Task<bool> DeleteRouteAsync(int id);

        // Trips
        Task<IEnumerable<TripDto>> GetAllTripsAsync();
        Task<TripDto?> GetTripByIdAsync(int id);
        Task<TripDto> CreateTripAsync(TripDto dto);
        Task<bool> UpdateTripAsync(TripDto dto);
        Task<bool> DeleteTripAsync(int id);
        Task<(int SuccessCount, List<string> Errors)> BulkUploadTripsAsync(System.IO.Stream excelStream);
    }
}
