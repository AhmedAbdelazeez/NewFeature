using System;
using System.ComponentModel.DataAnnotations;

namespace NewFeature.Models
{
    public class EmployeeEvaluation
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Employee selection is required")]
        public int EmployeeId { get; set; }

        [Required(ErrorMessage = "Evaluation Date is required")]
        [DataType(DataType.Date)]
        public DateTime EvaluationDate { get; set; }

        [Required(ErrorMessage = "Evaluation Score is required")]
        [Range(0.0, 100.0, ErrorMessage = "Evaluation Score must be between 0 and 100")]
        public double EvaluationScore { get; set; }

        [Required(ErrorMessage = "English Notes are required")]
        [StringLength(1000, ErrorMessage = "English Notes cannot exceed 1000 characters")]
        public string NotesEn { get; set; } = string.Empty;

        [Required(ErrorMessage = "Arabic Notes are required")]
        [StringLength(1000, ErrorMessage = "Arabic Notes cannot exceed 1000 characters")]
        public string NotesAr { get; set; } = string.Empty;

        // Navigation property
        public Employee? Employee { get; set; }
    }
}
