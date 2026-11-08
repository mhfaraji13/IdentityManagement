using IdentityManagement.DataLayer.Context;
using IdentityManagement.DomainClasses;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace IdentityManagement.Services
{
    public class PasswordResetService : IPasswordResetService
    {
        private readonly DbSet<PasswordResetOtp> _otps;
        private readonly DbSet<User> _users;
        private readonly IUnitOfWork _uow;
        private readonly ISecurityService _securityService;

        public PasswordResetService(
            IUnitOfWork uow,
            ISecurityService securityService)
        {
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
            _securityService = securityService ??
                throw new ArgumentNullException(nameof(securityService));

            _otps = _uow.Set<PasswordResetOtp>();
            _users = _uow.Set<User>();
        }

        public async Task<string?> GenerateOtpAsync(int userId)
        {
            var userExists = await _users.AnyAsync(x => x.Id == userId);

            if (!userExists)
                return null;

            var oldOtps = await _otps
                .Where(x => x.UserId == userId && !x.IsUsed)
                .ToListAsync();

            foreach (var oldOtp in oldOtps)
            {
                oldOtp.IsUsed = true;
            }

            var otp = RandomNumberGenerator.GetInt32(100000, 1000000)
                .ToString();

            var now = DateTime.Now;

            var otpRecord = new PasswordResetOtp
            {
                UserId = userId,
                CodeHash = _securityService.GetSha256Hash(otp),
                CreatedAt = now,
                ExpiresAt = now.AddMinutes(5),
                Attempts = 0,
                IsUsed = false,
                IsVerified = false
            };

            await _otps.AddAsync(otpRecord);
            await _uow.SaveChangesAsync();

            return otp;
        }

        public async Task<bool> VerifyOtpAsync(int userId, string otp)
        {
            var otpRecord = await _otps
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefaultAsync();

            if (otpRecord == null ||
                otpRecord.IsUsed ||
                otpRecord.IsVerified ||
                otpRecord.ExpiresAt <= DateTime.Now ||
                otpRecord.Attempts >= 5)
            {
                return false;
            }

            otpRecord.Attempts++;

            var enteredCodeHash =
                _securityService.GetSha256Hash(otp);

            if (otpRecord.CodeHash != enteredCodeHash)
            {
                await _uow.SaveChangesAsync();
                return false;
            }

            otpRecord.IsVerified = true;

            await _uow.SaveChangesAsync();

            return true;
        }

        public async Task<bool> ResetPasswordAsync(
            int userId,
            string newPassword)
        {
            var user = await _users.FirstOrDefaultAsync(
                x => x.Id == userId);

            if (user == null)
                return false;

            var otpRecord = await _otps
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefaultAsync();

            if (otpRecord == null ||
                otpRecord.IsUsed ||
                !otpRecord.IsVerified ||
                otpRecord.ExpiresAt <= DateTime.Now)
            {
                return false;
            }

            user.Password =
                _securityService.GetSha256Hash(newPassword);

            user.SerialNumber = Guid.NewGuid().ToString("N");

            otpRecord.IsUsed = true;
            otpRecord.IsVerified = false;

            await _uow.SaveChangesAsync();

            return true;
        }
    }
}
