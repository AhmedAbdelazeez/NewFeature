using System.ComponentModel.DataAnnotations;

namespace NewFeature.Models
{
    public class Task
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
        [StringLength(1000, ErrorMessage = "English Description cannot exceed 1000 characters")]
        public string DescriptionEn { get; set; } = string.Empty;

        [Required(ErrorMessage = "Arabic Description is required")]
        [StringLength(1000, ErrorMessage = "Arabic Description cannot exceed 1000 characters")]
        public string DescriptionAr { get; set; } = string.Empty;

        [DataType(DataType.Date)]
        public DateTime? StartDate { get; set; }

        [Required(ErrorMessage = "Due Date is required")]
        [DataType(DataType.Date)]
        public DateTime DueDate { get; set; }

        [Required(ErrorMessage = "Estimated Hours is required")]
        [Range(0.0, 1000.0, ErrorMessage = "Estimated hours must be between 0 and 1000")]
        public decimal EstimatedHours { get; set; }

        [Required(ErrorMessage = "Task Status is required")]
        public TaskStatus Status { get; set; } = TaskStatus.ToDo;

        public string? AssignedToUserId { get; set; }

        // Navigation properties
        public Project? Project { get; set; }
        public ApplicationUser? AssignedToUser { get; set; }
        public ICollection<TaskDependency> PredecessorDependencies { get; set; } = new List<TaskDependency>();
        public ICollection<TaskDependency> SuccessorDependencies { get; set; } = new List<TaskDependency>();
        public ICollection<TimeLog> TimeLogs { get; set; } = new List<TimeLog>();
    }
}
