using Microsoft.AspNetCore.Identity;

namespace NewFeature.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FullNameEn { get; set; } = string.Empty;
        public string FullNameAr { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;

        public int? DepartmentId { get; set; }
        public Department? Department { get; set; }

        // Navigation properties
        public ICollection<ProjectMember> ProjectMembers { get; set; } = new List<ProjectMember>();
        public ICollection<Trip> Trips { get; set; } = new List<Trip>();
        public ICollection<Task> AssignedTasks { get; set; } = new List<Task>();
        public ICollection<TimeLog> TimeLogs { get; set; } = new List<TimeLog>();
    }
}
