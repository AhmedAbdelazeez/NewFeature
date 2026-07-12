using System;
using System.ComponentModel.DataAnnotations;

namespace NewFeature.Models
{
    public class EmployeeDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "English Name is required")]
        public string FullNameEn { get; set; } = string.Empty;

        [Required(ErrorMessage = "Arabic Name is required")]
        public string FullNameAr { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone Number is required")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Role is required")]
        public string Role { get; set; } = string.Empty;

        [Required(ErrorMessage = "Department is required")]
        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; } = string.Empty;
        public string DepartmentNameEn { get; set; } = string.Empty;
        public string DepartmentNameAr { get; set; } = string.Empty;

        [Required(ErrorMessage = "Join Date is required")]
        public DateTime JoinDate { get; set; }

        [Required(ErrorMessage = "Salary is required")]
        public decimal Salary { get; set; }

        [Required(ErrorMessage = "Rating is required")]
        public int Rating { get; set; }

        public bool IsSaudi { get; set; }
        public bool IsActive { get; set; } = true;

        public string? UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        
        public int AssignedTasksCount { get; set; }

        public string FullName { get; set; } = string.Empty;
    }

    public class EmployeeEvaluationDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Employee selection is required")]
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Evaluation Date is required")]
        public DateTime EvaluationDate { get; set; }

        [Required(ErrorMessage = "Evaluation Score is required")]
        public double EvaluationScore { get; set; }

        [Required(ErrorMessage = "English Notes are required")]
        public string NotesEn { get; set; } = string.Empty;

        [Required(ErrorMessage = "Arabic Notes are required")]
        public string NotesAr { get; set; } = string.Empty;

        public string Notes { get; set; } = string.Empty;
    }

    public class HrKpisDto
    {
        public double SaudizationRateActual { get; set; }
        public double SaudizationRateTarget { get; set; }

        public double RetentionRateActual { get; set; }
        public double RetentionRateTarget { get; set; }

        public double AvgRatingActual { get; set; }
        public double AvgRatingTarget { get; set; }

        public double AvgEvaluationActual { get; set; }
        public double AvgEvaluationTarget { get; set; }

        public int TotalEmployeesActual { get; set; }
        public int TotalEmployeesTarget { get; set; }

        public decimal AvgSalaryActual { get; set; }
        public decimal AvgSalaryTarget { get; set; }

        public double AvgTasksPerEmployeeActual { get; set; }
        public double AvgTasksPerEmployeeTarget { get; set; }
    }
}
