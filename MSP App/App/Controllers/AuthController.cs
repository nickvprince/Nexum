using App.Models;
using App.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SharedComponents.Entities;
using SharedComponents.RequestEntities;
using SharedComponents.WebEntities.Requests.AuthRequests;
using SharedComponents.WebEntities.Responses.AuthResponses;
using SharedComponents.WebEntities.Responses.UserResponses;

namespace App.Controllers
{
    public class AuthController : Controller
    {
        private readonly AuthService _authService;
        private readonly UserService _userService;

        public AuthController(AuthService authService, UserService userService)
        {
            _authService = authService;
            _userService = userService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> IndexAsync(AuthViewModel loginViewModel)
        {
            if (ModelState.IsValid)
            {
                AuthLoginRequest request = new AuthLoginRequest
                {
                    Username = loginViewModel.Username,
                    Password = loginViewModel.Password
                };

                AuthLoginResponse? response = await _authService.LoginAsync(request);
                if (response != null)
                {
                    HttpContext.Session.SetString("Username", loginViewModel.Username!);
                    HttpContext.Session.SetString("Token", response.Token);
                    HttpContext.Session.SetString("RefreshToken", response.RefreshToken);
                    HttpContext.Session.SetString("Expires", response.Expires.ToString());
                    TempData["LastActionMessage"] = $"(Auth) : Success";
                    return RedirectToAction("Index", "Home");
                }
                TempData["ErrorMessage"] = $"(Auth) : Failed";
            }
            return View(loginViewModel);
        }

        

    }
}
