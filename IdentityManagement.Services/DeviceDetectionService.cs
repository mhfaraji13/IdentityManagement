using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using UAParser;

namespace IdentityManagement.Services
{
    public class DeviceDetectionService(ISecurityService securityService, IHttpContextAccessor httpContextAccessor) : IDeviceDetectionService
    {
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));

        private readonly ISecurityService _securityService = securityService ?? throw new ArgumentNullException(nameof(securityService));


        public string GetCurrentRequestDeviceDetails() => GetDeviceDetails(_httpContextAccessor.HttpContext);
        

        public string GetCurrentRequestDeviceDetailsHash()
        {
            throw new NotImplementedException();
        }

        public string? GetCurrentUserTokenDeviceDetailsHash()
        {
            throw new NotImplementedException();
        }

        public string GetDeviceDetails(HttpContext? context)
        {
            var ua = GetUserAgent(context);
            if (ua is null)
            {
                return "unKnown";
            }

        }

        public string GetDeviceDetailsHash(HttpContext? context)
        {
            throw new NotImplementedException();
        }

        public string? GetUserTokenDeviceDetailsHash(ClaimsIdentity? claimsIdentity)
        {
            throw new NotImplementedException();
        }

        public bool HasCurrentUserTokenValidDeviceDetails()
        {
            throw new NotImplementedException();
        }

        public bool HasUserTokenValidDeviceDetails(ClaimsIdentity? claimsIdentity)
        {
            throw new NotImplementedException();
        }
    }
}
