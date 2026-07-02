using System.ComponentModel.DataAnnotations;

namespace NewFeature.Models
{
    public class TaskDependency
    {
        public int Id { get; set; }

        [Required]
        public int PredecessorTaskId { get; set; }

        [Required]
        public int SuccessorTaskId { get; set; }

        [Required]
        public DependencyType DependencyType { get; set; } = DependencyType.FinishToStart;

        // Navigation properties
        public Task? PredecessorTask { get; set; }
        public Task? SuccessorTask { get; set; }
    }
}
