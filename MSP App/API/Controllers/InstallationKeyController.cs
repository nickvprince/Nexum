using API.Attributes.HasPermission;
﻿using API.Services;
using API.Services.Interfaces;
using Azure.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SharedComponents.DbServices;
using SharedComponents.Entities;
using SharedComponents.Results;
using SharedComponents.WebEntities.Requests.InstallationKeyRequests;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "v1-Web")]
    public class InstallationKeyController : ControllerBase
    {
        private readonly IDbInstallationKeyService _dbInstallationKeyService;
        private readonly IDbTenantService _dbTenantService;
        private readonly IAuthService _authService;

        public InstallationKeyController(IDbInstallationKeyService dbInstallationKeyService, IDbTenantService dbTenantService, IAuthService authService)
        {
            _dbInstallationKeyService = dbInstallationKeyService;
            _dbTenantService = dbTenantService;
            _authService = authService;
        }

        [HttpPost("")]
        [HasPermission("InstallationKey.Create.Permission", PermissionType.Tenant)]
        public async Task<IActionResult> CreateAsync([FromBody] InstallationKeyCreateRequest request)
        {
            if( ModelState.IsValid)
            {
                Tenant? tenant = await _dbTenantService.GetAsync(request.TenantId);
                if (tenant == null)
                {
                    return NotFound("Tenant not found.");
                }
                // Authentication check using roles + permissions
                if (!await _authService.UserHasPermissionAsync<InstallationKeyController>(Request.Headers["Authorization"].ToString(), tenant.Id))
                {
                    return new CustomForbidResult("User do not have access to this feature for the specified tenant");
                }
                // --- End of authentication check ---
                if (request.Type == null)
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
        [HasPermission("InstallationKey.Update.Permission", PermissionType.Tenant)]
        public async Task<IActionResult> UpdateAsync([FromBody] InstallationKeyUpdateRequest request)
        {
            if (ModelState.IsValid)
            {
                InstallationKey? installationKey = await _dbInstallationKeyService.GetAsync(request.Id);
                if (installationKey == null)
                {
                    return NotFound("Installation key not found.");
                }
                // Authentication check using roles + permissions
                if (!await _authService.UserHasPermissionAsync<InstallationKeyController>(Request.Headers["Authorization"].ToString(), installationKey.TenantId))
                {
                    return new CustomForbidResult("User do not have access to this feature for the specified tenant");
                }
                // --- End of authentication check ---
                
                if (request.Type == null)
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
                if (installationKey.IsDeleted)
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
        [HasPermission("InstallationKey.Delete.Permission", PermissionType.Tenant)]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            if (ModelState.IsValid)
            {
                InstallationKey? installationKey = await _dbInstallationKeyService.GetAsync(id);
                if (installationKey == null)
                {
                    return NotFound("Installation key not found.");
                }
                // Authentication check using roles + permissions
                if (!await _authService.UserHasPermissionAsync<InstallationKeyController>(Request.Headers["Authorization"].ToString(), installationKey.TenantId))
                {
                    return new CustomForbidResult("User do not have access to this feature for the specified tenant");
                }
                // --- End of authentication check ---
                if (await _dbInstallationKeyService.DeleteAsync(id))
                {
                    return Ok($"Installation key deleted successfully.");
                }
                return NotFound("Installation key not found.");
            }
            return BadRequest("Invalid Request.");
        }

        [HttpGet("{id}")]
        [HasPermission("InstallationKey.Get.Permission", PermissionType.Tenant)]
        public async Task<IActionResult> GetAsync(int id)
        {
            if (ModelState.IsValid)
            {
                InstallationKey? installationKey = await _dbInstallationKeyService.GetAsync(id);
                if (installationKey != null)
                {
                    // Authentication check using roles + permissions
                    if (!await _authService.UserHasPermissionAsync<InstallationKeyController>(Request.Headers["Authorization"].ToString(), installationKey.TenantId))
                    {
                        return new CustomForbidResult("User do not have access to this feature for the specified tenant");
                    }
                    // --- End of authentication check ---
                    return Ok(installationKey);
                }
                return NotFound("Installation key not found.");
            }
            return BadRequest("Invalid Request.");
        }

        [HttpGet("")]
        [HasPermission("InstallationKey.Get-All.Permission", PermissionType.Tenant)]
        public async Task<IActionResult> GetAllAsync()
        {
            if (ModelState.IsValid)
            {
                var tenantIds = await _authService.GetUserAccessibleTenantsAsync(Request.Headers["Authorization"].ToString());
                if (tenantIds == null)
                {
                    return new CustomForbidResult("User does not have any tenant permissions");
                }
                List<InstallationKey> installationKeys = new List<InstallationKey>();
                foreach (var tenantId in tenantIds)
                {
                    if (tenantId != null)
                    {
                        var tenantInstallationKeys = await _dbInstallationKeyService.GetAllByTenantIdAsync((int)tenantId);
                        if (tenantInstallationKeys != null)
                        {
                            installationKeys.AddRange(tenantInstallationKeys);
                        }
                    }
                }
                if (installationKeys != null)
                {
                    if (installationKeys.Any())
                    {
                        return Ok(installationKeys.Distinct());
                    }
                }
                return NotFound("Installation keys not found.");
            }
            return BadRequest("Invalid Request.");
        }

        [HttpGet("By-Tenant/{tenantId}")]
        [HasPermission("InstallationKey.Get-By-Tenant.Permission", PermissionType.Tenant)]
        public async Task<IActionResult> GetAllByTenantIdAsync(int tenantId)
        {
            if (ModelState.IsValid)
            {
                // Authentication check using roles + permissions
                if (!await _authService.UserHasPermissionAsync<InstallationKeyController>(Request.Headers["Authorization"].ToString(), tenantId))
                {
                    return new CustomForbidResult("User do not have access to this feature for the specified tenant");
                }
                // --- End of authentication check ---
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
