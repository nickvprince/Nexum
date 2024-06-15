using API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SharedComponents.DbServices;
using SharedComponents.Entities;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TenantController : ControllerBase
    {
        private readonly DbTenantService _dbTenantService;

        public TenantController(DbTenantService dbTenantService)
        {
            _dbTenantService = dbTenantService;
        }

        [HttpPost("Create")]
        public async Task<IActionResult> CreateAsync([FromBody] Tenant tenant)
        {
            Tenant newTenant = await _dbTenantService.CreateAsync(tenant);
            if (newTenant != null)
            {
                return Ok(newTenant);
            }
            return BadRequest("An error occurred while creating the tenant.");
        }

        [HttpPut("Update")]
        public async Task<IActionResult> UpdateAsync([FromBody] Tenant tenant)
        {
            Tenant updatedTenant = await _dbTenantService.UpdateAsync(tenant);
            if (updatedTenant != null)
            {
                return Ok(updatedTenant);
            }
            return BadRequest("An error occurred while updating the tenant.");
        }

        [HttpDelete("Delete/{id}")]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            if(await _dbTenantService.DeleteAsync(id))
            {
                return Ok($"Tenant deleted successfully.");

            }
            return NotFound("Tenant not found.");
        }

        [HttpGet("Get/{id}")]
        public async Task<IActionResult> GetAsync(int id)
        {
            Tenant? tenant = await _dbTenantService.GetAsync(id);
            if (tenant != null)
            {
                return Ok(tenant);
            }
            return NotFound("Tenant not found.");
        }

        [HttpGet("Get")]
        public async Task<IActionResult> GetAllAsync()
        {
            ICollection<Tenant> tenants = await _dbTenantService.GetAllAsync();
            if (tenants.Any())
            {
                return Ok(tenants);
            }
            return NotFound("No tenants found.");
        }
    }
}
