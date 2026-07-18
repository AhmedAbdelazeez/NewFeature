using System;
using System.ComponentModel.DataAnnotations;

namespace NewFeature.Models
{
    public class TourismTour
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string TourNameAr { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string TourNameEn { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string GuideNameAr { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string GuideNameEn { get; set; } = string.Empty;

        [Required]
        public DateTime Date { get; set; }

        [Range(1, 1000)]
        public int PassengerCount { get; set; }

        [Range(0, 720)]
        public double BookingLeadTimeHours { get; set; }

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Scheduled"; // Scheduled, Completed, Cancelled
    }
}
