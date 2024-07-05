﻿using API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SharedComponents.Entities;
using SharedComponents.WebRequestEntities.BackupRequests;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "v1-Web")]
    public class BackupController : ControllerBase
    {
        private readonly DbBackupService _dbBackupService;
        private readonly DbDeviceService _dbDeviceService;

        public BackupController(DbBackupService dbBackupService, DbDeviceService dbDeviceService)
        {
            _dbBackupService = dbBackupService;
            _dbDeviceService = dbDeviceService;
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
                DeviceBackup? backup = new DeviceBackup
                {
                    DeviceId = request.DeviceId,
                    Name = request.Name,
                    Path = request.Path,
                    Date = request.Date
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
                backup.Name = request.Name;
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
                ICollection<DeviceBackup>? backups = await _dbBackupService.GetAllByDeviceIdAsync(deviceId);
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
