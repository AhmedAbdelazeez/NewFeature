using System.ComponentModel.DataAnnotations;

namespace NewFeature.Models
{
    public class Route
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "English Route Name is required")]
        [StringLength(150, ErrorMessage = "English Name cannot exceed 150 characters")]
        public string NameEn { get; set; } = string.Empty;

        [Required(ErrorMessage = "Arabic Route Name is required")]
        [StringLength(150, ErrorMessage = "Arabic Name cannot exceed 150 characters")]
        public string NameAr { get; set; } = string.Empty;

        [Required(ErrorMessage = "English Start Location is required")]
        [StringLength(200, ErrorMessage = "English Start Location cannot exceed 200 characters")]
        public string StartLocationEn { get; set; } = string.Empty;

        [Required(ErrorMessage = "Arabic Start Location is required")]
        [StringLength(200, ErrorMessage = "Arabic Start Location cannot exceed 200 characters")]
        public string StartLocationAr { get; set; } = string.Empty;

        [Required(ErrorMessage = "English End Location is required")]
        [StringLength(200, ErrorMessage = "English End Location cannot exceed 200 characters")]
        public string EndLocationEn { get; set; } = string.Empty;

        [Required(ErrorMessage = "Arabic End Location is required")]
        [StringLength(200, ErrorMessage = "Arabic End Location cannot exceed 200 characters")]
        public string EndLocationAr { get; set; } = string.Empty;

        [Required(ErrorMessage = "Distance is required")]
        [Range(0.1, 100000.0, ErrorMessage = "Distance must be greater than zero")]
        public decimal DistanceKm { get; set; }

        // Navigation property
        public ICollection<Trip> Trips { get; set; } = new List<Trip>();
    }
}
