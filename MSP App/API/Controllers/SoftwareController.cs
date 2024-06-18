using API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SharedComponents.DbServices;
using SharedComponents.Entities;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SoftwareController : ControllerBase
    {
        private readonly DbSoftwareService _dbSoftwareService;
        private readonly DbSecurityService _dbSecurityService;
        private readonly IConfiguration _config;
        private readonly string _softwareFolder;

        public SoftwareController(DbSoftwareService dbSoftwareService, DbSecurityService dbSecurityService, IConfiguration config)
        {
            _dbSoftwareService = dbSoftwareService;
            _dbSecurityService = dbSecurityService;
            _config = config;
            _softwareFolder = Path.Combine(Directory.GetCurrentDirectory(), _config.GetSection("ApiAppSettings")?.GetValue<string>("SoftwareFolder")); 
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

        [HttpGet("Version/{id}")]
        public async Task<IActionResult> GetAsync(int id)
        {
            SoftwareFile? softwareFile = await _dbSoftwareService.GetAsync(id);
            if (softwareFile != null)
            {
                return Ok(softwareFile);
            }
            return NotFound("Software file not found.");
        }

        [HttpGet("Latest-Nexum-Version")]
        public async Task<IActionResult> GetLatestNexumAsync()
        {
            SoftwareFile? softwareFile = await _dbSoftwareService.GetLatestNexumAsync();
            if (softwareFile != null)
            {
                return Ok(softwareFile);
            }
            return NotFound("No Nexum software file found.");
        }

        [HttpGet("Latest-Nexum-Server-Version")]
        public async Task<IActionResult> GetLatestNexumServerAsync()
        {
            SoftwareFile? softwareFile = await _dbSoftwareService.GetLatestNexumServerAsync();
            if (softwareFile != null)
            {
                return Ok(softwareFile);
            }
            return NotFound("No Nexum Server software file found.");
        }

        [HttpGet("Latest-Nexum-Service-Version")]
        public async Task<IActionResult> GetLatestNexumServiceAsync()
        {
            SoftwareFile? softwareFile = await _dbSoftwareService.GetLatestNexumServiceAsync();
            if (softwareFile != null)
            {
                return Ok(softwareFile);
            }
            return NotFound("No Nexum Service software file found.");
        }

        [HttpGet("")]
        public async Task<IActionResult> GetAllAsync()
        {
            ICollection<SoftwareFile> softwareFiles = await _dbSoftwareService.GetAllAsync();
            if (softwareFiles != null)
            {
                return Ok(softwareFiles);
            }
            return NotFound("No software files found.");
        }

        [HttpGet("Nexum")]
        public async Task<IActionResult> NexumAsync([FromHeader] string apikey)
        {
            if (await _dbSecurityService.ValidateAPIKey(apikey))
            {
                string nexumFilePath = Path.Combine(_softwareFolder, "Nexum.exe");
                if (!System.IO.File.Exists(nexumFilePath))
                {
                    return NotFound("File not found.");
                }

                var memory = new MemoryStream();
                using (var stream = new FileStream(nexumFilePath, FileMode.Open))
                {
                    stream.CopyTo(memory);
                }
                memory.Position = 0;

                return File(memory, "application/octet-stream", Path.GetFileName(nexumFilePath));
            }
            return Unauthorized("Invalid API Key.");
        }

        [HttpGet("NexumServer")]
        public async Task<IActionResult> NexumServerAsync([FromHeader] string apikey)
        {
            if (await _dbSecurityService.ValidateAPIKey(apikey))
            {
                string nexumServerFilePath = Path.Combine(_softwareFolder, "NexumServer.exe");
                if (!System.IO.File.Exists(nexumServerFilePath))
                {
                    return NotFound("File not found.");
                }

                var memory = new MemoryStream();
                using (var stream = new FileStream(nexumServerFilePath, FileMode.Open))
                {
                    stream.CopyTo(memory);
                }
                memory.Position = 0;

                return File(memory, "application/octet-stream", Path.GetFileName(nexumServerFilePath));
            }
            return Unauthorized("Invalid API Key.");
        }

        [HttpGet("NexumService")]
        public async Task<IActionResult> NexumServiceAsync([FromHeader] string apikey)
        {
            if (await _dbSecurityService.ValidateAPIKey(apikey))
            {
                string nexumServiceFilePath = Path.Combine(_softwareFolder, "NexumService.exe");
                if (!System.IO.File.Exists(nexumServiceFilePath))
                {
                    return NotFound("File not found.");
                }

                var memory = new MemoryStream();
                using (var stream = new FileStream(nexumServiceFilePath, FileMode.Open))
                {
                    stream.CopyTo(memory);
                }
                memory.Position = 0;

                return File(memory, "application/octet-stream", Path.GetFileName(nexumServiceFilePath));
            }
            return Unauthorized("Invalid API Key.");
        }
    }
}
