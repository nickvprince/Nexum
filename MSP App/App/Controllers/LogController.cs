using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SharedComponents.Entities.DbEntities;
using SharedComponents.Services.APIRequestServices.Interfaces;

namespace App.Controllers
{
    [Authorize]
    [Route("[controller]")]
    public class LogController : Controller
    {
        private readonly IAPIRequestTenantService _tenantService;
        private readonly IAPIRequestDeviceService _deviceService;
        private readonly IAPIRequestNASServerService _nasServerService;
        private readonly IAPIRequestLogService _logService;

        public LogController(IAPIRequestTenantService tenantService, IAPIRequestDeviceService deviceService,
                       IAPIRequestNASServerService nasServerService, IAPIRequestLogService logService)
        {
            _tenantService = tenantService;
            _deviceService = deviceService;
            _nasServerService = nasServerService;
            _logService = logService;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            // Dynamically assign the return URL
            if (HttpContext != null)
            {
                HttpContext.Session.SetString("ReturnUrl", HttpContext.Request.Path);
                HttpContext.Session.SetString("ActiveNavLink", "logLink");
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
                    tenant.NASServers = await _nasServerService.GetAllByTenantIdAsync(tenant.Id);
                    if (tenant.Devices != null)
                    {
                        var logs = await _logService.GetAllByTenantIdAsync(tenant.Id);
                        if (logs != null)
                        {
                            foreach (var device in tenant.Devices)
                            {
                                device.Logs = logs.Where(l => l.DeviceId == device.Id).ToList();
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
                        t.Devices.Any(d => d.Logs != null && d.Id == activeDeviceId)).ToList();
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

        [HttpGet("TypeCards")]
        public async Task<IActionResult> TypeCardsAsync()
        {
            return await Task.FromResult(PartialView("_LogTypeCardsPartial", FilterTenantsBySession(await PopulateTenantsAsync())));
        }

        [HttpGet("Table")]
        public async Task<IActionResult> TableAsync()
        {
            return await Task.FromResult(PartialView("_LogTablePartial", FilterTenantsBySession(await PopulateTenantsAsync())));
        }

        [HttpPost("{id}/Delete")]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            if (ModelState.IsValid)
            {
                if (await _logService.DeleteAsync(id))
                {
                    TempData["LastActionMessage"] = "Log deleted successfully.";
                    return Json(new { success = true, message = TempData["LastActionMessage"]?.ToString() });
                }
                TempData["ErrorMessage"] = "An error occurred while deleting the log.";
            }
            return Json(new { success = false, message = TempData["ErrorMessage"]?.ToString() });
        }
    }
}
