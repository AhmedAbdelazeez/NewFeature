using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NewFeature.Models
{
    public class Employee
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "English Full Name is required")]
        [StringLength(150, ErrorMessage = "English Full Name cannot exceed 150 characters")]
        public string FullNameEn { get; set; } = string.Empty;

        [Required(ErrorMessage = "Arabic Full Name is required")]
        [StringLength(150, ErrorMessage = "Arabic Full Name cannot exceed 150 characters")]
        public string FullNameAr { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone Number is required")]
        [StringLength(20, ErrorMessage = "Phone Number cannot exceed 20 characters")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Role is required")]
        [StringLength(100, ErrorMessage = "Role cannot exceed 100 characters")]
        public string Role { get; set; } = string.Empty;

        [Required(ErrorMessage = "Department is required")]
        public int DepartmentId { get; set; }

        [Required(ErrorMessage = "Join Date is required")]
        [DataType(DataType.Date)]
        public DateTime JoinDate { get; set; }

        [Required(ErrorMessage = "Salary is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Salary must be greater than zero")]
        public decimal Salary { get; set; }

        [Required(ErrorMessage = "Rating is required")]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
        public int Rating { get; set; }

        [Required]
        public bool IsSaudi { get; set; }

        [Required]
        public bool IsActive { get; set; } = true;

        public string? UserId { get; set; }

        // Navigation properties
        public Department? Department { get; set; }
        public ApplicationUser? User { get; set; }
        public ICollection<EmployeeEvaluation> Evaluations { get; set; } = new List<EmployeeEvaluation>();
    }
}
