using System.ComponentModel.DataAnnotations;

namespace NewFeature.Models
{
    public class ContractItem
    {
        public int Id { get; set; }

        [Required]
        public int ContractId { get; set; }

        [Required(ErrorMessage = "English Description is required")]
        [StringLength(500, ErrorMessage = "English Description cannot exceed 500 characters")]
        public string DescriptionEn { get; set; } = string.Empty;

        [Required(ErrorMessage = "Arabic Description is required")]
        [StringLength(500, ErrorMessage = "Arabic Description cannot exceed 500 characters")]
        public string DescriptionAr { get; set; } = string.Empty;

        [Required(ErrorMessage = "Quantity is required")]
        [Range(0.001, double.MaxValue, ErrorMessage = "Quantity must be greater than zero")]
        public decimal Quantity { get; set; }

        [Required(ErrorMessage = "Unit Price is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Unit Price must be greater than zero")]
        public decimal UnitPrice { get; set; }

        public decimal TotalPrice { get; set; }

        // Navigation property
        public Contract? Contract { get; set; }
    }
}
