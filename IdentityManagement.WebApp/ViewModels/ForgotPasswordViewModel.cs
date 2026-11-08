using System.ComponentModel.DataAnnotations;

namespace IdentityManagement.WebApp.ViewModels
{
    public class ForgotPasswordViewModel
    {
        [Required(ErrorMessage = "Username is required.")]
        public string UserName { get; set; } = string.Empty;
    }
}
