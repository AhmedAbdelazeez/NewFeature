using System;
using System.ComponentModel.DataAnnotations;

namespace NewFeature.Models
{
    public class TourismHotelBooking
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string ClientNameAr { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string ClientNameEn { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string HotelNameAr { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string HotelNameEn { get; set; } = string.Empty;

        [Range(1, 1000)]
        public int RoomCount { get; set; }

        [Required]
        public DateTime CheckInDate { get; set; }

        [Required]
        public DateTime CheckOutDate { get; set; }

        [Required]
        public decimal Value { get; set; }

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Confirmed"; // Confirmed, Cancelled

        [Range(1, 5)]
        public int? GuestRating { get; set; }
    }
}
