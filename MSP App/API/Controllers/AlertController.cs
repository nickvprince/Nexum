using API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SharedComponents.Entities;
using SharedComponents.WebEntities.Requests.AlertRequests;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "v1-Web")]
    public class AlertController : ControllerBase
    {
        private readonly DbAlertService _dbAlertService;
        private readonly DbDeviceService _dbDeviceService;

        public AlertController(DbAlertService dbAlertService, DbDeviceService dbDeviceService)
        {
            _dbAlertService = dbAlertService;
            _dbDeviceService = dbDeviceService;
        }

        [HttpPost("")]
        public async Task<IActionResult> CreateAsync([FromBody] AlertCreateRequest request)
        {
            if(ModelState.IsValid)
            {
                Device? device = await _dbDeviceService.GetAsync(request.DeviceId);
                if (device == null)
                {
                    return NotFound("Device not found.");
                }
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
        public async Task<IActionResult> UpdateAsync([FromBody] AlertUpdateRequest request)
        {
            if (ModelState.IsValid)
            {
                DeviceAlert? alert = await _dbAlertService.GetAsync(request.Id);
                if (alert == null)
                {
                    return NotFound("Alert not found.");
                }
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

        [HttpPost("Acknowledge/{id}")]
        public async Task<IActionResult> AcknowledgeAsync(int id)
        {
            if (ModelState.IsValid)
            {
                DeviceAlert? alert = await _dbAlertService.GetAsync(id);
                if (alert == null)
                {
                    return NotFound("Alert not found.");
                }
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
        public async Task<IActionResult> DeleteAsync(int id)
        {
            if (ModelState.IsValid)
            {
                if (await _dbAlertService.DeleteAsync(id))
                {
                    return Ok($"Alert deleted successfully.");
                }
                return NotFound("Alert not found.");
            }
            return BadRequest("Invalid Request.");
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetAsync(int id)
        {
            if (ModelState.IsValid)
            {
                DeviceAlert? alert = await _dbAlertService.GetAsync(id);
                if (alert != null)
                {
                    return Ok(alert);
                }
                return NotFound("Alert not found.");
            }
            return BadRequest("Invalid Request.");
        }

        [HttpGet("")]
        public async Task<IActionResult> GetAllAsync()
        {
            if (ModelState.IsValid)
            {
                ICollection<DeviceAlert>? alerts = await _dbAlertService.GetAllAsync();
                if (alerts != null)
                {
                    if (alerts.Any())
                    {
                        return Ok(alerts);
                    }
                }
                return NotFound("No alerts found.");
            }
            return BadRequest("Invalid Request.");
        }

        [HttpGet("By-Device/{deviceId}")]
        public async Task<IActionResult> GetAllByDeviceIdAsync(int deviceId)
        {
            if (ModelState.IsValid)
            {
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
        public async Task<IActionResult> GetAllByTenantIdAsync(int tenantId)
        {
            if (ModelState.IsValid)
            {
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
