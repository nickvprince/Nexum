using API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SharedComponents.ResponseEntities;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DataLinkController : ControllerBase
    {
        private readonly DbTenantService _dbTenantService;
        private readonly DbDeviceService _dbDeviceService;
        private readonly DbSecurityService _dbSecurityService;
        private readonly IConfiguration _config;
        private readonly string _apiBaseUrl;
        private readonly string _webAppBaseUrl;
        private readonly string _filePath;

        public DataLinkController(DbTenantService dbTenantService, DbDeviceService dbDeviceService, DbSecurityService dbSecurityService, IConfiguration config)
        {
            _dbTenantService = dbTenantService;
            _dbDeviceService = dbDeviceService;
            _dbSecurityService = dbSecurityService;
            _config = config;
            _apiBaseUrl = _config.GetSection("ApiAppSettings")?.GetValue<string>("APIBaseUri") + ":" +
                          _config.GetSection("ApiAppSettings")?.GetValue<string>("APIBasePort");
            _webAppBaseUrl = _config.GetSection("ApiAppSettings")?.GetValue<string>("BaseUri") + ":" +
                             _config.GetSection("ApiAppSettings")?.GetValue<string>("BasePort");
            _filePath = Path.Combine(Directory.GetCurrentDirectory(), "Download");
        }

        [HttpGet("Urls")]
        public async Task<IActionResult> UrlsAsync([FromHeader] string apikey)
        {
            if(await _dbSecurityService.ValidateAPIKey(apikey))
            {
                UrlResponse response = new UrlResponse
                {
                    PortalUrl = _webAppBaseUrl + "/Account/login",
                    NexumUrl = _apiBaseUrl + "/api/DataLink/Nexum",
                    NexumServerUrl = _apiBaseUrl + "/api/DataLink/NexumServer",
                    NexumServiceUrl = _apiBaseUrl + "/api/DataLink/NexumService"
                };
                return Ok(response);
            }
            return Unauthorized("Invalid API Key.");
        }

        [HttpGet("Nexum")]
        public async Task<IActionResult> NexumAsync([FromHeader] string apikey)
        {
            if (await _dbSecurityService.ValidateAPIKey(apikey))
            {
                string nexumFilePath = Path.Combine(_filePath, "Nexum.exe");
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
                string nexumServerFilePath = Path.Combine(_filePath, "NexumServer.exe");
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
                string nexumServiceFilePath = Path.Combine(_filePath, "NexumService.exe");
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

        [HttpGet("Verify")]
        public IActionResult VerifyInstallation(string apiKey, string installationKey)
        {
            return Ok($"Installation Verified.");
        }
    }
}
