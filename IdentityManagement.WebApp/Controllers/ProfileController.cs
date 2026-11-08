using IdentityManagement.DomainClasses;
using IdentityManagement.Services;
using IdentityManagement.WebApp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace IdentityManagement.WebApp.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly IUsersService _usersService;

        public ProfileController(IUsersService usersService)
        {
            _usersService = usersService;
        }

        private int GetCurrentUserId()
        {
            var userId = User.FindFirstValue(ClaimTypes.UserData);
            return int.Parse(userId!);
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = await _usersService.FindUserAsync(GetCurrentUserId());

            if (user == null)
                return NotFound();

            var model = new MyProfileViewModel
            {
                Id = user.Id,
                UserName = user.UserName,
                DisplayName = user.DisplayName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber?.Replace("+98", "")
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Edit()
        {
            var user = await _usersService.FindUserAsync(GetCurrentUserId());

            if (user == null)
                return NotFound();

            var model = new MyProfileViewModel
            {
                Id = user.Id,
                UserName = user.UserName,
                DisplayName = user.DisplayName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber?.Replace("+98", "")
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(MyProfileViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _usersService.FindUserAsync(GetCurrentUserId());

            if (user == null)
                return NotFound();

            user.UserName = model.UserName;
            user.DisplayName = model.DisplayName;
            user.Email = model.Email;
            user.PhoneNumber = string.IsNullOrWhiteSpace(model.PhoneNumber)
                ? null
                : "+98" + model.PhoneNumber;

            var updated = await _usersService.UpdateUserAsync(user);

            if (!updated)
            {
                ModelState.AddModelError(
                    nameof(model.UserName),
                    "This username is already taken.");

                return View(model);
            }

            TempData["SuccessMessage"] = "Profile updated successfully.";

            return RedirectToAction(nameof(Index));
        }
    }
}
