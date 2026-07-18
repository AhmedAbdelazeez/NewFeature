using System;

namespace NewFeature.Models
{
    public class TourismHotelBookingDto
    {
        public int Id { get; set; }
        public string ClientNameAr { get; set; } = string.Empty;
        public string ClientNameEn { get; set; } = string.Empty;
        public string HotelNameAr { get; set; } = string.Empty;
        public string HotelNameEn { get; set; } = string.Empty;
        public int RoomCount { get; set; }
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public decimal Value { get; set; }
        public string Status { get; set; } = "Confirmed";
        public int? GuestRating { get; set; }
    }

    public class TourismTourDto
    {
        public int Id { get; set; }
        public string TourNameAr { get; set; } = string.Empty;
        public string TourNameEn { get; set; } = string.Empty;
        public string GuideNameAr { get; set; } = string.Empty;
        public string GuideNameEn { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public int PassengerCount { get; set; }
        public double BookingLeadTimeHours { get; set; }
        public string Status { get; set; } = "Scheduled";
    }

    public class TourismKpisDto
    {
        public double HotelOccupancyRateActual { get; set; }
        public double HotelOccupancyRateTarget { get; set; }

        public double BookingCancellationRateActual { get; set; }
        public double BookingCancellationRateTarget { get; set; }

        public double AverageGuestRatingActual { get; set; }
        public double AverageGuestRatingTarget { get; set; }

        public int ToursCompletedActual { get; set; }
        public int ToursCompletedTarget { get; set; }

        public decimal RevParActual { get; set; }
        public decimal RevParTarget { get; set; }

        public double BookingLeadTimeActual { get; set; }
        public double BookingLeadTimeTarget { get; set; }

        public int ActiveTourGuidesActual { get; set; }
        public int ActiveTourGuidesTarget { get; set; }
    }
}
