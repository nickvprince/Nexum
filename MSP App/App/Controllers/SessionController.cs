using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SharedComponents.Entities.DbEntities;
using SharedComponents.Entities.WebAppEntities.Requests.SessionRequests;
using SharedComponents.Services.APIRequestServices.Interfaces;

namespace App.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SessionController : ControllerBase
    {
        private readonly IAPIRequestTenantService _tenantService;
        public SessionController(IAPIRequestTenantService tenantService)
        {
            _tenantService = tenantService;
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
                        return await Task.FromResult(Ok());
                    }

                    Tenant? tenant = await _tenantService.GetAsync(int.Parse(id));
                    if (tenant != null)
                    {
                        HttpContext.Session.SetString("ActiveTenantId", tenant.Id.ToString());
                        return await Task.FromResult(Ok());
                    }
                }
                
            }
            return await Task.FromResult(BadRequest("Invalid tenant Id."));
        }
    }
}
