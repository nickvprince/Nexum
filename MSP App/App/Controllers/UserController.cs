using App.Models;
using App.Services;
using Microsoft.AspNetCore.Mvc;
using SharedComponents.Entities;

namespace App.Controllers
{
    public class UserController : Controller
    {
        private readonly UserService _userService;

        public UserController(UserService userService)
        {
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
