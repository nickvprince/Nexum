using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SharedComponents.Services.APIRequestServices.Interfaces;

namespace App.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DownloadController : ControllerBase
    {
        private readonly IAPIRequestSoftwareService _softwareService;

        public DownloadController(IAPIRequestSoftwareService softwareService)
        {
            _softwareService = softwareService;
        }

        [HttpGet("Installer")]
        public async Task<IActionResult> GetInstaller()
        {
            var installerFile = await _softwareService.GetNexumInstallerAsync();
            if (installerFile != null)
            {
                return await Task.FromResult(File(installerFile, "application/octet-stream", "Installer.exe"));
            }
            return await Task.FromResult(NotFound());
        }
    }
}
