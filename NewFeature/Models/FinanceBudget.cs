using System;
using System.ComponentModel.DataAnnotations;

namespace NewFeature.Models
{
    public class FinanceBudget
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "English Department Name is required")]
        [StringLength(100, ErrorMessage = "English Department Name cannot exceed 100 characters")]
        public string DepartmentNameEn { get; set; } = string.Empty;

        [Required(ErrorMessage = "Arabic Department Name is required")]
        [StringLength(100, ErrorMessage = "Arabic Department Name cannot exceed 100 characters")]
        public string DepartmentNameAr { get; set; } = string.Empty;

        [Required(ErrorMessage = "Allocated Amount is required")]
        [Range(0.01, 1000000000.0, ErrorMessage = "Allocated Amount must be greater than 0")]
        public decimal AllocatedAmount { get; set; }

        [Required(ErrorMessage = "Spent Amount is required")]
        [Range(0.0, 1000000000.0, ErrorMessage = "Spent Amount must be 0 or greater")]
        public decimal SpentAmount { get; set; }

        [Required(ErrorMessage = "Year is required")]
        [Range(2020, 2100, ErrorMessage = "Year must be between 2020 and 2100")]
        public int Year { get; set; } = DateTime.UtcNow.Year;
    }
}
