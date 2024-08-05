using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SharedComponents.Entities.DbEntities;
using SharedComponents.Services.APIRequestServices.Interfaces;

namespace App.Controllers
{
    [Authorize]
    [Route("[controller]")]
    public class PermissionController : Controller
    {
        private readonly IAPIRequestPermissionService _permissionService;

        public PermissionController(IAPIRequestPermissionService permissionService)
        {
            _permissionService = permissionService;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            // Dynamically assign the return URL
            if (HttpContext != null)
            {
                HttpContext.Session.SetString("ReturnUrl", HttpContext.Request.Path);
                HttpContext.Session.SetString("ActiveNavLink", "permissionLink");
            }
        }

        [HttpGet]
        public async Task<IActionResult> IndexAsync()
        {
            return await Task.FromResult(View());
        }

        [HttpGet("Table")]
        public async Task<IActionResult> TableAsync()
        {
            return await Task.FromResult(PartialView("_PermissionTablePartial", await _permissionService.GetAllAsync()));
        }
    }
}
