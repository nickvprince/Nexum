using API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SharedComponents.DbServices;
using SharedComponents.Entities;
using SharedComponents.RequestEntities;
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
        private readonly DbSoftwareService _dbSoftwareService;
        private readonly IConfiguration _config;
        private readonly string _apiBaseUrl;
        private readonly string _webAppBaseUrl;
        private readonly string _filePath;

        public DataLinkController(DbTenantService dbTenantService, DbDeviceService dbDeviceService, DbSecurityService dbSecurityService, DbSoftwareService dbSoftwareService,  IConfiguration config)
        {
            _dbTenantService = dbTenantService;
            _dbDeviceService = dbDeviceService;
            _dbSecurityService = dbSecurityService;
            _dbSoftwareService = dbSoftwareService;
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

        [HttpPost("Register-Server")]
        public async Task<IActionResult> RegisterServerAsync([FromHeader] string apikey, [FromBody] ServerRegistrationRequest request)
        {
            if (await _dbSecurityService.ValidateAPIKey(apikey))
            {
                Tenant? tenant = await _dbTenantService.GetByApiKeyAsync(apikey);
                if (tenant == null)
                {
                    return NotFound("Tenant not found.");
                }

                InstallationKey? installationKey = await _dbTenantService.GetInstallationKeyAsync(request.InstallationKey);
                if (installationKey == null)
                {
                    return Unauthorized("Invalid Installation Key.");
                }

                if (installationKey.TenantId != tenant.Id)
                {
                    return Unauthorized("Invalid Installation Key.");
                }

                if (!installationKey.IsActive)
                {
                    return Unauthorized("Inactive Installation Key.");
                }

                Device? serverDevice = new Device
                {
                    TenantId = tenant.Id,
                    DeviceInfo = new DeviceInfo
                    {
                        Name = request.Name,
                        Uuid = request.Uuid,
                        IpAddress = request.IpAddress,
                        Port = request.Port,
                        MACAddresses = request.MACAddresses,
                        Type = DeviceType.Server,
                    }
                };
                serverDevice = await _dbDeviceService.CreateAsync(serverDevice);
                if (serverDevice != null)
                {
                    if (serverDevice.DeviceInfo != null)
                    {
                        if (serverDevice.DeviceInfo.MACAddresses != null)
                        {
                            installationKey.IsActive = false;
                            installationKey = await _dbTenantService.UpdateInstallationKeyAsync(installationKey);

                            if (installationKey != null)
                            {
                                ServerRegistrationResponse response = new ServerRegistrationResponse
                                {
                                    Id = serverDevice.Id,
                                    Name = serverDevice.DeviceInfo.Name,
                                    Uuid = serverDevice.DeviceInfo.Uuid,
                                    IpAddress = serverDevice.DeviceInfo.IpAddress,
                                    Port = serverDevice.DeviceInfo.Port,
                                    Type = serverDevice.DeviceInfo.Type,
                                    MACAddresses = new List<MACAddressResponse>(serverDevice.DeviceInfo.MACAddresses.Select(m => new MACAddressResponse
                                    {
                                        Id = m.Id,
                                        Address = m.Address
                                    })),
                                    IsVerified = serverDevice.IsVerified
                                };
                                return Ok(response);
                            }
                        }
                    }
                }
                return BadRequest("An error occurred while registering the server.");
            }
            return Unauthorized("Invalid API Key.");
        }

        [HttpPost("Register-Client")]
        public async Task<IActionResult> RegisterClientAsync([FromHeader] string apikey, [FromBody] DeviceRegistrationRequest request)
        {
            if (await _dbSecurityService.ValidateAPIKey(apikey))
            {
                Tenant? tenant = await _dbTenantService.GetByApiKeyAsync(apikey);
                if (tenant == null)
                {
                    return NotFound("Tenant not found.");
                }

                if (tenant.Devices != null)
                {
                    Device? server = tenant.Devices.FirstOrDefault(d => d.DeviceInfo.Type == DeviceType.Server);
                    if (!server.IsVerified)
                    {
                        return BadRequest("Server not verified.");
                    }
                }

                InstallationKey? installationKey = await _dbTenantService.GetInstallationKeyAsync(request.InstallationKey);
                if (installationKey == null)
                {
                    return Unauthorized("Invalid Installation Key.");
                }

                if (installationKey.TenantId != tenant.Id)
                {
                    return Unauthorized("Invalid Installation Key.");
                }

                if (!installationKey.IsActive)
                {
                    return Unauthorized("Inactive Installation Key.");
                }

                Device? clientDevice = new Device
                {
                    TenantId = tenant.Id,
                    DeviceInfo = new DeviceInfo
                    {
                        Name = request.Name,
                        ClientId = request.Client_Id,
                        Uuid = request.Uuid,
                        IpAddress = request.IpAddress,
                        Port = request.Port,
                        MACAddresses = request.MACAddresses,
                        Type = DeviceType.Desktop,
                    }
                };
                clientDevice = await _dbDeviceService.CreateAsync(clientDevice);
                if (clientDevice != null)
                {
                    if (clientDevice.DeviceInfo != null)
                    {
                        if (clientDevice.DeviceInfo.MACAddresses != null)
                        {
                            installationKey.IsActive = false;
                            installationKey = await _dbTenantService.UpdateInstallationKeyAsync(installationKey);

                            if (installationKey != null)
                            {
                                DeviceRegistrationResponse response = new DeviceRegistrationResponse
                                {
                                    Id = clientDevice.Id,
                                    Name = clientDevice.DeviceInfo.Name,
                                    Client_Id = clientDevice.DeviceInfo.ClientId,
                                    Uuid = clientDevice.DeviceInfo.Uuid,
                                    IpAddress = clientDevice.DeviceInfo.IpAddress,
                                    Port = clientDevice.DeviceInfo.Port,
                                    Type = clientDevice.DeviceInfo.Type,
                                    MACAddresses = new List<MACAddressResponse>(clientDevice.DeviceInfo.MACAddresses.Select(m => new MACAddressResponse
                                    {
                                        Id = m.Id,
                                        Address = m.Address
                                    })),
                                    IsVerified = clientDevice.IsVerified
                                };
                                return Ok(response);
                            }
                        }
                    }
                }
                return BadRequest("An error occurred while registering the client.");
            }
            return Unauthorized("Invalid API Key.");
        }

        [HttpPost("Verify")]
        public async Task<IActionResult> VerifyInstallationAsync([FromHeader] string apikey, [FromBody] VerifyInstallationRequest request)
        {
            if (await _dbSecurityService.ValidateAPIKey(apikey))
            {
                Tenant? tenant = await _dbTenantService.GetByApiKeyAsync(apikey);
                if (tenant == null)
                {
                    return NotFound("Tenant not found.");
                }

                Device? device = await _dbDeviceService.GetByUuidAsync(request.Uuid);
                if (device == null)
                {
                    return NotFound("Device not found.");
                }

                if (device.TenantId != tenant.Id)
                {
                    return Unauthorized("Invalid Device.");
                }

                if (device.IsVerified)
                {
                    return BadRequest("Device has already been verified.");
                }

                InstallationKey? installationKey = await _dbTenantService.GetInstallationKeyAsync(request.InstallationKey);
                if (installationKey == null)
                {
                    return Unauthorized("Invalid Installation Key.");
                }

                device.IsVerified = true;
                device = await _dbDeviceService.UpdateAsync(device);

                if (device != null) 
                {
                    VerifyInstallationResponse response = new VerifyInstallationResponse
                    {
                        Name = device.DeviceInfo?.Name,
                        IsVerified = device.IsVerified
                    };
                    return Ok(response);
                }
                return BadRequest("An error occurred while verifying the device."); 
            }
            return Unauthorized("Invalid API Key.");
        }

        [HttpGet("Check-For-Updates")]
        public async Task<IActionResult> CheckForUpdatesAsync([FromHeader] string apikey, [FromBody] CheckForUpdatesRequest request)
        {
            if (await _dbSecurityService.ValidateAPIKey(apikey))
            {
                Tenant? tenant = await _dbTenantService.GetByApiKeyAsync(apikey);
                if (tenant == null)
                {
                    return NotFound("Tenant not found.");
                }

                if (request.NexumVersion == null)
                {
                    return BadRequest("Invalid Nexum Version.");
                }

                if (request.NexumServerVersion == null)
                {
                    return BadRequest("Invalid Nexum Server Version.");
                }

                if (request.NexumServiceVersion == null)
                {
                    return BadRequest("Invalid Nexum Service Version.");
                }

                SoftwareFile? nexumFile = await _dbSoftwareService.GetLatestNexumAsync();
                SoftwareFile? nexumServerFile = await _dbSoftwareService.GetLatestNexumServerAsync();
                SoftwareFile? nexumServiceFile = await _dbSoftwareService.GetLatestNexumServiceAsync();

                if (nexumFile == null || nexumServerFile == null || nexumServiceFile == null)
                {
                    return NotFound("Failed to retrieve file versions.");
                }

                CheckForUpdatesResponse response = new CheckForUpdatesResponse
                {
                    NexumUpdateAvailable = nexumFile.Version != request.NexumVersion,
                    NexumServerUpdateAvailable = nexumServerFile.Version != request.NexumServerVersion,
                    NexumServiceUpdateAvailable = nexumServiceFile.Version != request.NexumServiceVersion
                };

                return Ok(response);
            }
            return Unauthorized("Invalid API Key.");
        }
    }
}

