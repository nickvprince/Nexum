using API.Services;
using Azure.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using SharedComponents.DbServices;
using SharedComponents.Entities;
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

        public BackupController(IDbBackupService dbBackupService, IDbDeviceService dbDeviceService, IDbNASServerService dbNASServerService)
        {
            _dbBackupService = dbBackupService;
            _dbDeviceService = dbDeviceService;
            _dbNASServerService = dbNASServerService;
        }

        [HttpPost("")]
        public async Task<IActionResult> CreateAsync([FromBody] BackupCreateRequest request)
        {
            if (ModelState.IsValid)
            {
                Device? device = await _dbDeviceService.GetAsync(request.DeviceId);
                if (device == null)
                {
                    return NotFound("Device not found.");
                }
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
        public async Task<IActionResult> UpdateAsync([FromBody] BackupUpdateRequest request)
        {
            if (ModelState.IsValid)
            {
                DeviceBackup? backup = await _dbBackupService.GetAsync(request.Id);
                if (backup == null)
                {
                    return NotFound("Backup not found.");
                }
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
        public async Task<IActionResult> DeleteAsync(int id)
        {
            if (ModelState.IsValid)
            {
                DeviceBackup? backup = await _dbBackupService.GetAsync(id);
                if (backup == null)
                {
                    return NotFound("Backup not found.");
                }
                if (await _dbBackupService.DeleteAsync(id))
                {
                    return Ok("Backup deleted successfully.");
                }
                return BadRequest("An error occurred while deleting the backup.");
            }
            return BadRequest("Invalid Request.");
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetAsync(int id)
        {
            if (ModelState.IsValid)
            {
                DeviceBackup? backup = await _dbBackupService.GetAsync(id);
                if (backup != null)
                {
                    return Ok(backup);
                }
                return NotFound("Backup not found.");
            }
            return BadRequest("Invalid Request.");
        }

        [HttpGet("")]
        public async Task<IActionResult> GetAllAsync()
        {
            if (ModelState.IsValid)
            {
                ICollection<DeviceBackup>? backups = await _dbBackupService.GetAllAsync();
                if (backups != null)
                {
                    if (backups.Any())
                    {
                        return Ok(backups);
                    }
                }
                return NotFound("No backups found.");
            }
            return BadRequest("Invalid Request.");
        }

        [HttpGet("By-Device/{deviceId}")]
        public async Task<IActionResult> GetAllByDeviceIdAsync(int deviceId)
        {
            if (ModelState.IsValid)
            {
                Device? device = await _dbDeviceService.GetAsync(deviceId);
                if (device == null)
                {
                    return NotFound("Device not found.");
                }
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
        public async Task<IActionResult> GetAllByTenantIdAsync(int tenantId)
        {
            if (ModelState.IsValid)
            {
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
