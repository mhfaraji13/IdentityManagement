using System.ComponentModel.DataAnnotations;

namespace IdentityManagement.WebApp.ViewModels
{
    public class MyProfileViewModel
    {
        public int Id { get; set; }

        [Required]
        public string UserName { get; set; } = string.Empty;

        public string? DisplayName { get; set; }

        [EmailAddress]
        public string? Email { get; set; }

        [RegularExpression(@"^\d{10}$",
            ErrorMessage = "Phone number must contain exactly 10 digits.")]
        public string? PhoneNumber { get; set; }
    }
}
