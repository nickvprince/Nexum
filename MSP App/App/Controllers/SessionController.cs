using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SharedComponents.Entities.DbEntities;
using SharedComponents.Entities.WebAppEntities.Requests.SessionRequests;
using SharedComponents.Services.APIRequestServices.Interfaces;
using SharedComponents.Utilities;

namespace App.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class SessionController : ControllerBase
    {
        private readonly IAPIRequestTenantService _tenantService;
        private readonly IAPIRequestDeviceService _deviceService;
        public SessionController(IAPIRequestTenantService tenantService, IAPIRequestDeviceService deviceService)
        {
            _tenantService = tenantService;
            _deviceService = deviceService;
        }

        [HttpPost("Tenant/{id}")]
        public async Task<IActionResult> SetActiveTenant(string? id)
        {
            if (ModelState.IsValid)
            {
                if (id != null)
                {
                    if (id.Equals("All"))
                    {
                        HttpContext.Session.Remove("ActiveTenantId");
                        HttpContext.Session.Remove("ActiveDeviceId");
                        return await Task.FromResult(Ok());
                    }

                    Tenant? tenant = await _tenantService.GetAsync(int.Parse(id));
                    if (tenant != null)
                    {
                        HttpContext.Session.SetString("ActiveTenantId", tenant.Id.ToString());
                        HttpContext.Session.Remove("ActiveDeviceId");
                        return await Task.FromResult(Ok());
                    }
                }
                
            }
            return await Task.FromResult(BadRequest("Invalid tenant Id."));
        }

        [HttpPost("Device/{id}")]
        public async Task<IActionResult> SetActiveDevice(string? id)
        {
            if (ModelState.IsValid)
            {
                if (id != null)
                {
                    if (id.Equals("All"))
                    {
                        HttpContext.Session.Remove("ActiveDeviceId");
                        return await Task.FromResult(Ok());
                    }

                    Device? device = await _deviceService.GetAsync(int.Parse(id));
                    if (device != null)
                    {
                        HttpContext.Session.SetString("ActiveTenantId", device.TenantId.ToString());
                        HttpContext.Session.SetString("ActiveDeviceId", device.Id.ToString());
                        return await Task.FromResult(Ok());
                    }
                }
                
            }
            return await Task.FromResult(BadRequest("Invalid device Id."));
        }

        [HttpGet("Selector")]
        public async Task<IActionResult> GetTenantDeviceSelectorPartial()
        {
            ICollection<Tenant>? tenants = await _tenantService.GetAllAsync();
            if (tenants != null)
            {
                foreach (var tenant in tenants)
                {
                    tenant.Devices = await _deviceService.GetAllByTenantIdAsync(tenant.Id);
                }
                var partialViewString = await RenderUtilities.RenderViewToStringAsync(this, "_TenantDeviceSelectorPartial", tenants);
                return Ok(partialViewString);
            }
            return await Task.FromResult(NotFound());
        }
    }
}
