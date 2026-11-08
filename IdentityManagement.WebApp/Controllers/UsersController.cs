using IdentityManagement.DomainClasses;
using IdentityManagement.Services;
using IdentityManagement.WebApp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IdentityManagement.WebApp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UsersController(
    IUsersService usersService,
    IRolesService rolesService) : Controller
    {
        private readonly IUsersService _usersService = usersService;
        private readonly IRolesService _rolesService = rolesService;

        public async Task<IActionResult> Index()
        {
            var users = await _usersService.GetAllUsersAsync();

            return View(users);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateUserViewModel model)
        {
            if (!ModelState.IsValid)
            {
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
                    : $"+98{model.PhoneNumber}"
            };

            var result = await _usersService.CreateUserAsync(user);

            if (!result)
            {
                ModelState.AddModelError(
                    nameof(model.UserName),
                    "This username is already taken.");

                return View(model);
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var user = await _usersService.GetUserForEditAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            var model = new EditUserViewModel
            {
                Id = user.Id,
                UserName = user.UserName,
                DisplayName = user.DisplayName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber?.StartsWith("+98") == true
                    ? user.PhoneNumber[3..]
                    : user.PhoneNumber,
                IsActive = user.IsActive
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            EditUserViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = new User
            {
                Id = model.Id,
                UserName = model.UserName,
                DisplayName = model.DisplayName,
                Password = string.Empty,
                Email = model.Email,
                PhoneNumber = string.IsNullOrWhiteSpace(model.PhoneNumber)
                    ? null
                    : $"+98{model.PhoneNumber}",
                IsActive = model.IsActive
            };

            var result = await _usersService.UpdateUserAsync(user);

            if (!result)
            {
                ModelState.AddModelError(
                    nameof(model.UserName),
                    "Username already exists or user was not found.");

                return View(model);
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var currentUserId = User.FindFirst(
                System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (int.TryParse(currentUserId, out var loggedInUserId) &&
                loggedInUserId == id)
            {
                TempData["ErrorMessage"] = "You cannot delete your own account.";
                return RedirectToAction(nameof(Index));
            }

            var result = await _usersService.DeleteUserAsync(id);

            if (!result)
            {
                return NotFound();
            }

            TempData["SuccessMessage"] = "User deleted successfully.";

            return RedirectToAction(nameof(Index));
        }


        [HttpGet]
        public async Task<IActionResult> ManageRoles(int id)
        {
            var user = await _usersService.GetUserForEditAsync(id);

            if (user == null)
                return NotFound();

            var allRoles = await _rolesService.GetAllRolesAsync();
            var userRoles = await _rolesService.FindUserRolesAsync(id);

            var model = new ManageUserRolesViewModel
            {
                UserId = user.Id,
                UserName = user.UserName,
                Roles = allRoles.Select(role => new RoleItemViewModel
                {
                    RoleId = role.Id,
                    RoleName = role.Name,
                    IsSelected = userRoles.Any(
                        userRole => userRole.Id == role.Id)
                }).ToList()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ManageRoles(
            ManageUserRolesViewModel model)
        {
            var currentUserId = User.FindFirst(
                System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (int.TryParse(currentUserId, out var loggedInUserId) &&
                loggedInUserId == model.UserId)
            {
                TempData["ErrorMessage"] =
                    "You cannot change your own roles.";

                return RedirectToAction(nameof(Index));
            }

            var user = await _usersService.GetUserForEditAsync(model.UserId);

            if (user == null)
                return NotFound();

            var allRoles = await _rolesService.GetAllRolesAsync();
            var currentRoles =
                await _rolesService.FindUserRolesAsync(model.UserId);

            var selectedRoleIds = model.Roles
                .Where(x => x.IsSelected)
                .Select(x => x.RoleId)
                .ToHashSet();

            var currentRoleIds = currentRoles
                .Select(x => x.Id)
                .ToHashSet();

            foreach (var role in allRoles)
            {
                if (selectedRoleIds.Contains(role.Id) &&
                    !currentRoleIds.Contains(role.Id))
                {
                    await _rolesService.AddUserToRoleAsync(
                        model.UserId, role.Id);
                }
                else if (!selectedRoleIds.Contains(role.Id) &&
                         currentRoleIds.Contains(role.Id))
                {
                    await _rolesService.RemoveUserFromRoleAsync(
                        model.UserId, role.Id);
                }
            }

            TempData["SuccessMessage"] = "User roles updated successfully.";

            return RedirectToAction(nameof(ManageRoles),
                new { id = model.UserId });
        }

    }
}