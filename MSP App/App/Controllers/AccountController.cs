using App.Models;
using App.Services;
using Microsoft.AspNetCore.Mvc;
using SharedComponents.RequestEntities;

namespace App.Controllers
{
    public class AccountController : Controller
    {
        private readonly AccountService _accountService;

        public AccountController(AccountService accountService)
        {
            _accountService = accountService;
        }

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
                var response = await _accountService.LoginAsync(loginViewModel.Username, loginViewModel.Password);
                if (response != null)
                {
                    TempData["LastActionMessage"] = $"(Account) : Success";
                    return View(loginViewModel);
                }
                TempData["ErrorMessage"] = $"(Account) : Failed";
            }
            return View(loginViewModel);
        }
    }
}
