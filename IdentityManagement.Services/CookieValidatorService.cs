using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Security.Claims;
using System.Text;

namespace IdentityManagement.Services
{
    public class CookieValidatorService(IUsersService usersService, IDeviceDetectionService deviceDetectionService) : ICookieValidatorService
    {
        private readonly IDeviceDetectionService _deviceDetectionService = deviceDetectionService ?? throw new ArgumentNullException(nameof(deviceDetectionService));
        private readonly IUsersService _usersService = usersService ?? throw new ArgumentNullException(nameof(usersService));


        public async Task ValidateAsync(CookieValidatePrincipalContext context)
        {
            ArgumentNullException.ThrowIfNull(context);

            var claimsIdentity = context.Principal?.Identity as ClaimsIdentity;

            if (claimsIdentity?.Claims == null || !claimsIdentity.Claims.Any())
            {
                await HandleUnauthorizedRequest(context);
                return;
            }

            if (!_deviceDetectionService.HasUserTokenValidDeviceDetails(claimsIdentity))

            {
                await HandleUnauthorizedRequest(context);
                return;

            }

            var serialNumberClaim = claimsIdentity.FindFirst(ClaimTypes.SerialNumber);
            if (serialNumberClaim == null)
            {
                await HandleUnauthorizedRequest(context);
                return;
            }
            var userIdString = claimsIdentity.FindFirst(ClaimTypes.UserData)?.Value;
            if (!int.TryParse(userIdString, System.Globalization.NumberStyles.Number, CultureInfo.InvariantCulture, out var userId))
            {
                await HandleUnauthorizedRequest(context);
                return;
            }
            var user = await _usersService.FindUserAsync(userId);
            if (user == null || !string.Equals(user.SerialNumber, serialNumberClaim.Value, StringComparison.Ordinal) || !user.IsActive)
            {
                await HandleUnauthorizedRequest(context);
            }

            await _usersService.UpdateUserLastActivityDateAsync(userId);
        }

        private static Task HandleUnauthorizedRequest(CookieValidatePrincipalContext context)
        {
            context.RejectPrincipal();

            return context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        }
    }
}

