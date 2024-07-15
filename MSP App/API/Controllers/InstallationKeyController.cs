using API.Services;
using Azure.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SharedComponents.Entities;
using SharedComponents.WebEntities.Requests.InstallationKeyRequests;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "v1-Web")]
    public class InstallationKeyController : ControllerBase
    {
        private readonly DbInstallationKeyService _dbInstallationKeyService;
        private readonly DbTenantService _dbTenantService;

        public InstallationKeyController(DbInstallationKeyService dbInstallationKeyService, DbTenantService dbTenantService)
        {
            _dbInstallationKeyService = dbInstallationKeyService;
            _dbTenantService = dbTenantService;
        }

        [HttpPost("")]
        public async Task<IActionResult> CreateAsync([FromBody] InstallationKeyCreateRequest request)
        {
            if( ModelState.IsValid)
            {
                Tenant? tenant = await _dbTenantService.GetAsync(request.TenantId);
                if (tenant == null)
                {
                    return NotFound("Tenant not found.");
                }
                if(request.Type == null)
                {
                    return BadRequest("InstallationKey Type is required.");
                }
                if (request.Type.HasValue)
                {
                    if (!Enum.IsDefined(typeof(InstallationKeyType), request.Type.Value))
                    {
                        return BadRequest("Invalid InstallationKey Type.");
                    }
                }
                InstallationKey? installationKey = new InstallationKey
                {
                    Key = Guid.NewGuid().ToString(),
                    TenantId = request.TenantId,
                    Type = request.Type.Value,
                    IsActive = true,
                    IsDeleted = false
                };
                installationKey = await _dbInstallationKeyService.CreateAsync(installationKey);
                if (installationKey != null)
                {
                    return Ok(installationKey);
                }
                return BadRequest("An error occurred while creating the installation key.");
            }
            return BadRequest("Invalid Request.");
        }

        [HttpPut("")]
        public async Task<IActionResult> UpdateAsync([FromBody] InstallationKeyUpdateRequest request)
        {
            if (ModelState.IsValid)
            {
                InstallationKey? installationKey = await _dbInstallationKeyService.GetAsync(request.Id);
                if (installationKey == null)
                {
                    return NotFound("Installation key not found.");
                }
                if(installationKey.IsDeleted)
                {
                    return BadRequest("Installation key is deleted.");
                }
                installationKey.IsActive = request.IsActive;
                installationKey = await _dbInstallationKeyService.UpdateAsync(installationKey);
                if (installationKey != null)
                {
                    return Ok(installationKey);
                }
                return BadRequest("An error occurred while updating the installation key.");
            }
            return BadRequest("Invalid Request.");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            if (ModelState.IsValid)
            {
                if (await _dbInstallationKeyService.DeleteAsync(id))
                {
                    return Ok($"Installation key deleted successfully.");
                }
                return NotFound("Installation key not found.");
            }
            return BadRequest("Invalid Request.");
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetAsync(int id)
        {
            if (ModelState.IsValid)
            {
                InstallationKey? installationKey = await _dbInstallationKeyService.GetAsync(id);
                if (installationKey != null)
                {
                    return Ok(installationKey);
                }
                return NotFound("Installation key not found.");
            }
            return BadRequest("Invalid Request.");
        }

        [HttpGet("")]
        public async Task<IActionResult> GetAllAsync()
        {
            if (ModelState.IsValid)
            {
                ICollection<InstallationKey>? installationKeys = await _dbInstallationKeyService.GetAllAsync();
                if (installationKeys != null)
                {
                    if (installationKeys.Any())
                    {
                        return Ok(installationKeys);
                    }
                }
                return NotFound("Installation keys not found.");
            }
            return BadRequest("Invalid Request.");
        }

        [HttpGet("By-Tenant/{tenantId}")]
        public async Task<IActionResult> GetAllByTenantIdAsync(int tenantId)
        {
            if (ModelState.IsValid)
            {
                Tenant? tenant = await _dbTenantService.GetAsync(tenantId);
                if (tenant == null)
                {
                    return NotFound("Tenant not found.");
                }
                ICollection<InstallationKey>? installationKeys = await _dbInstallationKeyService.GetAllByTenantIdAsync(tenantId);
                if (installationKeys != null)
                {
                    if (installationKeys.Any())
                    {
                        return Ok(installationKeys);
                    }
                }
                return NotFound("No installation keys found for the specified tenant.");
            }
            return BadRequest("Invalid Request.");
        }
    }
}
