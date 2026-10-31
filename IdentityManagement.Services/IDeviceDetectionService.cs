using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;

namespace IdentityManagement.Services
{
    public interface IDeviceDetectionService
    {
        string GetDeviceDetails(HttpContext? context);
        string GetCurrentRequestDeviceDetails();
        string GetDeviceDetailsHash(HttpContext? context);
        string GetCurrentRequestDeviceDetailsHash();
        string? GetUserTokenDeviceDetailsHash(ClaimsIdentity? claimsIdentity);
        string? GetCurrentUserTokenDeviceDetailsHash();
    }
}
