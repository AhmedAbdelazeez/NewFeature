using System.ComponentModel.DataAnnotations;

namespace NewFeature.Models
{
    public class Client
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "English Name is required")]
        [StringLength(150, ErrorMessage = "English Name cannot exceed 150 characters")]
        public string NameEn { get; set; } = string.Empty;

        [Required(ErrorMessage = "Arabic Name is required")]
        [StringLength(150, ErrorMessage = "Arabic Name cannot exceed 150 characters")]
        public string NameAr { get; set; } = string.Empty;

        [Required(ErrorMessage = "Client Code is required")]
        [StringLength(20, ErrorMessage = "Code cannot exceed 20 characters")]
        public string Code { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email address is required")]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone number is required")]
        [Phone(ErrorMessage = "Invalid Phone Number")]
        [StringLength(20, ErrorMessage = "Phone cannot exceed 20 characters")]
        public string Phone { get; set; } = string.Empty;

        // Navigation properties
        public ICollection<ClientContact> Contacts { get; set; } = new List<ClientContact>();
        public ICollection<Contract> Contracts { get; set; } = new List<Contract>();
        public ICollection<Project> Projects { get; set; } = new List<Project>();
    }
}
