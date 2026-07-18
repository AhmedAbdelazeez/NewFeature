using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NewFeature.Models;
using NewFeature.Services.Repositories;

namespace NewFeature.Services
{
    public class TourismService : ITourismService
    {
        private readonly ApplicationDbContext _context;

        public TourismService(ApplicationDbContext context)
        {
            _context = context;
        }

        #region Hotel Bookings CRUD
        public async Task<IEnumerable<TourismHotelBookingDto>> GetAllHotelBookingsAsync()
        {
            return await _context.TourismHotelBookings
                .Select(b => MapToDto(b))
                .ToListAsync();
        }

        public async Task<TourismHotelBookingDto?> GetHotelBookingByIdAsync(int id)
        {
            var booking = await _context.TourismHotelBookings.FindAsync(id);
            return booking == null ? null : MapToDto(booking);
        }

        public async Task<TourismHotelBookingDto> CreateHotelBookingAsync(TourismHotelBookingDto dto)
        {
            var booking = new TourismHotelBooking
            {
                ClientNameAr = dto.ClientNameAr,
                ClientNameEn = dto.ClientNameEn,
                HotelNameAr = dto.HotelNameAr,
                HotelNameEn = dto.HotelNameEn,
                RoomCount = dto.RoomCount,
                CheckInDate = dto.CheckInDate,
                CheckOutDate = dto.CheckOutDate,
                Value = dto.Value,
                Status = dto.Status,
                GuestRating = dto.GuestRating
            };

            _context.TourismHotelBookings.Add(booking);
            await _context.SaveChangesAsync();
            dto.Id = booking.Id;
            return dto;
        }

        public async Task<bool> UpdateHotelBookingAsync(TourismHotelBookingDto dto)
        {
            var booking = await _context.TourismHotelBookings.FindAsync(dto.Id);
            if (booking == null) return false;

            booking.ClientNameAr = dto.ClientNameAr;
            booking.ClientNameEn = dto.ClientNameEn;
            booking.HotelNameAr = dto.HotelNameAr;
            booking.HotelNameEn = dto.HotelNameEn;
            booking.RoomCount = dto.RoomCount;
            booking.CheckInDate = dto.CheckInDate;
            booking.CheckOutDate = dto.CheckOutDate;
            booking.Value = dto.Value;
            booking.Status = dto.Status;
            booking.GuestRating = dto.GuestRating;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteHotelBookingAsync(int id)
        {
            var booking = await _context.TourismHotelBookings.FindAsync(id);
            if (booking == null) return false;

            _context.TourismHotelBookings.Remove(booking);
            await _context.SaveChangesAsync();
            return true;
        }
        #endregion

        #region Sightseeing Tours CRUD
        public async Task<IEnumerable<TourismTourDto>> GetAllToursAsync()
        {
            return await _context.TourismTours
                .Select(t => MapToDto(t))
                .ToListAsync();
        }

        public async Task<TourismTourDto?> GetTourByIdAsync(int id)
        {
            var tour = await _context.TourismTours.FindAsync(id);
            return tour == null ? null : MapToDto(tour);
        }

        public async Task<TourismTourDto> CreateTourAsync(TourismTourDto dto)
        {
            var tour = new TourismTour
            {
                TourNameAr = dto.TourNameAr,
                TourNameEn = dto.TourNameEn,
                GuideNameAr = dto.GuideNameAr,
                GuideNameEn = dto.GuideNameEn,
                Date = dto.Date,
                PassengerCount = dto.PassengerCount,
                BookingLeadTimeHours = dto.BookingLeadTimeHours,
                Status = dto.Status
            };

            _context.TourismTours.Add(tour);
            await _context.SaveChangesAsync();
            dto.Id = tour.Id;
            return dto;
        }

        public async Task<bool> UpdateTourAsync(TourismTourDto dto)
        {
            var tour = await _context.TourismTours.FindAsync(dto.Id);
            if (tour == null) return false;

            tour.TourNameAr = dto.TourNameAr;
            tour.TourNameEn = dto.TourNameEn;
            tour.GuideNameAr = dto.GuideNameAr;
            tour.GuideNameEn = dto.GuideNameEn;
            tour.Date = dto.Date;
            tour.PassengerCount = dto.PassengerCount;
            tour.BookingLeadTimeHours = dto.BookingLeadTimeHours;
            tour.Status = dto.Status;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteTourAsync(int id)
        {
            var tour = await _context.TourismTours.FindAsync(id);
            if (tour == null) return false;

            _context.TourismTours.Remove(tour);
            await _context.SaveChangesAsync();
            return true;
        }
        #endregion

        #region KPIs Calculation
        public async Task<TourismKpisDto> GetTourismKpisAsync()
        {
            var bookings = await _context.TourismHotelBookings.ToListAsync();
            var tours = await _context.TourismTours.ToListAsync();

            // Total room capacity limit for occupancy calculation
            const int totalRoomsCapacity = 300; 

            // 1. Hotel Occupancy Rate (%)
            var occupiedRooms = bookings.Where(b => b.Status == "Confirmed").Sum(b => b.RoomCount);
            var occupancyRate = totalRoomsCapacity > 0 ? ((double)occupiedRooms / totalRoomsCapacity) * 100.0 : 0.0;

            // 2. Booking Cancellation Rate (%)
            var totalBookingsCount = bookings.Count;
            var cancelledBookingsCount = bookings.Count(b => b.Status == "Cancelled");
            var cancellationRate = totalBookingsCount > 0 ? ((double)cancelledBookingsCount / totalBookingsCount) * 100.0 : 0.0;

            // 3. Average Guest Rating (1-5 stars)
            var ratedBookings = bookings.Where(b => b.GuestRating.HasValue).ToList();
            var avgRating = ratedBookings.Any() ? ratedBookings.Average(b => b.GuestRating!.Value) : 4.2;

            // 4. Tours Completed
            var completedTours = tours.Count(t => t.Status == "Completed");

            // 5. RevPAR (Revenue Per Available Room)
            var totalRevenue = bookings.Where(b => b.Status == "Confirmed").Sum(b => b.Value);
            var revPar = totalRoomsCapacity > 0 ? totalRevenue / totalRoomsCapacity : 0m;

            // 6. Booking Lead Time (Hours)
            var avgLeadTime = tours.Any() ? tours.Average(t => t.BookingLeadTimeHours) : 24.0;

            // 7. Active Tour Guides
            var activeGuides = tours.Where(t => t.Status != "Cancelled").Select(t => t.GuideNameEn).Distinct().Count();

            return new TourismKpisDto
            {
                HotelOccupancyRateActual = Math.Min(100.0, Math.Round(occupancyRate, 1)),
                HotelOccupancyRateTarget = 85.0,

                BookingCancellationRateActual = Math.Round(cancellationRate, 1),
                BookingCancellationRateTarget = 5.0,

                AverageGuestRatingActual = Math.Round(avgRating, 1),
                AverageGuestRatingTarget = 4.5,

                ToursCompletedActual = completedTours,
                ToursCompletedTarget = 15,

                RevParActual = Math.Round(revPar, 2),
                RevParTarget = 750.00m,

                BookingLeadTimeActual = Math.Round(avgLeadTime, 1),
                BookingLeadTimeTarget = 24.0,

                ActiveTourGuidesActual = activeGuides,
                ActiveTourGuidesTarget = 5
            };
        }
        #endregion

        #region Mappers
        private static TourismHotelBookingDto MapToDto(TourismHotelBooking b) => new()
        {
            Id = b.Id,
            ClientNameAr = b.ClientNameAr,
            ClientNameEn = b.ClientNameEn,
            HotelNameAr = b.HotelNameAr,
            HotelNameEn = b.HotelNameEn,
            RoomCount = b.RoomCount,
            CheckInDate = b.CheckInDate,
            CheckOutDate = b.CheckOutDate,
            Value = b.Value,
            Status = b.Status,
            GuestRating = b.GuestRating
        };

        private static TourismTourDto MapToDto(TourismTour t) => new()
        {
            Id = t.Id,
            TourNameAr = t.TourNameAr,
            TourNameEn = t.TourNameEn,
            GuideNameAr = t.GuideNameAr,
            GuideNameEn = t.GuideNameEn,
            Date = t.Date,
            PassengerCount = t.PassengerCount,
            BookingLeadTimeHours = t.BookingLeadTimeHours,
            Status = t.Status
        };
        #endregion
    }
}
