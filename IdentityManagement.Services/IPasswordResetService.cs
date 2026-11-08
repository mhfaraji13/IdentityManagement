using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityManagement.Services
{
    public interface IPasswordResetService
    {
        Task<string?> GenerateOtpAsync(int userId);

        Task<bool> VerifyOtpAsync(int userId, string otp);

        Task<bool> ResetPasswordAsync(int userId, string newPassword);
    }
}
