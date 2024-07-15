using API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SharedComponents.DbServices;
using SharedComponents.Entities;
using SharedComponents.WebEntities.Requests.TenantRequests;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "v1-Web")]
    public class TenantController : ControllerBase
    {
        private readonly DbTenantService _dbTenantService;

        public TenantController(DbTenantService dbTenantService)
        {
            _dbTenantService = dbTenantService;
        }

        [HttpPost("")]
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
        public async Task<IActionResult> UpdateAsync([FromBody] TenantUpdateRequest request)
        {
            if (ModelState.IsValid)
            {
                Tenant? tenant = await _dbTenantService.GetAsync(request.Id);
                if (tenant == null)
                {
                    return NotFound("Tenant not found.");
                }
                if(tenant.TenantInfo == null)
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
        public async Task<IActionResult> DeleteAsync(int id)
        {
            if (ModelState.IsValid)
            {
                if (await _dbTenantService.DeleteAsync(id))
                {
                    return Ok($"Tenant deleted successfully.");

                }
                return NotFound("Tenant not found.");
            }
            return BadRequest("Invalid request.");
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetAsync(int id)
        {
            if (ModelState.IsValid)
            {
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
        public async Task<IActionResult> GetRichAsync(int id)
        {
            if (ModelState.IsValid)
            {
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
