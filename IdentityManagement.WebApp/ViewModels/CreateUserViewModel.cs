using System.ComponentModel.DataAnnotations;

namespace IdentityManagement.WebApp.ViewModels
{
    public class CreateUserViewModel
    {
        [Required]
        public string UserName { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        public string? DisplayName { get; set; }

        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        public string? Email { get; set; }

        [RegularExpression(
            @"^\d{10}$",
            ErrorMessage = "Phone number must contain exactly 10 digits.")]
        public string? PhoneNumber { get; set; }
    }
}
