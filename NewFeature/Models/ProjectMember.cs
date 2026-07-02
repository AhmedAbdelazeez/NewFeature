using System.ComponentModel.DataAnnotations;

namespace NewFeature.Models
{
    public class ProjectMember
    {
        public int Id { get; set; }

        [Required]
        public int ProjectId { get; set; }

        [Required(ErrorMessage = "User selection is required")]
        public string UserId { get; set; } = string.Empty;

        [Required(ErrorMessage = "English Role description is required")]
        [StringLength(100, ErrorMessage = "English Role cannot exceed 100 characters")]
        public string RoleEn { get; set; } = string.Empty;

        [Required(ErrorMessage = "Arabic Role description is required")]
        [StringLength(100, ErrorMessage = "Arabic Role cannot exceed 100 characters")]
        public string RoleAr { get; set; } = string.Empty;

        // Navigation properties
        public Project? Project { get; set; }
        public ApplicationUser? User { get; set; }
    }
}
