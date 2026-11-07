using System.ComponentModel.DataAnnotations;

namespace IdentityManagement.WebApp.ViewModels
{
    public class EditUserViewModel
    {
        public int Id { get; set; }

        [Required]
        public string UserName { get; set; } = string.Empty;

        public string? DisplayName { get; set; }

        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        public string? Email { get; set; }

        [RegularExpression(
            @"^\d{10}$",
            ErrorMessage = "Phone number must contain exactly 10 digits.")]
        public string? PhoneNumber { get; set; }

        public bool IsActive { get; set; }
    }
}
