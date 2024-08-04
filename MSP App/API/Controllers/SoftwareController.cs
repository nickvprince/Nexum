using Microsoft.AspNetCore.Mvc;
using SharedComponents.Entities.DbEntities;
using SharedComponents.Handlers.Attributes.HasPermission;
using SharedComponents.Services.DbServices.Interfaces;

namespace API.Controllers
{
    /// <summary>
    /// Controller for managing software files.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "v1-Server")]
    public class SoftwareController : ControllerBase
    {
        private readonly IDbSoftwareService _dbSoftwareService;
        private readonly IDbSecurityService _dbSecurityService;
        private readonly IConfiguration _config;
        private readonly string _softwareFolder;

        /// <summary>
        /// Initializes a new instance of the <see cref="SoftwareController"/> class.
        /// </summary>
        /// <param name="dbSoftwareService">The software service.</param>
        /// <param name="dbSecurityService">The security service.</param>
        /// <param name="config">The configuration.</param>
        public SoftwareController(IDbSoftwareService dbSoftwareService, IDbSecurityService dbSecurityService, IConfiguration config)
        {
            _dbSoftwareService = dbSoftwareService;
            _dbSecurityService = dbSecurityService;
            _config = config;
            _softwareFolder = Path.Combine(Directory.GetCurrentDirectory(), _config.GetSection("ApiAppSettings")?.GetValue<string>("SoftwareFolder")); 
        }

        /// <summary>
        /// Creates a new software file.
        /// </summary>
        /// <param name="softwareFile">The software file metadata.</param>
        /// <param name="file">The software file to upload.</param>
        /// <returns>An action result containing the created software file.</returns>
        [HttpPost("Create")]
        [HasPermission("Software.Create.Permission", PermissionType.System)]
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

        /// <summary>
        /// Gets a software file by ID.
        /// </summary>
        /// <param name="id">The ID of the software file to retrieve.</param>
        /// <returns>An action result containing the software file.</returns>
        [HttpGet("{id}")]
        [HasPermission("Software.Get.Permission", PermissionType.System)]
        public async Task<IActionResult> GetAsync(int id)
        {
            SoftwareFile? softwareFile = await _dbSoftwareService.GetAsync(id);
            if (softwareFile != null)
            {
                return Ok(softwareFile);
            }
            return NotFound("Software file not found.");
        }

        /// <summary>
        /// Gets all software files.
        /// </summary>
        /// <returns>An action result containing all software files.</returns>
        [HttpGet("")]
        [HasPermission("Software.Get.Permission", PermissionType.System)]
        public async Task<IActionResult> GetAllAsync()
        {
            ICollection<SoftwareFile> softwareFiles = await _dbSoftwareService.GetAllAsync();
            if (softwareFiles != null)
            {
                return Ok(softwareFiles);
            }
            return NotFound("No software files found.");
        }

        /// <summary>
        /// Gets the latest Nexum software file.
        /// </summary>
        /// <returns>An action result containing the latest Nexum software file.</returns>
        [HttpGet("Latest-Nexum-Version")]
        [HasPermission("Software.Get.Permission", PermissionType.System)]
        public async Task<IActionResult> GetLatestNexumAsync()
        {
            SoftwareFile? softwareFile = await _dbSoftwareService.GetLatestNexumAsync();
            if (softwareFile != null)
            {
                return Ok(softwareFile);
            }
            return NotFound("No Nexum software file found.");
        }

        /// <summary>
        /// Gets the latest Nexum Server software file.
        /// </summary>
        /// <returns>An action result containing the latest Nexum Server software file.</returns>
        [HttpGet("Latest-Nexum-Server-Version")]
        [HasPermission("Software.Get.Permission", PermissionType.System)]
        public async Task<IActionResult> GetLatestNexumServerAsync()
        {
            SoftwareFile? softwareFile = await _dbSoftwareService.GetLatestNexumServerAsync();
            if (softwareFile != null)
            {
                return Ok(softwareFile);
            }
            return NotFound("No Nexum Server software file found.");
        }

        /// <summary>
        /// Gets the latest Nexum Service software file.
        /// </summary>
        /// <returns>An action result containing the latest Nexum Service software file.</returns>
        [HttpGet("Latest-Nexum-Service-Version")]
        [HasPermission("Software.Get.Permission", PermissionType.System)]
        public async Task<IActionResult> GetLatestNexumServiceAsync()
        {
            SoftwareFile? softwareFile = await _dbSoftwareService.GetLatestNexumServiceAsync();
            if (softwareFile != null)
            {
                return Ok(softwareFile);
            }
            return NotFound("No Nexum Service software file found.");
        }

        /// <summary>
        /// Downloads the Nexum software file.
        /// </summary>
        /// <param name="apikey">The API key for authentication.</param>
        /// <returns>The Nexum software file.</returns>
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

        /// <summary>
        /// Downloads the Nexum Server software file.
        /// </summary>
        /// <param name="apikey">The API key for authentication.</param>
        /// <returns>The Nexum Server software file.</returns>
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

        /// <summary>
        /// Downloads the Nexum Service software file.
        /// </summary>
        /// <param name="apikey">The API key for authentication.</param>
        /// <returns>The Nexum Service software file.</returns>
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
