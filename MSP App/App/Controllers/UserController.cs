using App.Models;
using App.Services;
using Microsoft.AspNetCore.Mvc;

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
    }
}
