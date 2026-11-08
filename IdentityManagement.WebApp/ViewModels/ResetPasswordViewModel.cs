using System.ComponentModel.DataAnnotations;

namespace IdentityManagement.WebApp.ViewModels
{
    public class ResetPasswordViewModel
    {
        public int UserId { get; set; }

        [Required(ErrorMessage = "New password is required.")]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please confirm your password.")]
        [DataType(DataType.Password)]
        [Compare(nameof(NewPassword),
            ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
