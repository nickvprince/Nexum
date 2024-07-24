using App.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using SharedComponents.Services.APIRequestServices.Interfaces;
using SharedComponents.Entities.WebEntities.Requests.AuthRequests;
using SharedComponents.Entities.WebEntities.Responses.AuthResponses;
using Microsoft.AspNetCore.Mvc.Filters;
using SharedComponents.Utilities;

namespace App.Controllers
{
    public class AuthController : Controller
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IAPIRequestAuthService _authService;
        private readonly IAPIRequestUserService _userService;

        public AuthController(IAPIRequestAuthService authService, IAPIRequestUserService userService, IHttpContextAccessor httpContextAccessor)
        {
            _authService = authService;
            _userService = userService;
            _httpContextAccessor = httpContextAccessor;
        }

        [HttpGet]
        public async Task<IActionResult> IndexAsync(string? returnUrl = null)
        {
            var users = await _userService.GetAllAsync();
            if(HttpContext.User.Identity != null)
            {
                if (HttpContext.User.Identity.IsAuthenticated)
                {
                    return RedirectToAction("Index", "Home");
                }
            }
            if(returnUrl != null)
            {
                HttpContext.Session.SetString("ReturnUrl", URLUtilities.CapitalizeFirstLetterAfterSlashes(returnUrl));
            }
            return await Task.FromResult(View());
        }

        [HttpPost]
        public async Task<IActionResult> IndexAsync(AuthViewModel authViewModel, string? returnUrl = null)
        {
            if (ModelState.IsValid)
            {
                AuthLoginRequest request = new AuthLoginRequest
                {
                    Username = authViewModel.Username,
                    Password = authViewModel.Password
                };
                AuthLoginResponse? response = await _authService.LoginAsync(request);
                if (response != null)
                {
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, authViewModel.Username!),
                        new Claim("Token", response.Token),
                        new Claim("RefreshToken", response.RefreshToken),
                        new Claim("Expires", response.Expires.ToString())
                    };
                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                    var authProperties = new AuthenticationProperties
                    {
                        IsPersistent = true,
                        ExpiresUtc = response.Expires
                    };

                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);
                    
                    TempData["LastActionMessage"] = $"(Auth) : Success";

                    if (Url.IsLocalUrl(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }
                    else
                    {
                        return RedirectToAction("Index", "Home");
                    }
                }
                TempData["ErrorMessage"] = $"(Auth) : Failed";
            }
            return View(authViewModel);
        }
    }
}
