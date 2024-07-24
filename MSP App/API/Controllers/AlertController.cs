using Microsoft.AspNetCore.Mvc;
using SharedComponents.Entities.DbEntities;
using SharedComponents.Entities.WebEntities.Requests.AlertRequests;
using SharedComponents.Handlers.Attributes.HasPermission;
using SharedComponents.Handlers.Results;
using SharedComponents.Services.APIServices.Interfaces;
using SharedComponents.Services.DbServices.Interfaces;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "v1-Web")]
    public class AlertController : ControllerBase
    {
        private readonly IDbAlertService _dbAlertService;
        private readonly IDbDeviceService _dbDeviceService;
        private readonly IAPIAuthService _authService;

        public AlertController(IDbAlertService dbAlertService, IDbDeviceService dbDeviceService,
            IAPIAuthService authService)
        {
            _dbAlertService = dbAlertService;
            _dbDeviceService = dbDeviceService;
            _authService = authService;
        }

        [HttpPost("")]
        [HasPermission("Alert.Create.Permission", PermissionType.Tenant)]
        public async Task<IActionResult> CreateAsync([FromBody] AlertCreateRequest request)
        {
            if(ModelState.IsValid)
            {
                Device? device = await _dbDeviceService.GetAsync(request.DeviceId);
                if (device == null)
                {
                    return NotFound("Device not found.");
                }
                // Authentication check using roles + permissions
                if (!await _authService.UserHasPermissionAsync<AlertController>(Request.Headers["Authorization"].ToString(), device.TenantId))
                {
                    return new CustomForbidResult("User do not have access to this feature for the specified tenant");
                }
                // --- End of authentication check ---
                DeviceAlert? alert = new DeviceAlert
                {
                    DeviceId = request.DeviceId,
                    Severity = request.Severity,
                    Message = request.Message,
                    Time = request.Time,
                    Acknowledged = false,
                    IsDeleted = false
                };
                alert = await _dbAlertService.CreateAsync(alert);
                if (alert != null)
                {
                    return Ok(alert);
                }
                return BadRequest("An error occurred while creating the alert.");
            }
            return BadRequest("Invalid Request.");
        }

        [HttpPut("")]
        [HasPermission("Alert.Update.Permission", PermissionType.Tenant)]
        public async Task<IActionResult> UpdateAsync([FromBody] AlertUpdateRequest request)
        {
            if (ModelState.IsValid)
            {
                DeviceAlert? alert = await _dbAlertService.GetAsync(request.Id);
                if (alert == null)
                {
                    return NotFound("Alert not found.");
                }
                Device? device = await _dbDeviceService.GetAsync(alert.DeviceId);
                if (device == null)
                {
                    return NotFound("Device not found.");
                }
                // Authentication check using roles + permissions
                if (!await _authService.UserHasPermissionAsync<AlertController>(Request.Headers["Authorization"].ToString(), device.TenantId))
                {
                    return new CustomForbidResult("User do not have access to this feature for the specified tenant");
                }
                // --- End of authentication check ---
                if (alert.IsDeleted)
                {
                    return BadRequest("Alert is deleted.");
                }
                alert.Severity = request.Severity;
                alert.Message = request.Message;
                alert = await _dbAlertService.UpdateAsync(alert);
                if (alert != null)
                {
                    return Ok(alert);
                }
                return BadRequest("An error occurred while updating the alert.");
            }
            return BadRequest("Invalid Request.");
        }
        [HttpPost("{id}/Acknowledge")]
        [HasPermission("Alert.Update.Permission", PermissionType.Tenant)]
        public async Task<IActionResult> AcknowledgeAsync(int id)
        {
            if (ModelState.IsValid)
            {
                DeviceAlert? alert = await _dbAlertService.GetAsync(id);
                if (alert == null)
                {
                    return NotFound("Alert not found.");
                }
                Device? device = await _dbDeviceService.GetAsync(alert.DeviceId);
                if (device == null)
                {
                    return NotFound("Device not found.");
                }
                // Authentication check using roles + permissions
                if (!await _authService.UserHasPermissionAsync<AlertController>(Request.Headers["Authorization"].ToString(), device.TenantId))
                {
                    return new CustomForbidResult("User do not have access to this feature for the specified tenant");
                }
                // --- End of authentication check ---
                if (alert.IsDeleted)
                {
                    return BadRequest("Alert is deleted.");
                }
                if (alert.Acknowledged)
                {
                    return BadRequest("Alert is already acknowledged.");
                }
                alert.Acknowledged = true;
                alert = await _dbAlertService.UpdateAsync(alert);
                if (alert != null)
                {
                    return Ok("Alert acknowledged successfully.");
                }
                return NotFound("Alert not found.");
            }
            return BadRequest("Invalid Request.");
        }

        [HttpDelete("{id}")]
        [HasPermission("Alert.Delete.Permission", PermissionType.Tenant)]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            if (ModelState.IsValid)
            {
                DeviceAlert? alert = await _dbAlertService.GetAsync(id);
                if (alert == null)
                {
                    return NotFound("Alert not found.");
                }
                Device? device = await _dbDeviceService.GetAsync(alert.DeviceId);
                if (device == null)
                {
                    return NotFound("Device not found.");
                }
                // Authentication check using roles + permissions
                if (!await _authService.UserHasPermissionAsync<AlertController>(Request.Headers["Authorization"].ToString(), device.TenantId))
                {
                    return new CustomForbidResult("User do not have access to this feature for the specified tenant");
                }
                // --- End of authentication check ---
                if (await _dbAlertService.DeleteAsync(id))
                {
                    return Ok($"Alert deleted successfully.");
                }
                return NotFound("Alert not found.");
            }
            return BadRequest("Invalid Request.");
        }

        [HttpGet("{id}")]
        [HasPermission("Alert.Get.Permission", PermissionType.Tenant)]
        public async Task<IActionResult> GetAsync(int id)
        {
            if (ModelState.IsValid)
            {
                DeviceAlert? alert = await _dbAlertService.GetAsync(id);
                if (alert != null)
                {
                    Device? device = await _dbDeviceService.GetAsync(alert.DeviceId);
                    if (device == null)
                    {
                        return NotFound("Device not found.");
                    }
                    // Authentication check using roles + permissions
                    if (!await _authService.UserHasPermissionAsync<AlertController>(Request.Headers["Authorization"].ToString(), device.TenantId))
                    {
                        return new CustomForbidResult("User do not have access to this feature for the specified tenant");
                    }
                    // --- End of authentication check ---
                    return Ok(alert);
                }
                return NotFound("Alert not found.");
            }
            return BadRequest("Invalid Request.");
        }

        [HttpGet("")]
        [HasPermission("Alert.Get.Permission", PermissionType.Tenant)]
        public async Task<IActionResult> GetAllAsync()
        {
            if (ModelState.IsValid)
            {
                var tenantIds = await _authService.GetUserAccessibleTenantsAsync(Request.Headers["Authorization"].ToString());
                if (tenantIds == null)
                {
                    return new CustomForbidResult("User does not have any tenant permissions");
                }
                List<DeviceAlert>? alerts = new List<DeviceAlert>();
                foreach (var tenantId in tenantIds)
                {
                    if (tenantId != null)
                    {
                        var tenantAlerts = await _dbAlertService.GetAllByTenantIdAsync((int)tenantId);
                        if (tenantAlerts != null)
                        {
                            alerts.AddRange(tenantAlerts);
                        }
                    }
                }
                if (alerts != null)
                {
                    if (alerts.Any())
                    {
                        return Ok(alerts.Distinct());
                    }
                }
                return NotFound("No alerts found.");
            }
            return BadRequest("Invalid Request.");
        }

        [HttpGet("By-Device/{deviceId}")]
        [HasPermission("Alert.Get.Permission", PermissionType.Tenant)]
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
                if (!await _authService.UserHasPermissionAsync<AlertController>(Request.Headers["Authorization"].ToString(), device.TenantId))
                {
                    return new CustomForbidResult("User do not have access to this feature for the specified tenant");
                }
                // --- End of authentication check ---
                ICollection<DeviceAlert>? alerts = await _dbAlertService.GetAllByDeviceIdAsync(deviceId);
                if (alerts != null)
                {
                    if (alerts.Any())
                    {
                        return Ok(alerts);
                    }
                }
                return NotFound("No alerts found for the specified device.");
            }
            return BadRequest("Invalid Request.");
        }

        [HttpGet("By-Tenant/{tenantId}")]
        [HasPermission("Alert.Get.Permission", PermissionType.Tenant)]
        public async Task<IActionResult> GetAllByTenantIdAsync(int tenantId)
        {
            if (ModelState.IsValid)
            {
                // Authentication check using roles + permissions
                if (!await _authService.UserHasPermissionAsync<AlertController>(Request.Headers["Authorization"].ToString(), tenantId))
                {
                    return new CustomForbidResult("User do not have access to this feature for the specified tenant");
                }
                // --- End of authentication check ---
                ICollection<DeviceAlert>? alerts = await _dbAlertService.GetAllByTenantIdAsync(tenantId);
                if (alerts != null)
                {
                    if (alerts.Any())
                    {
                        return Ok(alerts);
                    }
                }
                return NotFound("No alerts found for the specified tenant.");
            }
            return BadRequest("Invalid Request.");
        }
    }
}
