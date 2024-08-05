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
    [Route("[controller]")]
    public class AlertController : Controller
    {
        private readonly IAPIRequestTenantService _tenantService;
        private readonly IAPIRequestDeviceService _deviceService;
        private readonly IAPIRequestAlertService _alertService;

        public AlertController(IAPIRequestTenantService tenantService, IAPIRequestDeviceService deviceService,
                       IAPIRequestAlertService alertService)
        {
            _tenantService = tenantService;
            _deviceService = deviceService;
            _alertService = alertService;
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

        private async Task<ICollection<Tenant>?> PopulateTenantsAsync()
        {
            var tenants = await _tenantService.GetAllAsync();
            if (tenants != null)
            {
                foreach (var tenant in tenants)
                {
                    tenant.Devices = await _deviceService.GetAllByTenantIdAsync(tenant.Id);
                    if (tenant.Devices != null)
                    {
                        var alerts = await _alertService.GetAllByTenantIdAsync(tenant.Id);
                        if (alerts != null)
                        {
                            foreach (var device in tenant.Devices)
                            {
                                device.Alerts = alerts.Where(a => a.DeviceId == device.Id).ToList();
                            }
                        }
                    }
                }
            }
            return tenants;
        }

        private ICollection<Tenant>? FilterTenantsBySession(ICollection<Tenant>? tenants)
        {
            if (HttpContext.Session.GetString("ActiveDeviceId") != null)
            {
                int? activeDeviceId = int.Parse(HttpContext.Session.GetString("ActiveDeviceId"));
                return tenants?.Where(t => t.Devices != null &&
                        t.Devices.Any(d => d.Alerts != null && d.Id == activeDeviceId)).ToList();
            }
            if (HttpContext.Session.GetString("ActiveTenantId") != null)
            {
                int? activeTenantId = int.Parse(HttpContext.Session.GetString("ActiveTenantId"));
                return tenants?.Where(t => t.Id == activeTenantId).ToList();
            }
            return tenants;
        }

        [HttpGet]
        public async Task<IActionResult> IndexAsync()
        {
            return await Task.FromResult(View());
        }

        [HttpGet("SeverityCards")]
        public async Task<IActionResult> SeverityCardsAsync()
        {
            return await Task.FromResult(PartialView("_AlertSeverityCardsPartial", FilterTenantsBySession(await PopulateTenantsAsync())));
        }

        [HttpGet("Table")]
        public async Task<IActionResult> TableAsync()
        {
            return await Task.FromResult(PartialView("_AlertTablePartial", FilterTenantsBySession(await PopulateTenantsAsync())));
        }

        [HttpPost("{id}/Delete")]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            if (ModelState.IsValid)
            {
                if (await _alertService.DeleteAsync(id))
                {
                    TempData["LastActionMessage"] = "Alert deleted successfully.";
                    return Json(new { success = true, message = TempData["LastActionMessage"]?.ToString() });
                }
                TempData["ErrorMessage"] = "An error occurred while deleting the alert.";
            }
            return Json(new { success = false, message = TempData["ErrorMessage"]?.ToString() });
        }
    }
}
