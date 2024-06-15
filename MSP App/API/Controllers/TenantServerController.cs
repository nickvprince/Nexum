using API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SharedComponents.ResponseEntities;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TenantServerController : ControllerBase
    {
        private readonly DbTenantService _dbTenantService;
        private readonly DbDeviceService _dbDeviceService;
        private readonly DbSecurityService _dbSecurityService;
        public readonly IConfiguration _config;

        public TenantServerController(DbTenantService dbTenantService, DbDeviceService dbDeviceService, DbSecurityService dbSecurityService, IConfiguration config)
        {
            _dbTenantService = dbTenantService;
            _dbDeviceService = dbDeviceService;
            _dbSecurityService = dbSecurityService;
            _config = config;
        }

        [HttpGet("Portal")]
        public async Task<IActionResult> Portal([FromHeader] string apikey)
        {
            if(await _dbSecurityService.ValidateAPIKey(apikey))
            {
                string? portalUrl = _config.GetSection("ApiSettings")?.GetValue<string>("APIBaseUri") + ":" +
                _config.GetSection("ApiSettings")?.GetValue<string>("APIBasePort");

                UrlResponse response = new UrlResponse
                {
                    Url = _config.GetSection("ApiSettings")?.GetValue<string>("APIBaseUri") + ":" +
                          _config.GetSection("ApiSettings")?.GetValue<string>("APIBasePort")
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
