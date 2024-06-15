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
        public async Task<IActionResult> CreateTenant([FromBody] Tenant tenant)
        {
            if (await _dbTenantService.CreateAsync(tenant))
            {
                return Ok($"Tenant created successfully.");
            }
            return BadRequest(new { message = "An error occurred while creating the tenant." });
        }

        [HttpPut("Update")]
        public async Task<IActionResult> UpdateTenant([FromBody] Tenant tenant)
        {
            if (await _dbTenantService.UpdateAsync(tenant))
            {
                return Ok($"Tenant updated successfully.");
            }
            return BadRequest(new { message = "An error occurred while updating the tenant." });
        }

        [HttpDelete("Delete/{id}")]
        public async Task<IActionResult> DeleteTenant(int id)
        {
            if(await _dbTenantService.DeleteAsync(id))
            {
                return Ok($"Tenant deleted successfully.");

            }
            return NotFound(new { message = "tenant not found." });
        }

        [HttpGet("Get/{id}")]
        public async Task<IActionResult> GetTenant(int id)
        {
            Tenant? tenant = await _dbTenantService.GetAsync(id);
            if (tenant != null)
            {
                return Ok(tenant);
            }
            return NotFound(new { message = "tenant not found." });
        }

        [HttpGet("Get")]
        public async Task<IActionResult> GetTenantsAsync()
        {
            ICollection<Tenant> tenants = await _dbTenantService.GetAllAsync();

            if (tenants.Any())
            {
                return Ok(tenants);
            }
            return NotFound(new { message = "No tenants found." });
        }
    }
}
