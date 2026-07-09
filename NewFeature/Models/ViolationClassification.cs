using System.ComponentModel.DataAnnotations;

namespace NewFeature.Models
{
    public class ViolationClassification
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "English name is required")]
        [StringLength(150, ErrorMessage = "English name cannot exceed 150 characters")]
        public string NameEn { get; set; } = string.Empty;

        [Required(ErrorMessage = "Arabic name is required")]
        [StringLength(150, ErrorMessage = "Arabic name cannot exceed 150 characters")]
        public string NameAr { get; set; } = string.Empty;

        [Required(ErrorMessage = "Classification code is required")]
        [StringLength(20, ErrorMessage = "Classification code cannot exceed 20 characters")]
        public string Code { get; set; } = string.Empty;

        // Navigation properties
        public ICollection<Violation> Violations { get; set; } = new List<Violation>();
    }
}
