using System.ComponentModel.DataAnnotations;

namespace NewFeature.Models
{
    public class ProjectMilestone
    {
        public int Id { get; set; }

        [Required]
        public int ProjectId { get; set; }

        [Required(ErrorMessage = "English Title is required")]
        [StringLength(150, ErrorMessage = "English Title cannot exceed 150 characters")]
        public string TitleEn { get; set; } = string.Empty;

        [Required(ErrorMessage = "Arabic Title is required")]
        [StringLength(150, ErrorMessage = "Arabic Title cannot exceed 150 characters")]
        public string TitleAr { get; set; } = string.Empty;

        [Required(ErrorMessage = "English Description is required")]
        [StringLength(500, ErrorMessage = "English Description cannot exceed 500 characters")]
        public string DescriptionEn { get; set; } = string.Empty;

        [Required(ErrorMessage = "Arabic Description is required")]
        [StringLength(500, ErrorMessage = "Arabic Description cannot exceed 500 characters")]
        public string DescriptionAr { get; set; } = string.Empty;

        [Required(ErrorMessage = "Due Date is required")]
        [DataType(DataType.Date)]
        public DateTime DueDate { get; set; }

        public bool IsCompleted { get; set; } = false;

        // Navigation property
        public Project? Project { get; set; }
    }
}
