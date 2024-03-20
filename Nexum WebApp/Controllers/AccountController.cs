using Microsoft.AspNetCore.Mvc;
using Nexum_WebApp_Class_Library.Models;

namespace Nexum_WebApp.Controllers
{
    public class AccountController : Controller
    {
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(AccountLoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                return RedirectToAction("Index", "Menu");
            }
            ModelState.AddModelError(string.Empty, "Please enter a username and password.");
            return View(model);
        }
    }
}
