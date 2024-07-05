using API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SharedComponents.Entities;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "v1-Web")]
    public class LogController : ControllerBase
    {
        private readonly DbLogService _dbLogService;

        public LogController(DbLogService dbLogService)
        {
            _dbLogService = dbLogService;
        }

        [HttpPost("")]
        public async Task<IActionResult> CreateAsync([FromBody] DeviceLog log)
        {
            DeviceLog? newLog = await _dbLogService.CreateAsync(log);
            if (newLog != null)
            {
                return Ok(newLog);
            }
            return BadRequest("An error occurred while creating the log.");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            if (await _dbLogService.DeleteAsync(id))
            {
                return Ok($"Log deleted successfully.");
            }
            return NotFound("Log not found.");
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetAsync(int id)
        {
            DeviceLog? log = await _dbLogService.GetAsync(id);
            if (log != null)
            {
                return Ok(log);
            }
            return NotFound("Log not found.");
        }

        [HttpGet("")]
        public async Task<IActionResult> GetAllAsync()
        {
            ICollection<DeviceLog>? logs = await _dbLogService.GetAllAsync();
            if (logs != null)
            {
                return Ok(logs);
            }
            return NotFound("Logs not found.");
        }
    }
}
