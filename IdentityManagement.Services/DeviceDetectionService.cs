using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using UAParser;

namespace IdentityManagement.Services
{
    public class DeviceDetectionService(
        ISecurityService securityService,
        IHttpContextAccessor httpContextAccessor,
        ILogger<DeviceDetectionService> logger) : IDeviceDetectionService
    {
        private static readonly Parser UaParser = Parser.GetDefault();

        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));

        private readonly ISecurityService _securityService = securityService ?? throw new ArgumentNullException(nameof(securityService));

        private readonly ILogger<DeviceDetectionService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));


        public string GetCurrentRequestDeviceDetails()
        {
            var details = GetDeviceDetails(_httpContextAccessor.HttpContext);
            _logger.LogInformation("Current request device details: {DeviceDetails}", details);
            return details;
        }


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
                return "unknown";
            }

            var client = UaParser.Parse(ua);
            return $"{client.UA.Family}, {client.OS.Family}, {client.Device.Family}";
        }

        private static string? GetUserAgent(HttpContext? context)
        {
            var ua = context?.Request.Headers[HeaderNames.UserAgent].ToString();
            return string.IsNullOrWhiteSpace(ua) ? null : ua;
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
