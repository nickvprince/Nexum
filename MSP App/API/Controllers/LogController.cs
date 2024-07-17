using API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SharedComponents.DbServices;
using SharedComponents.Entities;
using SharedComponents.WebEntities.Requests.LogRequests;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "v1-Web")]
    public class LogController : ControllerBase
    {
        private readonly IDbLogService _dbLogService;
        private readonly IDbDeviceService _dbDeviceService;

        public LogController(IDbLogService dbLogService, IDbDeviceService dbDeviceService)
        {
            _dbLogService = dbLogService;
            _dbDeviceService = dbDeviceService;
        }

        [HttpPost("")]
        public async Task<IActionResult> CreateAsync([FromBody] LogCreateRequest request)
        {
            if (ModelState.IsValid)
            {
                Device? device = await _dbDeviceService.GetAsync(request.DeviceId);
                if (device == null)
                {
                    return NotFound("Device not found.");
                }
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
        public async Task<IActionResult> UpdateAsync([FromBody] LogUpdateRequest request)
        {
            if (ModelState.IsValid)
            {
                DeviceLog? log = await _dbLogService.GetAsync(request.Id);
                if (log == null)
                {
                    return NotFound("Log not found.");
                }
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
        public async Task<IActionResult> AcknowledgeAsync(int id)
        {
            if (ModelState.IsValid)
            {
                DeviceLog? log = await _dbLogService.GetAsync(id);
                if (log == null)
                {
                    return NotFound("Log not found.");
                }
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
        public async Task<IActionResult> DeleteAsync(int id)
        {
            if (ModelState.IsValid)
            {
                if (await _dbLogService.DeleteAsync(id))
                {
                    return Ok($"Log deleted successfully.");
                }
                return NotFound("Log not found.");
            }
            return BadRequest("Invalid Request.");
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetAsync(int id)
        {
            if (ModelState.IsValid)
            {
                DeviceLog? log = await _dbLogService.GetAsync(id);
                if (log != null)
                {
                    return Ok(log);
                }
                return NotFound("Log not found.");
            }
            return BadRequest("Invalid Request.");
        }

        [HttpGet("")]
        public async Task<IActionResult> GetAllAsync()
        {
            if (ModelState.IsValid)
            {
                ICollection<DeviceLog>? logs = await _dbLogService.GetAllAsync();
                if (logs != null)
                {
                    return Ok(logs);
                }
                return NotFound("Logs not found.");
            }
            return BadRequest("Invalid Request.");
        }

        [HttpGet("By-Device/{deviceId}")]
        public async Task<IActionResult> GetAllByDeviceIdAsync(int deviceId)
        {
            if (ModelState.IsValid)
            {
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
        public async Task<IActionResult> GetAllByTenantIdAsync(int tenantId)
        {
            if (ModelState.IsValid)
            {
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
