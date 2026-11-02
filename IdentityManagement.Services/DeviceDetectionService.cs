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

        /// <summary>
        ///  darkhast joziiyat-e dastgah-e request-e al'an
        /// </summary>
        /// <returns></returns>
        public string GetCurrentRequestDeviceDetails() => GetDeviceDetails(_httpContextAccessor.HttpContext);
        /// <summary>
        /// gereftan-e hash shodeye joziiyat-e dastgahe request-e fe'li
        /// </summary>
        /// <returns></returns>

        public string GetCurrentRequestDeviceDetailsHash()
        {
            return GetDeviceDetailsHash(_httpContextAccessor.HttpContext);
        }
        /// <summary>
        /// gereftan-e hash shodeye joziiyat-e dastgahe fe'li --- Token-user
        /// </summary>
        /// <returns></returns>
        public string? GetCurrentUserTokenDeviceDetailsHash()
        {
            return GetUserTokenDeviceDetailsHash(_httpContextAccessor.HttpContext?.User.Identity as ClaimsIdentity);
        }
        /// <summary>
        /// gereftan-e joziiyat-e dastgah
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public string GetDeviceDetails(HttpContext? context)
        {
            var ua = GetUserAgent(context);
            if (ua is null)
            {
                return "unKnown";
            }
            var client = Parser.GetDefault().Parse(ua);
            var deviceInfo = client.Device.Family;
            var browserInfo = $"{client.UA.Family}, {client.UA.Major}.{client.UA.Minor}";
            var osInfo = $"{client.OS.Family} , {client.OS.Major}.{client.OS.Minor}";
            return $"{deviceInfo},{browserInfo},{osInfo}";
        }
        /// <summary>
        /// gereftan-e hash shodeye joziiyat-e dastgah
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>

        public string GetDeviceDetailsHash(HttpContext? context)
        {
            return _securityService.GetSha256Hash(GetDeviceDetails(context));
        }
        /// <summary>
        /// gereftan-e hash shodeye joziiyat-e dastgah ba token user
        /// </summary>
        /// <param name="claimsIdentity"></param>
        /// <returns></returns>
        public string? GetUserTokenDeviceDetailsHash(ClaimsIdentity? claimsIdentity)
        {
            if (claimsIdentity == null)
            {
                return null;
            }
            if (claimsIdentity.Claims == null)
            {
                return null;
            }
            if (!claimsIdentity.Claims.Any())
            {
                return null;
            }
            Claim? systemClaim = claimsIdentity.FindFirst(ClaimTypes.System);
            if (systemClaim == null)
            {
                return null;
            }
            return systemClaim.Value;
        }
        /// <summary>
        /// aya user fe'li token-e joziiiyat-e dastgahesh valid shode
        /// </summary>
        /// <returns></returns>
        public bool HasCurrentUserTokenValidDeviceDetails()
        {
            return HasUserTokenValidDeviceDetails(_httpContextAccessor.HttpContext?.User.Identity as ClaimsIdentity);
        }
        /// <summary>
        /// aya user tokenesh valid shode babat-e joziiyat-e dastgah 
        /// </summary>
        /// <param name="claimsIdentity"></param>
        /// <returns></returns>
        public bool HasUserTokenValidDeviceDetails(ClaimsIdentity? claimsIdentity)
        {
            return string.Equals(GetCurrentRequestDeviceDetailsHash(), GetUserTokenDeviceDetailsHash(claimsIdentity), StringComparison.Ordinal);
        }

        private static string? GetUserAgent(HttpContext? context)
        {
            if (context == null)
            {
                return null;
            }
            return context.Request.Headers.TryGetValue(HeaderNames.UserAgent, out var userAgent) ? userAgent.ToString() : null;
        }
    }
}
