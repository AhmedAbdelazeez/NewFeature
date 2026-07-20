using System.Collections.Generic;
using System.Threading.Tasks;
using NewFeature.Models;

namespace NewFeature.Services
{
    public interface IFleetService
    {
        // Vehicles
        Task<IEnumerable<VehicleDto>> GetAllVehiclesAsync();
        Task<VehicleDto?> GetVehicleByIdAsync(int id);
        Task<VehicleDto> CreateVehicleAsync(VehicleDto dto);
        Task<bool> UpdateVehicleAsync(VehicleDto dto);
        Task<bool> DeleteVehicleAsync(int id);
        Task<(int SuccessCount, List<string> Errors)> BulkUploadVehiclesAsync(System.IO.Stream excelStream);

        // Routes
        Task<IEnumerable<RouteDto>> GetAllRoutesAsync();
        Task<RouteDto?> GetRouteByIdAsync(int id);
        Task<RouteDto> CreateRouteAsync(RouteDto dto);
        Task<bool> UpdateRouteAsync(RouteDto dto);
        Task<bool> DeleteRouteAsync(int id);
        Task<(int SuccessCount, List<string> Errors)> BulkUploadRoutesAsync(System.IO.Stream excelStream);

        // Trips
        Task<IEnumerable<TripDto>> GetAllTripsAsync();
        Task<TripDto?> GetTripByIdAsync(int id);
        Task<TripDto> CreateTripAsync(TripDto dto);
        Task<bool> UpdateTripAsync(TripDto dto);
        Task<bool> DeleteTripAsync(int id);
        Task<(int SuccessCount, List<string> Errors)> BulkUploadTripsAsync(System.IO.Stream excelStream);
    }
}
