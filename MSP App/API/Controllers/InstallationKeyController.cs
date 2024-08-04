using Microsoft.AspNetCore.Mvc;
using SharedComponents.Entities.DbEntities;
using SharedComponents.Entities.WebEntities.Requests.InstallationKeyRequests;
using SharedComponents.Handlers.Attributes.HasPermission;
using SharedComponents.Handlers.Results;
using SharedComponents.Services.APIServices.Interfaces;
using SharedComponents.Services.DbServices.Interfaces;

namespace API.Controllers
{
    /// <summary>
    /// Controller for handling installation key-related operations.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "v1-Web")]
    public class InstallationKeyController : ControllerBase
    {
        private readonly IDbInstallationKeyService _dbInstallationKeyService;
        private readonly IDbTenantService _dbTenantService;
        private readonly IAPIAuthService _authService;

        /// <summary>
        /// Initializes a new instance of the <see cref="InstallationKeyController"/> class.
        /// </summary>
        /// <param name="dbInstallationKeyService">The installation key service.</param>
        /// <param name="dbTenantService">The tenant service.</param>
        /// <param name="authService">The authentication service.</param>
        public InstallationKeyController(IDbInstallationKeyService dbInstallationKeyService, IDbTenantService dbTenantService, IAPIAuthService authService)
        {
            _dbInstallationKeyService = dbInstallationKeyService;
            _dbTenantService = dbTenantService;
            _authService = authService;
        }

        /// <summary>
        /// Creates a new installation key.
        /// </summary>
        /// <param name="request">The installation key create request.</param>
        /// <returns>An action result containing the created installation key.</returns>
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

        /// <summary>
        /// Updates an existing installation key.
        /// </summary>
        /// <param name="request">The installation key update request.</param>
        /// <returns>An action result containing the updated installation key.</returns>
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
                installationKey.Type = request.Type.Value;
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

        /// <summary>
        /// Deletes an installation key by ID.
        /// </summary>
        /// <param name="id">The ID of the installation key to delete.</param>
        /// <returns>An action result indicating the outcome of the deletion.</returns>
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

        /// <summary>
        /// Gets an installation key by ID.
        /// </summary>
        /// <param name="id">The ID of the installation key to retrieve.</param>
        /// <returns>An action result containing the installation key.</returns>
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

        /// <summary>
        /// Gets all installation keys accessible by the authenticated user.
        /// </summary>
        /// <returns>An action result containing all installation keys accessible by the user.</returns>
        [HttpGet("")]
        [HasPermission("InstallationKey.Get.Permission", PermissionType.Tenant)]
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

        /// <summary>
        /// Gets all installation keys for a specific tenant.
        /// </summary>
        /// <param name="tenantId">The ID of the tenant.</param>
        /// <returns>An action result containing the installation keys for the tenant.</returns>
        [HttpGet("By-Tenant/{tenantId}")]
        [HasPermission("InstallationKey.Get.Permission", PermissionType.Tenant)]
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
