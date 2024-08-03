using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SharedComponents.Entities.DbEntities;
using SharedComponents.Services.APIRequestServices.Interfaces;

namespace App.Controllers
{
    [Authorize]
    [Route("[controller]")]
    public class JobController : Controller
    {
        private readonly IAPIRequestTenantService _tenantService;
        private readonly IAPIRequestDeviceService _deviceService;
        private readonly IAPIRequestNASServerService _nasServerService;
        private readonly IAPIRequestJobService _jobService;

        public JobController(IAPIRequestTenantService tenantService, IAPIRequestDeviceService deviceService,
                       IAPIRequestNASServerService nasServerService, IAPIRequestJobService jobService)
        {
            _tenantService = tenantService;
            _deviceService = deviceService;
            _nasServerService = nasServerService;
            _jobService = jobService;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            // Dynamically assign the return URL
            if (HttpContext != null)
            {
                HttpContext.Session.SetString("ReturnUrl", HttpContext.Request.Path);
                HttpContext.Session.SetString("ActiveNavLink", "jobLink");
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
                        var jobs = await _jobService.GetAllByTenantIdAsync(tenant.Id);
                        if(jobs != null)
                        {
                            foreach (var device in tenant.Devices)
                            {
                                device.Jobs = jobs.Where(j => j.DeviceId == device.Id).ToList();
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
                        t.Devices.Any(d => d.Jobs != null && d.Id == activeDeviceId)).ToList();
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

        [HttpGet("StatusCards")]
        public async Task<IActionResult> StatusCardsAsync()
        {
            return await Task.FromResult(PartialView("_JobStatusCardsPartial", FilterTenantsBySession(await PopulateTenantsAsync())));
        }
    }
}
