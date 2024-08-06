using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SharedComponents.Entities.DbEntities;
using SharedComponents.Entities.WebAppEntities.Requests.SessionRequests;
using SharedComponents.Services.APIRequestServices.Interfaces;
using SharedComponents.Utilities;

namespace App.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class SessionController : ControllerBase
    {
        private readonly IAPIRequestTenantService _tenantService;
        private readonly IAPIRequestDeviceService _deviceService;
        public SessionController(IAPIRequestTenantService tenantService, IAPIRequestDeviceService deviceService)
        {
            _tenantService = tenantService;
            _deviceService = deviceService;
        }

        [HttpGet("Tenant")]
        public async Task<IActionResult> GetActiveTenant()
        {
            string? activeTenantId = HttpContext.Session.GetString("ActiveTenantId");
            if (activeTenantId != null)
            {
                Tenant? tenant = await _tenantService.GetAsync(int.Parse(activeTenantId));
                if (tenant != null)
                {
                    return await Task.FromResult(Ok(new { activeTenantId }));
                }
            }
            return await Task.FromResult(Ok(new { activeTenantId = (int?)null }));
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

        [HttpGet("Device")]
        public async Task<IActionResult> GetActiveDevice()
        {
            string? activeDeviceId = HttpContext.Session.GetString("ActiveDeviceId");
            if (activeDeviceId != null)
            {
                Device? device = await _deviceService.GetAsync(int.Parse(activeDeviceId));
                if (device != null)
                {
                    return await Task.FromResult(Ok(new { activeDeviceId }));
                }
            }
            return await Task.FromResult(Ok(new { activeDeviceId = (int?)null }));
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
            }
            var partialViewString = await RenderUtilities.RenderViewToStringAsync(this, "_TenantDeviceSelectorPartial", tenants);
            return Ok(partialViewString);
        }
    }
}
