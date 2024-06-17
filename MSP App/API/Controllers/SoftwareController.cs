using API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SharedComponents.Entities;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SoftwareController : ControllerBase
    {
        private readonly DbSoftwareService _dbSoftwareService;

        public SoftwareController(DbSoftwareService dbSoftwareService)
        {
            _dbSoftwareService = dbSoftwareService;
        }

        [HttpPost("Create")]
        public async Task<IActionResult> CreateAsync([FromForm] SoftwareFile softwareFile, [FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }

            // Save the file to a specific path
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "Download", file.FileName);

            try
            {
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Create the SoftwareFile record in the database
                SoftwareFile? newSoftwareFile = await _dbSoftwareService.CreateAsync(softwareFile);
                if (newSoftwareFile != null)
                {
                    return Ok(newSoftwareFile);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to upload file: {ex.Message}");
            }

            return BadRequest($"Failed to upload file.");
        }

        [HttpGet("Get/{id}")]
        public async Task<IActionResult> GetAsync(int id)
        {
            SoftwareFile? softwareFile = await _dbSoftwareService.GetAsync(id);
            if (softwareFile != null)
            {
                return Ok(softwareFile);
            }
            return NotFound("Software file not found.");
        }

        [HttpGet("Get-Latest-Nexum")]
        public async Task<IActionResult> GetLatestNexumAsync()
        {
            SoftwareFile? softwareFile = await _dbSoftwareService.GetLatestNexumAsync();
            if (softwareFile != null)
            {
                return Ok(softwareFile);
            }
            return NotFound("No Nexum software file found.");
        }

        [HttpGet("Get-Latest-Nexum-Server")]
        public async Task<IActionResult> GetLatestNexumServerAsync()
        {
            SoftwareFile? softwareFile = await _dbSoftwareService.GetLatestNexumServerAsync();
            if (softwareFile != null)
            {
                return Ok(softwareFile);
            }
            return NotFound("No Nexum Server software file found.");
        }

        [HttpGet("Get-Latest-Nexum-Service")]
        public async Task<IActionResult> GetLatestNexumServiceAsync()
        {
            SoftwareFile? softwareFile = await _dbSoftwareService.GetLatestNexumServiceAsync();
            if (softwareFile != null)
            {
                return Ok(softwareFile);
            }
            return NotFound("No Nexum Service software file found.");
        }

        [HttpGet("Get")]
        public async Task<IActionResult> GetAllAsync()
        {
            ICollection<SoftwareFile> softwareFiles = await _dbSoftwareService.GetAllAsync();
            if (softwareFiles != null)
            {
                return Ok(softwareFiles);
            }
            return NotFound("No software files found.");
        }
    }
}
