using System.ComponentModel.DataAnnotations;

namespace IdentityManagement.WebApp.ViewModels
{
    public class VerifyOtpViewModel
    {
        public int UserId { get; set; }

        [Required(ErrorMessage = "OTP is required.")]
        [RegularExpression(@"^\d{6}$",
            ErrorMessage = "OTP must contain exactly 6 digits.")]
        public string Otp { get; set; } = string.Empty;
    }
}
