using Microsoft.AspNetCore.Mvc;
using SharedComponents.Entities.DbEntities;
using SharedComponents.Entities.WebEntities.Requests.BackupRequests;
using SharedComponents.Handlers.Attributes.HasPermission;
using SharedComponents.Handlers.Results;
using SharedComponents.Services.APIServices.Interfaces;
using SharedComponents.Services.DbServices.Interfaces;

namespace API.Controllers
{
    /// <summary>
    /// Controller for handling backup-related operations.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "v1-Web")]
    public class BackupController : ControllerBase
    {
        private readonly IDbBackupService _dbBackupService;
        private readonly IDbDeviceService _dbDeviceService;
        private readonly IDbNASServerService _dbNASServerService;
        private readonly IAPIAuthService _authService;

        /// <summary>
        /// Initializes a new instance of the <see cref="BackupController"/> class.
        /// </summary>
        /// <param name="dbBackupService">The backup service.</param>
        /// <param name="dbDeviceService">The device service.</param>
        /// <param name="dbNASServerService">The NAS server service.</param>
        /// <param name="authService">The authentication service.</param>
        public BackupController(IDbBackupService dbBackupService, IDbDeviceService dbDeviceService,
            IDbNASServerService dbNASServerService, IAPIAuthService authService)
        {
            _dbBackupService = dbBackupService;
            _dbDeviceService = dbDeviceService;
            _dbNASServerService = dbNASServerService;
            _authService = authService;
        }

        /// <summary>
        /// Creates a new backup.
        /// </summary>
        /// <param name="request">The backup create request.</param>
        /// <returns>An action result containing the created backup.</returns>
        [HttpPost("")]
        [HasPermission("Backup.Create.Permission", PermissionType.Tenant)]
        public async Task<IActionResult> CreateAsync([FromBody] BackupCreateRequest request)
        {
            if (ModelState.IsValid)
            {
                Device? device = await _dbDeviceService.GetAsync(request.DeviceId);
                if (device == null)
                {
                    return NotFound("Device not found.");
                }
                // Authentication check using roles + permissions
                if (!await _authService.UserHasPermissionAsync<BackupController>(Request.Headers["Authorization"].ToString(), device.TenantId))
                {
                    return new CustomForbidResult("User do not have access to this feature for the specified tenant");
                }
                // --- End of authentication check ---
                NASServer? nasServer = await _dbNASServerService.GetAsync(request.NASServerId);
                if (nasServer == null)
                {
                    return NotFound("NAS Server not found.");
                }
                if(nasServer.TenantId != device.TenantId)
                {
                    return BadRequest("NAS Server does not belong to the specified device's tenant.");
                }
                DeviceBackup? backup = new DeviceBackup
                {
                    Client_Id = device.DeviceInfo.ClientId,
                    Uuid = device.DeviceInfo.Uuid,
                    TenantId = device.TenantId,
                    Filename = request.Name,
                    Path = request.Path,
                    Date = request.Date,
                    NASServerId = request.NASServerId
                };
                backup = await _dbBackupService.CreateAsync(backup);
                if (backup != null)
                {
                    return Ok(backup);
                }
                return BadRequest("An error occurred while creating the backup.");
            }
            return BadRequest("Invalid Request.");
        }

        /// <summary>
        /// Updates an existing backup.
        /// </summary>
        /// <param name="request">The backup update request.</param>
        /// <returns>An action result containing the updated backup.</returns>
        [HttpPut("")]
        [HasPermission("Backup.Update.Permission", PermissionType.Tenant)]
        public async Task<IActionResult> UpdateAsync([FromBody] BackupUpdateRequest request)
        {
            if (ModelState.IsValid)
            {
                DeviceBackup? backup = await _dbBackupService.GetAsync(request.Id);
                if (backup == null)
                {
                    return NotFound("Backup not found.");
                }
                // Authentication check using roles + permissions
                if (!await _authService.UserHasPermissionAsync<BackupController>(Request.Headers["Authorization"].ToString(), backup.TenantId))
                {
                    return new CustomForbidResult("User do not have access to this feature for the specified tenant");
                }
                // --- End of authentication check ---
                backup.Filename = request.Name;
                backup.Path = request.Path;
                backup.Date = request.Date;
                backup = await _dbBackupService.UpdateAsync(backup);
                if (backup != null)
                {
                    return Ok(backup);
                }
                return BadRequest("An error occurred while updating the backup.");
            }
            return BadRequest("Invalid Request.");
        }

        /// <summary>
        /// Deletes a backup by ID.
        /// </summary>
        /// <param name="id">The ID of the backup to delete.</param>
        /// <returns>An action result indicating the outcome of the deletion.</returns>
        [HttpDelete("{id}")]
        [HasPermission("Backup.Delete.Permission", PermissionType.Tenant)]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            if (ModelState.IsValid)
            {
                DeviceBackup? backup = await _dbBackupService.GetAsync(id);
                if (backup == null)
                {
                    return NotFound("Backup not found.");
                }
                // Authentication check using roles + permissions
                if (!await _authService.UserHasPermissionAsync<BackupController>(Request.Headers["Authorization"].ToString(), backup.TenantId))
                {
                    return new CustomForbidResult("User do not have access to this feature for the specified tenant");
                }
                // --- End of authentication check ---
                if (await _dbBackupService.DeleteAsync(id))
                {
                    return Ok("Backup deleted successfully.");
                }
                return BadRequest("An error occurred while deleting the backup.");
            }
            return BadRequest("Invalid Request.");
        }

        /// <summary>
        /// Gets a backup by ID.
        /// </summary>
        /// <param name="id">The ID of the backup to retrieve.</param>
        /// <returns>An action result containing the backup.</returns>
        [HttpGet("{id}")]
        [HasPermission("Backup.Get.Permission", PermissionType.Tenant)]
        public async Task<IActionResult> GetAsync(int id)
        {
            if (ModelState.IsValid)
            {
                DeviceBackup? backup = await _dbBackupService.GetAsync(id);
                if (backup != null)
                {
                    // Authentication check using roles + permissions
                    if (!await _authService.UserHasPermissionAsync<BackupController>(Request.Headers["Authorization"].ToString(), backup.TenantId))
                    {
                        return new CustomForbidResult("User do not have access to this feature for the specified tenant");
                    }
                    // --- End of authentication check ---
                    return Ok(backup);
                }
                return NotFound("Backup not found.");
            }
            return BadRequest("Invalid Request.");
        }

        /// <summary>
        /// Gets all backups accessible by the authenticated user.
        /// </summary>
        /// <returns>An action result containing all backups accessible by the user.</returns>
        [HttpGet("")]
        [HasPermission("Backup.Get.Permission", PermissionType.Tenant)]
        public async Task<IActionResult> GetAllAsync()
        {
            if (ModelState.IsValid)
            {
                var tenantIds = await _authService.GetUserAccessibleTenantsAsync(Request.Headers["Authorization"].ToString());
                if (tenantIds == null)
                {
                    return new CustomForbidResult("User does not have any tenant permissions");
                }
                List<DeviceBackup>? backups = new List<DeviceBackup>();
                foreach (var tenantId in tenantIds)
                {
                    if (tenantId != null)
                    {
                        var tenantBackups = await _dbBackupService.GetAllByTenantIdAsync((int)tenantId);
                        if (tenantBackups != null)
                        {
                            backups.AddRange(tenantBackups);
                        }
                    }
                }
                if (backups != null)
                {
                    if (backups.Any())
                    {
                        return Ok(backups.Distinct());
                    }
                }
                return NotFound("No backups found.");
            }
            return BadRequest("Invalid Request.");
        }

        /// <summary>
        /// Gets all backups for a specific device.
        /// </summary>
        /// <param name="deviceId">The ID of the device.</param>
        /// <returns>An action result containing the backups for the device.</returns>
        [HttpGet("By-Device/{deviceId}")]
        [HasPermission("Backup.Get.Permission", PermissionType.Tenant)]
        public async Task<IActionResult> GetAllByDeviceIdAsync(int deviceId)
        {
            if (ModelState.IsValid)
            {
                Device? device = await _dbDeviceService.GetAsync(deviceId);
                if (device == null)
                {
                    return NotFound("Device not found.");
                }
                // Authentication check using roles + permissions
                if (!await _authService.UserHasPermissionAsync<BackupController>(Request.Headers["Authorization"].ToString(), device.TenantId))
                {
                    return new CustomForbidResult("User do not have access to this feature for the specified tenant");
                }
                // --- End of authentication check ---
                ICollection<DeviceBackup>? backups = await _dbBackupService.GetAllByClientIdAndUuidAsync(device.TenantId, device.DeviceInfo.ClientId, device.DeviceInfo.Uuid);
                if (backups != null)
                {
                    if (backups.Any())
                    {
                        return Ok(backups);
                    }
                }
                return NotFound("No backups found for the specified device.");
            }
            return BadRequest("Invalid Request.");
        }

        /// <summary>
        /// Gets all backups for a specific tenant.
        /// </summary>
        /// <param name="tenantId">The ID of the tenant.</param>
        /// <returns>An action result containing the backups for the tenant.</returns>
        [HttpGet("By-Tenant/{tenantId}")]
        [HasPermission("Backup.Get.Permission", PermissionType.Tenant)]
        public async Task<IActionResult> GetAllByTenantIdAsync(int tenantId)
        {
            if (ModelState.IsValid)
            {
                // Authentication check using roles + permissions
                if (!await _authService.UserHasPermissionAsync<BackupController>(Request.Headers["Authorization"].ToString(), tenantId))
                {
                    return new CustomForbidResult("User do not have access to this feature for the specified tenant");
                }
                // --- End of authentication check ---
                ICollection<DeviceBackup>? backups = await _dbBackupService.GetAllByTenantIdAsync(tenantId);
                if (backups != null)
                {
                    if (backups.Any())
                    {
                        return Ok(backups);
                    }
                }
                return NotFound("No backups found for the specified tenant.");
            }
            return BadRequest("Invalid Request.");
        }
    }
}
