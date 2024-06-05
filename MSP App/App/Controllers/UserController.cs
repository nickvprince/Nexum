using App.Models;
using App.Services;
using Microsoft.AspNetCore.Mvc;
using SharedComponents.Entities;

namespace App.Controllers
{
    public class UserController : Controller
    {
        private readonly UserPermissionSetService _userPermissionSetService;
        private readonly UserService _userService;

        public UserController(UserPermissionSetService userPermissionSetService, UserService userService)
        {
            _userPermissionSetService = userPermissionSetService;
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
        public async Task<IActionResult> UserPermissionSetsAsync()
        {
            User? user = await _userService.GetAsync(HttpContext.Session.GetString("Username"));
            if (user != null)
            {
                ICollection<UserPermissionSet> permissionSets = await _userPermissionSetService.GetAllAsync();
                if (permissionSets.Any())
                {
                    user.UserPermissionSets = permissionSets;
                }
            }
            return View(user);
        }

    }
}
