using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TenantServerController : ControllerBase
    {
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
