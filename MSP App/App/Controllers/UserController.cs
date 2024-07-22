using App.Models;
using App.Services;
using Microsoft.AspNetCore.Mvc;
using SharedComponents.Entities;
using SharedComponents.WebEntities.Responses.UserResponses;

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

        [HttpGet]
        public async Task<IActionResult> EditAsync()
        {
            UserResponse? user = await _userService.GetByUserNameAsync(HttpContext.Session.GetString("Username"));
            if (user != null)
            {
                UserInfoViewModel userInfoViewModel = new UserInfoViewModel
                {
                    Username = user.UserName,
                    Email = user.Email
                };
                return View(userInfoViewModel);
            }
            TempData["ErrorMessage"] = $"(User) : User Not Found";
            return RedirectToAction("Login");
        }

    }
}
