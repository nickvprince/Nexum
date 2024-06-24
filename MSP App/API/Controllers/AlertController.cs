using API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SharedComponents.Entities;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "v1-Web")]
    public class AlertController : ControllerBase
    {
        private readonly DbAlertService _dbAlertService;

        public AlertController(DbAlertService dbAlertService)
        {
            _dbAlertService = dbAlertService;
        }

        [HttpPost("")]
        public async Task<IActionResult> CreateAsync([FromBody] DeviceAlert alert)
        {
            DeviceAlert? newAlert = await _dbAlertService.CreateAsync(alert);
            if (newAlert != null)
            {
                return Ok(newAlert);
            }
            return BadRequest("An error occurred while creating the alert.");
        }

        [HttpPut("")]
        public async Task<IActionResult> UpdateAsync([FromBody] DeviceAlert alert)
        {
            DeviceAlert? updatedAlert = await _dbAlertService.UpdateAsync(alert);
            if (updatedAlert != null)
            {
                return Ok(updatedAlert);
            }
            return BadRequest("An error occurred while updating the alert.");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            if (await _dbAlertService.DeleteAsync(id))
            {
                return Ok($"Alert deleted successfully.");
            }
            return NotFound("Alert not found.");
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetAsync(int id)
        {
            DeviceAlert? alert = await _dbAlertService.GetAsync(id);
            if (alert != null)
            {
                return Ok(alert);
            }
            return NotFound("Alert not found.");
        }

        [HttpGet("")]
        public async Task<IActionResult> GetAllAsync()
        {
            ICollection<DeviceAlert> alerts = await _dbAlertService.GetAllAsync();
            if (alerts.Any())
            {
                return Ok(alerts);
            }
            return NotFound("No alerts found.");
        }
    }
}
