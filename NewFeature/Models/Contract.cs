using System.ComponentModel.DataAnnotations;

namespace NewFeature.Models
{
    public class Contract
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Client selection is required")]
        public int ClientId { get; set; }

        [Required(ErrorMessage = "Contract Number is required")]
        [StringLength(50, ErrorMessage = "Contract Number cannot exceed 50 characters")]
        public string ContractNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "English Title is required")]
        [StringLength(200, ErrorMessage = "English Title cannot exceed 200 characters")]
        public string TitleEn { get; set; } = string.Empty;

        [Required(ErrorMessage = "Arabic Title is required")]
        [StringLength(200, ErrorMessage = "Arabic Title cannot exceed 200 characters")]
        public string TitleAr { get; set; } = string.Empty;

        [Required(ErrorMessage = "Start Date is required")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "End Date is required")]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }

        [Required(ErrorMessage = "Total Amount is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Total Amount must be greater than zero")]
        public decimal TotalAmount { get; set; }

        // Navigation properties
        public Client? Client { get; set; }
        public ICollection<ContractItem> ContractItems { get; set; } = new List<ContractItem>();
    }
}
