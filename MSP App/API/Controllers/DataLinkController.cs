﻿using API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SharedComponents.DbServices;
using SharedComponents.Entities;
using SharedComponents.RequestEntities;
using SharedComponents.ResponseEntities;
using SharedComponents.Utilities;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "v1-Server")]
    public class DataLinkController : ControllerBase
    {
        private readonly DbTenantService _dbTenantService;
        private readonly DbDeviceService _dbDeviceService;
        private readonly DbSecurityService _dbSecurityService;
        private readonly DbSoftwareService _dbSoftwareService;
        private readonly DbAlertService _dbAlertService;
        private readonly DbLogService _dbLogService;
        private readonly DbInstallationKeyService _dbInstallationKeyService;
        private readonly IConfiguration _config;
        private readonly string _apiBaseUrl;
        private readonly string _webAppBaseUrl;

        public DataLinkController(DbTenantService dbTenantService, DbDeviceService dbDeviceService, 
            DbSecurityService dbSecurityService, DbSoftwareService dbSoftwareService, 
            DbAlertService dbAlertService, DbLogService dbLogService,
            DbInstallationKeyService dbInstallationKeyService, IConfiguration config)
        {
            _dbTenantService = dbTenantService;
            _dbDeviceService = dbDeviceService;
            _dbSecurityService = dbSecurityService;
            _dbSoftwareService = dbSoftwareService;
            _dbAlertService = dbAlertService;
            _dbLogService = dbLogService;
            _dbInstallationKeyService = dbInstallationKeyService;
            _config = config;
            _apiBaseUrl = _config.GetSection("ApiAppSettings")?.GetValue<string>("APIBaseUri") + ":" +
                          _config.GetSection("ApiAppSettings")?.GetValue<string>("APIBasePort") + "/api";
            _webAppBaseUrl = _config.GetSection("ApiAppSettings")?.GetValue<string>("BaseUri") + ":" +
                             _config.GetSection("ApiAppSettings")?.GetValue<string>("BasePort");
        }

        [HttpGet("Urls")]
        public async Task<IActionResult> UrlsAsync([FromHeader] string apikey)
        {
            if(await _dbSecurityService.ValidateAPIKey(apikey))
            {
                UrlResponse response = new UrlResponse
                {
                    PortalUrl = _webAppBaseUrl + "/Account/login",
                    NexumUrl = _apiBaseUrl + "/Software/Nexum",
                    NexumServerUrl = _apiBaseUrl + "/Software/NexumServer",
                    NexumServiceUrl = _apiBaseUrl + "/Software/NexumService"
                };
                return Ok(response);
            }
            return Unauthorized("Invalid API Key.");
        }

        [HttpPost("Register")]
        public async Task<IActionResult> RegisterAsync([FromHeader] string apikey, [FromBody] DeviceRegistrationRequest request)
        {
            if (await _dbSecurityService.ValidateAPIKey(apikey))
            {
                Tenant? tenant = await _dbTenantService.GetByApiKeyAsync(apikey);
                if (tenant == null)
                {
                    return NotFound("Tenant not found.");
                }

                InstallationKey? installationKey = await _dbInstallationKeyService.GetByInstallationKeyAsync(request.InstallationKey);
                if (installationKey == null)
                {
                    return Unauthorized("Invalid Installation Key.");
                }

                if (installationKey.TenantId != tenant.Id)
                {
                    return Unauthorized("Invalid Installation Key.");
                }

                if (!installationKey.IsActive || installationKey.IsDeleted)
                {
                    return Unauthorized("Inactive Installation Key.");
                }

                if (!Enum.IsDefined(typeof(DeviceType), request.Type))
                {
                    return BadRequest("Invalid Device Type.");
                }

                ICollection<Device>? devices = await _dbDeviceService.GetAllByTenantIdAsync(tenant.Id);
                if (devices != null)
                {
                    Device? existingDevice = devices.FirstOrDefault(d => d.DeviceInfo!.ClientId == request.Client_Id && d.DeviceInfo!.Uuid == request.Uuid);
                    if (existingDevice != null)
                    {
                        return BadRequest("Device already registered.");
                    }
                }
                if (request.Type == DeviceType.Server)
                {
                    if (devices != null)
                    {
                        Device? server = devices.FirstOrDefault(d => d.DeviceInfo!.Type == DeviceType.Server);
                        if (server != null)
                        {
                            return BadRequest("Server already exists.");
                        }
                    }
                    if (request.ApiBaseUrl == null || request.ApiBasePort == null)
                    {
                        return BadRequest("Invalid API Base URL or Port.");
                    }
                    tenant.ApiBaseUrl = request.ApiBaseUrl;
                    tenant.ApiBasePort = request.ApiBasePort;
                    tenant = await _dbTenantService.UpdateAsync(tenant);
                    if (tenant == null)
                    {
                        return BadRequest("An error occurred while updating the tenant.");
                    }
                }
                else
                {
                    if (devices != null)
                    {
                        Device? server = devices.FirstOrDefault(d => d.DeviceInfo!.Type == DeviceType.Server);
                        if (server != null)
                        {
                            if (!server.IsVerified)
                            {
                                return BadRequest("Server not verified.");
                            }
                        }
                    }
                }
                Device? device = new Device
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
                        Type = request.Type,
                    }
                };
                
                device = await _dbDeviceService.CreateAsync(device);
                if (device != null && tenant != null)
                {
                    if (device.DeviceInfo != null)
                    {
                        if (device.DeviceInfo.MACAddresses != null && device.DeviceInfo.Type != null)
                        {
                            installationKey.IsActive = false;
                            installationKey = await _dbInstallationKeyService.UpdateAsync(installationKey);

                            if (installationKey != null)
                            {
                                DeviceRegistrationResponse response = new DeviceRegistrationResponse
                                {
                                    Name = device.DeviceInfo.Name,
                                    Client_Id = device.DeviceInfo.ClientId,
                                    Uuid = device.DeviceInfo.Uuid,
                                    IpAddress = device.DeviceInfo.IpAddress,
                                    Port = device.DeviceInfo.Port,
                                    Type = EnumUtilities.EnumToString(device.DeviceInfo.Type.Value),
                                    ApiBaseUrl = tenant.ApiBaseUrl,
                                    ApiBasePort = tenant.ApiBasePort,
                                    MACAddresses = new List<MACAddressResponse>(device.DeviceInfo.MACAddresses.Select(m => new MACAddressResponse
                                    {
                                        Address = m.Address
                                    })),
                                    IsVerified = device.IsVerified
                                };
                                return Ok(response);
                            }
                        }
                    }
                }
                return BadRequest("An error occurred while registering the device.");
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

                Device? device = await _dbDeviceService.GetByClientIdAndUuidAsync(tenant.Id, request.Client_Id, request.Uuid);
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

                InstallationKey? installationKey = await _dbInstallationKeyService.GetByInstallationKeyAsync(request.InstallationKey);
                if (installationKey == null)
                {
                    return Unauthorized("Invalid Installation Key.");
                }

                device.IsVerified = true;
                Device? updatedDevice = await _dbDeviceService.UpdateAsync(device);

                if (updatedDevice != null) 
                {
                    VerifyInstallationResponse response = new VerifyInstallationResponse
                    {
                        Name = updatedDevice.DeviceInfo?.Name,
                        IsVerified = updatedDevice.IsVerified
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

                if (request.NexumTag == null)
                {
                    return BadRequest("Invalid Nexum Tag.");
                }

                if (request.NexumServerVersion == null)
                {
                    return BadRequest("Invalid Nexum Server Version.");
                }

                if (request.NexumServerTag == null)
                {
                    return BadRequest("Invalid Nexum Server Tag.");
                }

                if (request.NexumServiceVersion == null)
                {
                    return BadRequest("Invalid Nexum Service Version.");
                }

                if (request.NexumServiceTag == null)
                {
                    return BadRequest("Invalid Nexum Service Tag.");
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
                    NexumUpdateAvailable = (nexumFile.Version != request.NexumVersion) || (nexumFile.Tag != request.NexumTag),
                    NexumServerUpdateAvailable = (nexumServerFile.Version != request.NexumServerVersion) || (nexumServerFile.Tag != request.NexumServerTag),
                    NexumServiceUpdateAvailable = (nexumServiceFile.Version != request.NexumServiceVersion) || (nexumServiceFile.Tag != request.NexumServiceTag)
                };

                return Ok(response);
            }
            return Unauthorized("Invalid API Key.");
        }

        [HttpPut("Update")]
        public async Task<IActionResult> UpdateAsync([FromHeader] string apikey, [FromBody] UpdateDeviceRequest request)
        {
            if(await _dbSecurityService.ValidateAPIKey(apikey))
            {
                Tenant? tenant = await _dbTenantService.GetByApiKeyAsync(apikey);
                if (tenant == null)
                {
                    return NotFound("Tenant not found.");
                }

                Device? device = await _dbDeviceService.GetByClientIdAndUuidAsync(tenant.Id, request.Client_Id, request.Uuid);
                if (device == null)
                {
                    return NotFound("Device not found.");
                }

                if (device.TenantId != tenant.Id)
                {
                    return Unauthorized("Invalid Device.");
                }

                if(device.DeviceInfo != null)
                {
                    device.DeviceInfo.Name = request.Name;
                    device.DeviceInfo.IpAddress = request.IpAddress;
                    device.DeviceInfo.Port = request.Port;
                    device.DeviceInfo.MACAddresses = request.MACAddresses;
                }

                device = await _dbDeviceService.UpdateAsync(device);
                if (device != null)
                {
                    if (device.DeviceInfo != null)
                    {
                        if (device.DeviceInfo.MACAddresses != null && device.DeviceInfo.Type != null)
                        {
                            UpdateDeviceResponse response = new UpdateDeviceResponse
                            {
                                Name = device.DeviceInfo.Name,
                                IpAddress = device.DeviceInfo.IpAddress,
                                Port = device.DeviceInfo.Port,
                                Type = EnumUtilities.EnumToString(device.DeviceInfo.Type.Value),
                                MACAddresses = new List<MACAddressResponse>(device.DeviceInfo.MACAddresses.Select(m => new MACAddressResponse
                                {
                                    Address = m.Address
                                }))
                            };
                            return Ok(response);
                        }
                    }
                }
                return BadRequest("An error occurred while updating the client.");
            }
            return Unauthorized("Invalid API Key.");
        }

        [HttpPut("Update-Status")]
        public async Task<IActionResult> UpdateStatusAsync([FromHeader] string apikey, [FromBody] UpdateDeviceStatusRequest request)
        {
            if (await _dbSecurityService.ValidateAPIKey(apikey))
            {
                Tenant? tenant = await _dbTenantService.GetByApiKeyAsync(apikey);
                if (tenant == null)
                {
                    return NotFound("Tenant not found.");
                }

                Device? device = await _dbDeviceService.GetByClientIdAndUuidAsync(tenant.Id, request.Client_Id, request.Uuid);
                if (device == null)
                {
                    return NotFound("Device not found.");
                }

                if (device.TenantId != tenant.Id)
                {
                    return Unauthorized("Invalid Device.");
                }

                if (!Enum.IsDefined(typeof(DeviceStatus), request.Status))
                {
                    return BadRequest("Invalid Device Status.");
                }

                device.Status = request.Status;

                device = await _dbDeviceService.UpdateAsync(device);

                if (device != null)
                {
                    UpdateDeviceStatusResponse response = new UpdateDeviceStatusResponse
                    {
                        Name = device.DeviceInfo?.Name,
                        Status = EnumUtilities.EnumToString(device.Status.Value)
                    };
                    return Ok(response);
                }
                return BadRequest("An error occurred while updating the device status.");
            }
            return Unauthorized("Invalid API Key.");
        }

        [HttpPost("Alert")]
        public async Task<IActionResult> AlertAsync([FromHeader] string apikey, [FromBody] CreateAlertRequest request)
        {
            if (await _dbSecurityService.ValidateAPIKey(apikey))
            {
                Tenant? tenant = await _dbTenantService.GetByApiKeyAsync(apikey);
                if (tenant == null)
                {
                    return NotFound("Tenant not found.");
                }

                Device? device = await _dbDeviceService.GetByClientIdAndUuidAsync(tenant.Id, request.Client_Id, request.Uuid);
                if (device == null)
                {
                    return NotFound("Device not found.");
                }

                if (device.TenantId != tenant.Id)
                {
                    return Unauthorized("Invalid Device.");
                }

                if (!Enum.IsDefined(typeof(AlertSeverity), request.Severity))
                {
                    return BadRequest("Invalid Alert Type.");
                }

                DeviceAlert? alert = new DeviceAlert
                {
                    Severity = request.Severity,
                    Message = request.Message,
                    Time = request.Time,
                    DeviceId = device.Id
                };

                alert = await _dbAlertService.CreateAsync(alert);
                if (alert != null)
                {
                    CreateAlertResponse response = new CreateAlertResponse
                    {
                        Name = device?.DeviceInfo?.Name,
                        Severity = EnumUtilities.EnumToString(alert.Severity),
                        Message = alert.Message,
                        Time = alert.Time
                    };
                    return Ok(response);
                }
                return BadRequest("An error occurred while creating the alert.");
            }
            return Unauthorized("Invalid API Key.");
        }

        [HttpPost("Log")]
        public async Task<IActionResult> LogAsync([FromHeader] string apikey, [FromBody] CreateLogRequest request)
        {
            if (await _dbSecurityService.ValidateAPIKey(apikey))
            {
                Tenant? tenant = await _dbTenantService.GetByApiKeyAsync(apikey);
                if (tenant == null)
                {
                    return NotFound("Tenant not found.");
                }

                Device? device = await _dbDeviceService.GetByClientIdAndUuidAsync(tenant.Id, request.Client_Id, request.Uuid);
                if (device == null)
                {
                    return NotFound("Device not found.");
                }

                if (device.TenantId != tenant.Id)
                {
                    return Unauthorized("Invalid Device.");
                }

                if (!Enum.IsDefined(typeof(LogType), request.Type))
                {
                    return BadRequest("Invalid Log Type.");
                }

                DeviceLog? log = new DeviceLog
                {
                    Filename = request.Filename,
                    Function = request.Function,
                    Message = request.Message,
                    Code = request.Code,
                    Stack_Trace = request.Stack_Trace,
                    Time = request.Time,
                    Type = request.Type,
                    DeviceId = device.Id
                };

                log = await _dbLogService.CreateAsync(log);
                if (log != null)
                {
                    CreateLogResponse response = new CreateLogResponse
                    {
                        Name = device?.DeviceInfo?.Name,
                        Filename = request.Filename,
                        Function = request.Function,
                        Message = log.Message,
                        Code = log.Code,
                        Stack_Trace = log.Stack_Trace,
                        Time = log.Time,
                        Type = EnumUtilities.EnumToString(log.Type)
                    };
                    return Ok(response);
                }
                return BadRequest("An error occurred while creating the log.");
            }
            return Unauthorized("Invalid API Key.");
        }

    }
}

