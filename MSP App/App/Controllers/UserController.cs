using App.Models;
using App.Services;
using Microsoft.AspNetCore.Mvc;
using SharedComponents.Entities;

namespace App.Controllers
{
    public class UserController : Controller
    {
        private readonly PermissionService _permissionService;
        private readonly UserService _userService;

        public UserController(PermissionService permissionService, UserService userService)
        {
            _permissionService = permissionService;
            _userService = userService;
        }

        [HttpGet]
        public async Task<IActionResult> IndexAsync()
        {
            UserViewModel userViewModel = new UserViewModel
            {
                Users = await _userService.GetAllAsync()
            };
            return View(userViewModel);
        }

        [HttpGet]
        public async Task<IActionResult> PermissionsAsync()
        {
            User user = await _userService.GetAsync(HttpContext.Session.GetString("Username"));
            if (user != null)
            {
                List<Permission> permissions = await _permissionService.GetAllAsync(); // Change to By ID
                if (permissions.Any())
                {
                    user.Permissions = permissions;
                }
            }
            return View(user);
        }
    }
}
