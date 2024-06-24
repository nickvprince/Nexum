using API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SharedComponents.DbServices;
using SharedComponents.Entities;

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
        public async Task<IActionResult> CreateAsync([FromBody] Tenant tenant)
        {
            Tenant? newTenant = await _dbTenantService.CreateAsync(tenant);
            if (newTenant != null)
            {
                return Ok(newTenant);
            }
            return BadRequest("An error occurred while creating the tenant.");
        }

        [HttpPut("")]
        public async Task<IActionResult> UpdateAsync([FromBody] Tenant tenant)
        {
            Tenant? updatedTenant = await _dbTenantService.UpdateAsync(tenant);
            if (updatedTenant != null)
            {
                return Ok(updatedTenant);
            }
            return BadRequest("An error occurred while updating the tenant.");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            if(await _dbTenantService.DeleteAsync(id))
            {
                return Ok($"Tenant deleted successfully.");

            }
            return NotFound("Tenant not found.");
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetAsync(int id)
        {
            Tenant? tenant = await _dbTenantService.GetAsync(id);
            if (tenant != null)
            {
                return Ok(tenant);
            }
            return NotFound("Tenant not found.");
        }

        [HttpGet("")]
        public async Task<IActionResult> GetAllAsync()
        {
            ICollection<Tenant> tenants = await _dbTenantService.GetAllAsync();
            if (tenants.Any())
            {
                return Ok(tenants);
            }
            return NotFound("No tenants found.");
        }

        [HttpPost("Create-Installation-Key/{tenantId}")]
        public async Task<IActionResult> CreateInstallationKeyAsync(int tenantId)
        {
            InstallationKey? newInstallationKey = await _dbTenantService.CreateInstallationKeyAsync(tenantId);
            if (newInstallationKey != null)
            {
                return Ok(newInstallationKey);
            }
            return BadRequest("An error occurred while creating the installation key.");
        }

        [HttpPut("Update-Installation-Key")]
        public async Task<IActionResult> UpdateInstallationKeyAsync([FromBody] InstallationKey installationKey)
        {
            InstallationKey? updatedInstallationKey = await _dbTenantService.UpdateInstallationKeyAsync(installationKey);
            if (updatedInstallationKey != null)
            {
                return Ok(updatedInstallationKey);
            }
            return BadRequest("An error occurred while updating the installation key.");
        }
        [HttpDelete("Delete-Installation-Key/{id}")]
        public async Task<IActionResult> DeleteInstallationKeyAsync(string? installationKey)
        {
            if (await _dbTenantService.DeleteInstallationKeyAsync(installationKey))
            {
                return Ok($"Installation key deleted successfully.");

            }
            return NotFound("Installation key not found.");
        }
    }
}
