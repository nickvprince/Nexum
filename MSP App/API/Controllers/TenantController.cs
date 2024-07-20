using Microsoft.AspNetCore.Mvc;
using SharedComponents.Entities.DbEntities;
using SharedComponents.Entities.WebEntities.Requests.TenantRequests;
using SharedComponents.Handlers.Attributes.HasPermission;
using SharedComponents.Handlers.Results;
using SharedComponents.Services.APIServices.Interfaces;
using SharedComponents.Services.DbServices.Interfaces;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "v1-Web")]
    public class TenantController : ControllerBase
    {
        private readonly IDbTenantService _dbTenantService;
        private readonly IAPIAuthService _authService;

        public TenantController(IDbTenantService dbTenantService, IAPIAuthService authService)
        {
            _dbTenantService = dbTenantService;
            _authService = authService;
        }

        [HttpPost("")]
        [HasPermission("Tenant.Create.Permission", PermissionType.System)]
        public async Task<IActionResult> CreateAsync([FromBody] TenantCreateRequest request)
        {
            if (ModelState.IsValid)
            {
                Tenant? tenant = new Tenant
                {
                    Name = request.Name,
                    TenantInfo = new TenantInfo
                    {
                        Name = request.ContactName,
                        Email = request.ContactEmail,
                        Phone = request.ContactPhone,
                        Address = request.Address,
                        City = request.City,
                        State = request.State,
                        Zip = request.Zip,
                        Country = request.Country
                    }
                };
                tenant = await _dbTenantService.CreateAsync(tenant);
                if (tenant != null)
                {
                    return Ok(tenant);
                }
                return BadRequest("An error occurred while creating the tenant.");
            }
            return BadRequest("Invalid request.");
        }

        [HttpPut("")]
        [HasPermission("Tenant.Update.Permission", PermissionType.Tenant)]
        public async Task<IActionResult> UpdateAsync([FromBody] TenantUpdateRequest request)
        {
            if (ModelState.IsValid)
            {
                // Authentication check using roles + permissions
                if (!await _authService.UserHasPermissionAsync<TenantController>(Request.Headers["Authorization"].ToString(), request.Id))
                {
                    return new CustomForbidResult("User do not have access to this feature for the specified tenant");
                }
                // --- End of authentication check ---
                Tenant? tenant = await _dbTenantService.GetAsync(request.Id);
                if (tenant == null)
                {
                    return NotFound("Tenant not found.");
                }
                if (tenant.TenantInfo == null)
                {
                    return NotFound("TenantInfo not found.");
                }
                tenant.Name = request.Name;
                tenant.TenantInfo.Name = request.ContactName;
                tenant.TenantInfo.Email = request.ContactEmail;
                tenant.TenantInfo.Phone = request.ContactPhone;
                tenant.TenantInfo.Address = request.Address;
                tenant.TenantInfo.City = request.City;
                tenant.TenantInfo.State = request.State;
                tenant.TenantInfo.Zip = request.Zip;
                tenant.TenantInfo.Country = request.Country;
                tenant = await _dbTenantService.UpdateAsync(tenant);
                if (tenant != null)
                {
                    return Ok(tenant);
                }
                return BadRequest("An error occurred while updating the tenant.");
            }
            return BadRequest("Invalid request.");
        }

        [HttpDelete("{id}")]
        [HasPermission("Tenant.Delete.Permission", PermissionType.Tenant)]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            if (ModelState.IsValid)
            {
                // Authentication check using roles + permissions
                if (!await _authService.UserHasPermissionAsync<TenantController>(Request.Headers["Authorization"].ToString(), id))
                {
                    return new CustomForbidResult("User do not have access to this feature for the specified tenant");
                }
                // --- End of authentication check ---
                Tenant? tenant = await _dbTenantService.GetAsync(id);
                if (tenant == null)
                {
                    return NotFound("Tenant not found.");
                }
                if (await _dbTenantService.DeleteAsync(id))
                {
                    return Ok($"Tenant deleted successfully.");

                }
                return NotFound("Tenant not found.");
            }
            return BadRequest("Invalid request.");
        }

        [HttpGet("{id}")]
        [HasPermission("Tenant.Get.Permission", PermissionType.Tenant)]
        public async Task<IActionResult> GetAsync(int id)
        {
            if (ModelState.IsValid)
            {
                // Authentication check using roles + permissions
                if (!await _authService.UserHasPermissionAsync<TenantController>(Request.Headers["Authorization"].ToString(), id))
                {
                    return new CustomForbidResult("User do not have access to this feature for the specified tenant");
                }
                // --- End of authentication check ---
                Tenant? tenant = await _dbTenantService.GetAsync(id);
                if (tenant != null)
                {
                    return Ok(tenant);
                }
                return NotFound("Tenant not found.");
            }
            return BadRequest("Invalid request.");
        }

        [HttpGet("{id}/Rich")]
        [HasPermission("Tenant.Get-Rich.Permission", PermissionType.Tenant)]
        public async Task<IActionResult> GetRichAsync(int id)
        {
            if (ModelState.IsValid)
            {
                // Authentication check using roles + permissions
                if (!await _authService.UserHasPermissionAsync<TenantController>(Request.Headers["Authorization"].ToString(), id))
                {
                    return new CustomForbidResult("User do not have access to this feature for the specified tenant");
                }
                // --- End of authentication check ---
                Tenant? tenant = await _dbTenantService.GetRichAsync(id);
                if (tenant != null)
                {
                    return Ok(tenant);
                }
                return NotFound("Tenant not found.");
            }
            return BadRequest("Invalid request.");
        }

        [HttpGet("")]
        [HasPermission("Tenant.Get-All.Permission", PermissionType.Tenant)]
        public async Task<IActionResult> GetAllAsync()
        {
            if (ModelState.IsValid)
            {
                ICollection<Tenant>? tenants = await _dbTenantService.GetAllAsync();
                if (tenants != null)
                {
                    if (tenants.Any())
                    {
                        return Ok(tenants);
                    }
                }
                return NotFound("No tenants found.");
            }
            return BadRequest("Invalid request.");
        }
    }
}
