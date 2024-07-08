using App.Models;
using App.Services;
using Microsoft.AspNetCore.Mvc;
using SharedComponents.Entities;
using SharedComponents.RequestEntities;
using SharedComponents.WebEntities.Responses.UserResponses;

namespace App.Controllers
{
    public class AccountController : Controller
    {
        private readonly AccountService _accountService;
        private readonly UserService _userService;

        public AccountController(AccountService accountService, UserService userService)
        {
            _accountService = accountService;
            _userService = userService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> LoginAsync(LoginViewModel loginViewModel)
        {
            if (ModelState.IsValid)
            {
                ApplicationUser user = await _accountService.LoginAsync(loginViewModel.Username, loginViewModel.Password);
                if (user != null)
                {
                    HttpContext.Session.SetString("Username", user.UserName);
                    HttpContext.Session.SetString("Password", user.PasswordHash);
                    TempData["LastActionMessage"] = $"(Account) : Success";
                    return RedirectToAction("Index", "Home");
                }
                TempData["ErrorMessage"] = $"(Account) : Failed";
            }
            return View(loginViewModel);
        }

        [HttpGet]
        public async Task<IActionResult> EditAsync()
        {
            UserResponse? user = await _userService.GetByUserNameAsync(HttpContext.Session.GetString("Username"));
            if (user != null)
            {
                AccountViewModel accountViewModel = new AccountViewModel
                {
                    Username = user.UserName,
                    Email = user.Email
                };
                return View(accountViewModel);
            }
            TempData["ErrorMessage"] = $"(Account) : User Not Found";
            return RedirectToAction("Login");
        }

        [HttpPost]
        public async Task<IActionResult> EditAsync(AccountViewModel accountViewModel)
        {
            // Do Edit
            // Return to Page get
            return RedirectToAction("Edit");
        }

    }
}
