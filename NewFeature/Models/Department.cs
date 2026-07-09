using System.ComponentModel.DataAnnotations;

namespace NewFeature.Models
{
    public class Department
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "English name is required")]
        [StringLength(150, ErrorMessage = "English name cannot exceed 150 characters")]
        public string NameEn { get; set; } = string.Empty;

        [Required(ErrorMessage = "Arabic name is required")]
        [StringLength(150, ErrorMessage = "Arabic name cannot exceed 150 characters")]
        public string NameAr { get; set; } = string.Empty;

        [Required(ErrorMessage = "Department code is required")]
        [StringLength(20, ErrorMessage = "Department code cannot exceed 20 characters")]
        public string Code { get; set; } = string.Empty;

        [Required]
        public bool IsCompliant { get; set; } = true;

        // Navigation properties
        public ICollection<Violation> Violations { get; set; } = new List<Violation>();
        public ICollection<InternalAudit> Audits { get; set; } = new List<InternalAudit>();
        public ICollection<ImprovementAction> ImprovementActions { get; set; } = new List<ImprovementAction>();
    }
}
