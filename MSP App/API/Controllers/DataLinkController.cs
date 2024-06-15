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
        }

        [HttpGet("Portal")]
        public async Task<IActionResult> Portal([FromHeader] string apikey)
        {
            if(await _dbSecurityService.ValidateAPIKey(apikey))
            {
                string? portalUrl = _webAppBaseUrl + "/Account/login";

                UrlResponse response = new UrlResponse
                {
                    Url = portalUrl
                };
                return Ok(response);
            }
            return Unauthorized("Invalid API Key.");
        }

        [HttpGet("Nexum.exe")]
        public IActionResult Nexum()
        {
            //Get the Nexum.exe file
            return Ok($"Retrieved Nexum.exe successfully.");
        }

        [HttpGet("Watchdog.exe")]
        public IActionResult WatchDog()
        {
            //Get the Watchdog.exe file
            return Ok($"Retrieved Watchdog.exe successfully.");
        }

        [HttpGet("Verify")]
        public IActionResult VerifyInstallation(string apiKey, string installationKey)
        {
            //Verify the installation
            return Ok($"Installation Verified.");
        }
    }
}
