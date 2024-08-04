using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace App.Controllers
{
    [Authorize]
    public class RoleController : Controller
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            // Dynamically assign the return URL
            if (HttpContext != null)
            {
                HttpContext.Session.SetString("ReturnUrl", HttpContext.Request.Path);
                HttpContext.Session.SetString("ActiveNavLink", "roleLink");
            }
        }

        [HttpGet]
        public async Task<IActionResult> IndexAsync()
        {
            return await Task.FromResult(View());
        }
    }
}
