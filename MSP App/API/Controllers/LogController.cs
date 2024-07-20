using Microsoft.AspNetCore.Mvc;
using SharedComponents.Entities.DbEntities;
using SharedComponents.Entities.WebEntities.Requests.LogRequests;
using SharedComponents.Handlers.Attributes.HasPermission;
using SharedComponents.Handlers.Results;
using SharedComponents.Services.APIServices.Interfaces;
using SharedComponents.Services.DbServices.Interfaces;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "v1-Web")]
    public class LogController : ControllerBase
    {
        private readonly IDbLogService _dbLogService;
        private readonly IDbDeviceService _dbDeviceService;
        private readonly IAPIAuthService _authService;

        public LogController(IDbLogService dbLogService, IDbDeviceService dbDeviceService, IAPIAuthService authService)
        {
            _dbLogService = dbLogService;
            _dbDeviceService = dbDeviceService;
            _authService = authService;
        }

        [HttpPost("")]
        [HasPermission("Log.Create.Permission", PermissionType.Tenant)]
        public async Task<IActionResult> CreateAsync([FromBody] LogCreateRequest request)
        {
            if (ModelState.IsValid)
            {
                Device? device = await _dbDeviceService.GetAsync(request.DeviceId);
                if (device == null)
                {
                    return NotFound("Device not found.");
                }
                // Authentication check using roles + permissions
                if (!await _authService.UserHasPermissionAsync<LogController>(Request.Headers["Authorization"].ToString(), device.TenantId))
                {
                    return new CustomForbidResult("User do not have access to this feature for the specified tenant");
                }
                // --- End of authentication check ---
                DeviceLog? log = new DeviceLog
                {
                    DeviceId = request.DeviceId,
                    Filename = request.Filename,
                    Function = request.Function,
                    Message = request.Message,
                    Time = request.Time,
                    Acknowledged = false,
                    IsDeleted = false
                };
                log = await _dbLogService.CreateAsync(log);
                if (log != null)
                {
                    return Ok(log);
                }
                return BadRequest("An error occurred while creating the log.");
            }
            return BadRequest("Invalid Request.");
        }

        [HttpPut("")]
        [HasPermission("Log.Update.Permission", PermissionType.Tenant)]
        public async Task<IActionResult> UpdateAsync([FromBody] LogUpdateRequest request)
        {
            if (ModelState.IsValid)
            {
                DeviceLog? log = await _dbLogService.GetAsync(request.Id);
                if (log == null)
                {
                    return NotFound("Log not found.");
                }
                Device? device = await _dbDeviceService.GetAsync(log.DeviceId);
                if (device == null)
                {
                    return NotFound("Device not found.");
                }
                // Authentication check using roles + permissions
                if (!await _authService.UserHasPermissionAsync<LogController>(Request.Headers["Authorization"].ToString(), device.TenantId))
                {
                    return new CustomForbidResult("User do not have access to this feature for the specified tenant");
                }
                // --- End of authentication check ---
                if (log.IsDeleted)
                {
                    return BadRequest("Log is deleted.");
                }
                log.Filename = request.Filename;
                log.Function = request.Function;
                log.Message = request.Message;
                log.Code = request.Code;
                log.Stack_Trace = request.Stack_Trace;
                log.Type = request.Type;
                log = await _dbLogService.UpdateAsync(log);
                if (log != null)
                {
                    return Ok(log);
                }
                return BadRequest("An error occurred while updating the log.");
            }
            return BadRequest("Invalid Request.");
        }

        [HttpPost("{id}/Acknowledge")]
        [HasPermission("Log.Acknowledge.Permission", PermissionType.Tenant)]
        public async Task<IActionResult> AcknowledgeAsync(int id)
        {
            if (ModelState.IsValid)
            {
                DeviceLog? log = await _dbLogService.GetAsync(id);
                if (log == null)
                {
                    return NotFound("Log not found.");
                }
                Device? device = await _dbDeviceService.GetAsync(log.DeviceId);
                if (device == null)
                {
                    return NotFound("Device not found.");
                }
                // Authentication check using roles + permissions
                if (!await _authService.UserHasPermissionAsync<LogController>(Request.Headers["Authorization"].ToString(), device.TenantId))
                {
                    return new CustomForbidResult("User do not have access to this feature for the specified tenant");
                }
                // --- End of authentication check ---
                if (log.IsDeleted)
                {
                    return BadRequest("Log is deleted.");
                }
                if (log.Acknowledged)
                {
                    return BadRequest("Log is already acknowledged.");
                }
                log.Acknowledged = true;
                log = await _dbLogService.UpdateAsync(log);
                if (log != null)
                {
                    return Ok("Log acknowledged successfully.");
                }
                return BadRequest("An error occurred while acknowledging the log.");
            }
            return BadRequest("Invalid Request.");
        }

        [HttpDelete("{id}")]
        [HasPermission("Log.Delete.Permission", PermissionType.Tenant)]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            if (ModelState.IsValid)
            {
                DeviceLog? log = await _dbLogService.GetAsync(id);
                if (log == null)
                {
                    return NotFound("Log not found.");
                }
                Device? device = await _dbDeviceService.GetAsync(log.DeviceId);
                if (device == null)
                {
                    return NotFound("Device not found.");
                }
                // Authentication check using roles + permissions
                if (!await _authService.UserHasPermissionAsync<LogController>(Request.Headers["Authorization"].ToString(), device.TenantId))
                {
                    return new CustomForbidResult("User do not have access to this feature for the specified tenant");
                }
                // --- End of authentication check ---
                if (await _dbLogService.DeleteAsync(id))
                {
                    return Ok($"Log deleted successfully.");
                }
                return NotFound("Log not found.");
            }
            return BadRequest("Invalid Request.");
        }

        [HttpGet("{id}")]
        [HasPermission("Log.Get.Permission", PermissionType.Tenant)]
        public async Task<IActionResult> GetAsync(int id)
        {
            if (ModelState.IsValid)
            {
                DeviceLog? log = await _dbLogService.GetAsync(id);
                if (log != null)
                {
                    Device? device = await _dbDeviceService.GetAsync(log.DeviceId);
                    if (device == null)
                    {
                        return NotFound("Device not found.");
                    }
                    // Authentication check using roles + permissions
                    if (!await _authService.UserHasPermissionAsync<LogController>(Request.Headers["Authorization"].ToString(), device.TenantId))
                    {
                        return new CustomForbidResult("User do not have access to this feature for the specified tenant");
                    }
                    // --- End of authentication check ---
                    return Ok(log);
                }
                return NotFound("Log not found.");
            }
            return BadRequest("Invalid Request.");
        }

        [HttpGet("")]
        [HasPermission("Log.Get-All.Permission", PermissionType.Tenant)]
        public async Task<IActionResult> GetAllAsync()
        {
            if (ModelState.IsValid)
            {
                var tenantIds = await _authService.GetUserAccessibleTenantsAsync(Request.Headers["Authorization"].ToString());
                if (tenantIds == null)
                {
                    return new CustomForbidResult("User does not have any tenant permissions");
                }
                List<DeviceLog> logs = new List<DeviceLog>();
                foreach (var tenantId in tenantIds)
                {
                    if (tenantId != null)
                    {
                        var tenantLogs = await _dbLogService.GetAllByTenantIdAsync((int)tenantId);
                        if(tenantLogs != null)
                        {
                            logs.AddRange(tenantLogs);
                        }
                    }
                }
                if (logs != null)
                {
                    if(logs.Any())
                    {
                        return Ok(logs);
                    }
                }
                return NotFound("Logs not found.");
            }
            return BadRequest("Invalid Request.");
        }

        [HttpGet("By-Device/{deviceId}")]
        [HasPermission("Log.Get-By-Device.Permission", PermissionType.Tenant)]
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
                if (!await _authService.UserHasPermissionAsync<LogController>(Request.Headers["Authorization"].ToString(), device.TenantId))
                {
                    return new CustomForbidResult("User do not have access to this feature for the specified tenant");
                }
                // --- End of authentication check ---
                ICollection<DeviceLog>? logs = await _dbLogService.GetAllByDeviceIdAsync(deviceId);
                if (logs != null)
                {
                    if (logs.Any())
                    {
                        return Ok(logs);
                    }
                }
                return NotFound("No logs found for the specified device.");
            }
            return BadRequest("Invalid Request.");
        }

        [HttpGet("By-Tenant/{tenantId}")]
        [HasPermission("Log.Get-By-Tenant.Permission", PermissionType.Tenant)]
        public async Task<IActionResult> GetAllByTenantIdAsync(int tenantId)
        {
            if (ModelState.IsValid)
            {
                // Authentication check using roles + permissions
                if (!await _authService.UserHasPermissionAsync<LogController>(Request.Headers["Authorization"].ToString(), tenantId))
                {
                    return new CustomForbidResult("User do not have access to this feature for the specified tenant");
                }
                // --- End of authentication check ---
                ICollection<DeviceLog>? logs = await _dbLogService.GetAllByTenantIdAsync(tenantId);
                if (logs != null)
                {
                    if (logs.Any())
                    {
                        return Ok(logs);
                    }
                }
                return NotFound("No logs found for the specified tenant.");
            }
            return BadRequest("Invalid Request.");
        }
    }
}
