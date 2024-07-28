using App.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SharedComponents.Entities;
using SharedComponents.Entities.DbEntities;
using SharedComponents.Services.APIRequestServices.Interfaces;

namespace App.Controllers
{
    [Authorize]
    public class AlertController : Controller
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IAPIRequestAlertService _alertService;

        public AlertController(IAPIRequestAlertService alertService, IHttpContextAccessor httpContextAccessor)
        {
            _alertService = alertService;
            _httpContextAccessor = httpContextAccessor;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            // Dynamically assign the return URL
            if (HttpContext != null)
            {
                HttpContext.Session.SetString("ReturnUrl", HttpContext.Request.Path);
                HttpContext.Session.SetString("ActiveNavLink", "alertLink");
            }
        }

        [HttpGet]
        public async Task<IActionResult> IndexAsync()
        {
            return await Task.FromResult(View());
        }

        [HttpGet]
        public async Task<IActionResult> Table()
        {
            ICollection<DeviceAlert>? alerts = await _alertService.GetAllAsync();
            return PartialView("_AlertTablePartial", alerts);
        }
    }
}
