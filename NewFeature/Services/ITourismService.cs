using System.Collections.Generic;
using System.Threading.Tasks;
using NewFeature.Models;

namespace NewFeature.Services
{
    public interface ITourismService
    {
        // Hotel Bookings
        Task<IEnumerable<TourismHotelBookingDto>> GetAllHotelBookingsAsync();
        Task<TourismHotelBookingDto?> GetHotelBookingByIdAsync(int id);
        Task<TourismHotelBookingDto> CreateHotelBookingAsync(TourismHotelBookingDto dto);
        Task<bool> UpdateHotelBookingAsync(TourismHotelBookingDto dto);
        Task<bool> DeleteHotelBookingAsync(int id);

        // Sightseeing Tours
        Task<IEnumerable<TourismTourDto>> GetAllToursAsync();
        Task<TourismTourDto?> GetTourByIdAsync(int id);
        Task<TourismTourDto> CreateTourAsync(TourismTourDto dto);
        Task<bool> UpdateTourAsync(TourismTourDto dto);
        Task<bool> DeleteTourAsync(int id);

        // KPIs calculation
        Task<TourismKpisDto> GetTourismKpisAsync();
    }
}
