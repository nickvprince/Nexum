using API.Attributes.HasPermission;
using API.Services;
using API.Services.Interfaces;
using Azure.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using SharedComponents.DbServices;
using SharedComponents.Entities;
using SharedComponents.Results;
using SharedComponents.WebEntities.Requests.BackupRequests;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "v1-Web")]
    public class BackupController : ControllerBase
    {
        private readonly IDbBackupService _dbBackupService;
        private readonly IDbDeviceService _dbDeviceService;
        private readonly IDbNASServerService _dbNASServerService;
        private readonly IAuthService _authService;

        public BackupController(IDbBackupService dbBackupService, IDbDeviceService dbDeviceService,
            IDbNASServerService dbNASServerService, IAuthService authService)
        {
            _dbBackupService = dbBackupService;
            _dbDeviceService = dbDeviceService;
            _dbNASServerService = dbNASServerService;
            _authService = authService;
        }

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

        [HttpGet("")]
        [HasPermission("Backup.Get-All.Permission", PermissionType.Tenant)]
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

        [HttpGet("By-Device/{deviceId}")]
        [HasPermission("Backup.Get-By-Device.Permission", PermissionType.Tenant)]
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

        [HttpGet("By-Tenant/{tenantId}")]
        [HasPermission("Backup.Get-By-Tenant.Permission", PermissionType.Tenant)]
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
