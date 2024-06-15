using API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SharedComponents.DbServices;
using SharedComponents.Entities;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DeviceController : ControllerBase
    {
        private readonly DbDeviceService _dbDeviceService;

        public DeviceController(DbDeviceService dbDeviceService)
        {
            _dbDeviceService = dbDeviceService;
        }

        [HttpPost("Create")]
        public async Task<IActionResult> CreateAsync([FromBody] Device device)
        {
            Device newDevice = await _dbDeviceService.CreateAsync(device);
            if (newDevice != null)
            {
                return Ok(newDevice);
            }
            return BadRequest(new { message = "An error occurred while creating the device." });
        }

        [HttpPut("Update")]
        public async Task<IActionResult> UpdateAsync([FromBody] Device device)
        {
            Device updatedDevice = await _dbDeviceService.UpdateAsync(device);
            if (updatedDevice != null)
            {
                return Ok(updatedDevice);
            }
            return BadRequest(new { message = "An error occurred while updating the device." });
        }

        [HttpDelete("Delete/{id}")]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            if (await _dbDeviceService.DeleteAsync(id))
            {
                return Ok($"Device deleted successfully.");

            }
            return NotFound(new { message = "Device not found." });
        }

        [HttpGet("Get/{id}")]
        public async Task<IActionResult> GetAsync(int id)
        {
            Device? device = await _dbDeviceService.GetAsync(id);
            if (device != null)
            {
                return Ok(device);
            }
            return NotFound(new { message = "Device not found." });
        }

        [HttpGet("Get")]
        public async Task<IActionResult> GetAllAsync()
        {
            ICollection<Device> devices = await _dbDeviceService.GetAllAsync();
            if (devices != null)
            {
                return Ok(devices);
            }
            return NotFound(new { message = "No devices found." });
        }
    }
}
