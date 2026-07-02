using System.ComponentModel.DataAnnotations;

namespace NewFeature.Models
{
    public class ClientContact
    {
        public int Id { get; set; }

        [Required]
        public int ClientId { get; set; }

        [Required(ErrorMessage = "English Name is required")]
        [StringLength(150, ErrorMessage = "English Name cannot exceed 150 characters")]
        public string NameEn { get; set; } = string.Empty;

        [Required(ErrorMessage = "Arabic Name is required")]
        [StringLength(150, ErrorMessage = "Arabic Name cannot exceed 150 characters")]
        public string NameAr { get; set; } = string.Empty;

        [Required(ErrorMessage = "English Job Title is required")]
        [StringLength(100, ErrorMessage = "English Job Title cannot exceed 100 characters")]
        public string JobTitleEn { get; set; } = string.Empty;

        [Required(ErrorMessage = "Arabic Job Title is required")]
        [StringLength(100, ErrorMessage = "Arabic Job Title cannot exceed 100 characters")]
        public string JobTitleAr { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone is required")]
        [Phone(ErrorMessage = "Invalid Phone Number")]
        [StringLength(20, ErrorMessage = "Phone cannot exceed 20 characters")]
        public string Phone { get; set; } = string.Empty;

        // Navigation property
        public Client? Client { get; set; }
    }
}
