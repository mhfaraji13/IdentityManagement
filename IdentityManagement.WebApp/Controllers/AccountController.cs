using IdentityManagement.DomainClasses;
using IdentityManagement.Services;
using IdentityManagement.WebApp.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Security.Claims;

namespace IdentityManagement.WebApp.Controllers
{
    public class AccountController(
    IUsersService usersService,
    IRolesService rolesService,
    IDeviceDetectionService deviceDetectionService,
    IConfiguration configuration) : Controller
    {
        private readonly IUsersService _usersService = usersService;
        private readonly IRolesService _rolesService = rolesService;
        private readonly IDeviceDetectionService _deviceDetectionService = deviceDetectionService;
        private readonly IConfiguration _configuration = configuration;

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _usersService.CheckCredsAsync(
                model.UserName,
                model.Password);

            if (user?.IsActive != true)
            {
                await HttpContext.SignOutAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme);

                ModelState.AddModelError(
                    string.Empty,
                    "نام کاربری یا رمز عبور اشتباه است.");

                return View(model);
            }

            var claimsPrincipal = await CreateCookieClaimsAsync(user);

            var expirationDays = _configuration.GetValue<int>(
                "LoginCookieExpirationDays", 30);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                claimsPrincipal,
                new AuthenticationProperties
                {
                    IsPersistent = model.RememberMe,
                    IssuedUtc = DateTimeOffset.UtcNow,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddDays(expirationDays)
                });

            await _usersService.UpdateUserLastActivityDateAsync(user.Id);

            if (!string.IsNullOrWhiteSpace(returnUrl) &&
                Url.IsLocalUrl(returnUrl))
            {
                return LocalRedirect(returnUrl);
            }

            return RedirectToAction("Index", "Dashboard");
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction(nameof(Login));
        }

        private async Task<ClaimsPrincipal> CreateCookieClaimsAsync(User user)
        {
            var identity = new ClaimsIdentity(
                CookieAuthenticationDefaults.AuthenticationScheme);

            identity.AddClaim(new Claim(
                ClaimTypes.NameIdentifier,
                user.Id.ToString(CultureInfo.InvariantCulture)));

            identity.AddClaim(new Claim(
                ClaimTypes.Name,
                user.UserName));

            identity.AddClaim(new Claim(
                "DisplayName",
                user.DisplayName ?? string.Empty));

            identity.AddClaim(new Claim(
                ClaimTypes.SerialNumber,
                user.SerialNumber ?? string.Empty));

            identity.AddClaim(new Claim(
                ClaimTypes.System,
                _deviceDetectionService.GetCurrentRequestDeviceDetailsHash(),
                ClaimValueTypes.String));

            identity.AddClaim(new Claim(
                ClaimTypes.UserData,
                user.Id.ToString(CultureInfo.InvariantCulture)));

            var roles = await _rolesService.FindUserRolesAsync(user.Id);

            foreach (var role in roles)
            {
                identity.AddClaim(new Claim(ClaimTypes.Role, role.Name));
            }

            return new ClaimsPrincipal(identity);
        }
    }
}





