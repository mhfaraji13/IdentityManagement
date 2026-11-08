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
     IPasswordResetService passwordResetService,
     IConfiguration configuration,
     IWebHostEnvironment environment) : Controller
    {
        private readonly IUsersService _usersService = usersService;
        private readonly IRolesService _rolesService = rolesService;
        private readonly IDeviceDetectionService _deviceDetectionService = deviceDetectionService;
        private readonly IConfiguration _configuration = configuration;
        private readonly IPasswordResetService _passwordResetService =
    passwordResetService;

        private readonly IWebHostEnvironment _environment = environment;

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
                    "The username or password is incorrect.");

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

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Register()
        {
            return View(new RegisterViewModel());
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var roles = await _rolesService.GetAllRolesAsync();

            var userRole = roles.FirstOrDefault(
                x => x.Name == CustomRoles.User);

            if (userRole == null)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Default user role is not available.");

                return View(model);
            }

            var user = new User
            {
                UserName = model.UserName,
                Password = model.Password,
                DisplayName = model.DisplayName,
                Email = model.Email,
                PhoneNumber = string.IsNullOrWhiteSpace(model.PhoneNumber)
                    ? null
                    : "+98" + model.PhoneNumber
            };

            var created = await _usersService.CreateUserAsync(user);

            if (!created)
            {
                ModelState.AddModelError(
                    nameof(model.UserName),
                    "This username is already taken.");

                return View(model);
            }

            var createdUser = await _usersService.FindByUserNameAsync(
                model.UserName);

            if (createdUser == null)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Unable to complete registration.");

                return View(model);
            }

            var roleAssigned = await _rolesService.AddUserToRoleAsync(
                createdUser.Id,
                userRole.Id);

            if (!roleAssigned)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Unable to assign the default user role.");

                return View(model);
            }

            TempData["SuccessMessage"] =
                "Your account has been created successfully. Please login.";

            return RedirectToAction(nameof(Login));
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View(new ForgotPasswordViewModel());
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(
            ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _usersService.FindByUserNameAsync(
                model.UserName);

            if (user == null || !user.IsActive)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "If the account exists, a password reset process can be started.");

                return View(model);
            }

            var otp = await _passwordResetService.GenerateOtpAsync(user.Id);

            if (otp == null)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Unable to start the password reset process.");

                return View(model);
            }

            TempData["PasswordResetUserId"] = user.Id;

            if (_environment.IsDevelopment())
            {
                TempData["DevelopmentOtp"] = otp;
            }

            return RedirectToAction(nameof(VerifyOtp));
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult VerifyOtp()
        {
            if (TempData.Peek("PasswordResetUserId") is not int userId)
                return RedirectToAction(nameof(ForgotPassword));

            var model = new VerifyOtpViewModel
            {
                UserId = userId
            };

            return View(model);
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyOtp(VerifyOtpViewModel model)
        {
            if (TempData.Peek("PasswordResetUserId") is not int userId)
                return RedirectToAction(nameof(ForgotPassword));

            model.UserId = userId;

            if (!ModelState.IsValid)
                return View(model);

            var isValid = await _passwordResetService.VerifyOtpAsync(
                userId,
                model.Otp);

            if (!isValid)
            {
                ModelState.AddModelError(
                    nameof(model.Otp),
                    "Invalid or expired OTP.");

                return View(model);
            }

            TempData.Remove("PasswordResetUserId");
            TempData["VerifiedPasswordResetUserId"] = userId;

            return RedirectToAction(nameof(ResetPassword));
        }


        [AllowAnonymous]
        [HttpGet]
        public IActionResult ResetPassword()
        {
            if (TempData.Peek("VerifiedPasswordResetUserId") is not int userId)
                return RedirectToAction(nameof(ForgotPassword));

            return View(new ResetPasswordViewModel
            {
                UserId = userId
            });
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(
    ResetPasswordViewModel model)
        {
            if (TempData.Peek("VerifiedPasswordResetUserId") is not int userId)
                return RedirectToAction(nameof(ForgotPassword));

            model.UserId = userId;

            if (!ModelState.IsValid)
                return View(model);

            var result = await _passwordResetService.ResetPasswordAsync(
                userId,
                model.NewPassword);

            if (!result)
            {
                TempData.Remove("VerifiedPasswordResetUserId");

                ModelState.AddModelError(
                    string.Empty,
                    "Password reset failed. Please start the process again.");

                return View(model);
            }

            TempData.Remove("VerifiedPasswordResetUserId");

            TempData["SuccessMessage"] =
                "Your password has been reset successfully. Please login.";

            return RedirectToAction(nameof(Login));
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





