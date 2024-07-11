using API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SharedComponents.DbServices;
using SharedComponents.Entities;
using SharedComponents.ResponseEntities;
using SharedComponents.WebEntities.Requests.DeviceRequests;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "v1-Web")]
    public class DeviceController : ControllerBase
    {
        private readonly DbDeviceService _dbDeviceService;
        private readonly DbTenantService _dbTenantService;
        private readonly DbInstallationKeyService _dbInstallationKeyService;
        private readonly HTTPDeviceService _httpDeviceService;

        public DeviceController(DbDeviceService dbDeviceService, DbTenantService dbTenantService,
            DbInstallationKeyService dbInstallationKeyService, HTTPDeviceService httpDeviceService)
        {
            _dbDeviceService = dbDeviceService;
            _dbTenantService = dbTenantService;
            _dbInstallationKeyService = dbInstallationKeyService;
            _httpDeviceService = httpDeviceService;
        }

        [HttpPost("")]
        public async Task<IActionResult> CreateAsync([FromBody] DeviceCreateRequest request)
        {
            if (ModelState.IsValid)
            {
                Tenant? tenant = await _dbTenantService.GetAsync(request.TenantId);
                if (tenant == null)
                {
                    return NotFound("Tenant not found.");
                }
                Device? device = await _dbDeviceService.GetByClientIdAndUuidAsync(request.TenantId, request.ClientId, request.Uuid);
                if (device != null)
                {
                    return BadRequest("Device already exists.");
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

                if(installationKey.Type != InstallationKeyType.Server || installationKey.Type != InstallationKeyType.Device)
                {
                    return Unauthorized("Invalid Installation Key Type.");
                }

                ICollection<Device>? devices = await _dbDeviceService.GetAllByTenantIdAsync(request.TenantId);
                if (request.Type == DeviceType.Server)
                {
                    if(installationKey.Type != InstallationKeyType.Server)
                    {
                        return Unauthorized("Invalid Installation Key Type.");
                    }
                    if (devices != null)
                    {
                        Device? server = devices.FirstOrDefault(d => d.DeviceInfo!.Type == DeviceType.Server);
                        if (server != null)
                        {
                            return BadRequest("Server already registered.");
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
                    if (installationKey.Type != InstallationKeyType.Device)
                    {
                        return Unauthorized("Invalid Installation Key Type.");
                    }
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
                device = new Device
                {
                    TenantId = request.TenantId,
                    DeviceInfo = new DeviceInfo
                    {
                        Name = request.Name,
                        ClientId = request.ClientId,
                        Uuid = request.Uuid,
                        IpAddress = request.IpAddress,
                        Port = request.Port,
                        Type = request.Type,
                        MACAddresses = new List<MACAddress>(request.MACAddresses!.Select(m => new MACAddress
                        {
                            Address = m
                        }))
                    }
                };
                device = await _dbDeviceService.CreateAsync(device);
                if (device != null)
                {
                    return Ok(device);
                }
                return BadRequest("An error occurred while creating the device.");
            }
            return BadRequest("Invalid Request.");
        }

        [HttpPut("")]
        public async Task<IActionResult> UpdateAsync([FromBody] DeviceUpdateRequest request)
        {
            if (ModelState.IsValid)
            {
                Device? device = await _dbDeviceService.GetAsync(request.Id);
                if (device == null)
                {
                    return NotFound("Device not found.");
                }
                if (device.DeviceInfo == null)
                {
                    return NotFound("DeviceInfo not found.");
                }
                ICollection<Device>? devices = await _dbDeviceService.GetAllByTenantIdAsync(device.TenantId);
                if (request.Type == DeviceType.Server)
                {
                    if (devices != null)
                    {
                        Device? server = devices.FirstOrDefault(d => d.DeviceInfo!.Type == DeviceType.Server);
                        if (server != null)
                        {
                            return BadRequest("Server already registered.");
                        }
                    }
                }
                if (device.DeviceInfo.Type == DeviceType.Server)
                {
                    if (request.Type != DeviceType.Server)
                    {
                        return BadRequest("Server cannot be changed to another type.");
                    }
                    Tenant? tenant = await _dbTenantService.GetAsync(device.TenantId);
                    if (tenant == null)
                    {
                        return NotFound("Tenant not found.");
                    }
                    tenant.ApiBaseUrl = request.ApiBaseUrl;
                    tenant.ApiBasePort = request.ApiBasePort;
                    tenant = await _dbTenantService.UpdateAsync(tenant);
                    if (tenant == null)
                    {
                        return BadRequest("An error occurred while updating the tenant.");
                    }
                }
                device.DeviceInfo.Nickname = request.Nickname;
                device.DeviceInfo.Type = request.Type;
                device.DeviceInfo.Name = request.Name;
                device.DeviceInfo.IpAddress = request.IpAddress;
                device.DeviceInfo.Port = request.Port;
                device.DeviceInfo.MACAddresses = new List<MACAddress>(request.MACAddresses!.Select(m => new MACAddress
                {
                    Address = m
                }));
                device = await _dbDeviceService.UpdateAsync(device);
                if (device != null)
                {
                    return Ok(device);
                }
                return BadRequest("An error occurred while updating the device.");
            }
            return BadRequest("Invalid Request.");
        }

        [HttpPut("Status")]
        public async Task<IActionResult> UpdateStatusAsync([FromBody] DeviceUpdateStatusRequest request)
        {
            if (ModelState.IsValid)
            {
                Device? device = await _dbDeviceService.GetAsync(request.Id);
                if (device == null)
                {
                    return NotFound("Device not found.");
                }
                if (device.DeviceInfo == null)
                {
                    return NotFound("DeviceInfo not found.");
                }
                device.Status = request.Status;
                device = await _dbDeviceService.UpdateAsync(device);
                if (device != null)
                {
                    return Ok(device);
                }
                return BadRequest("An error occurred while updating the device status.");
            }
            return BadRequest("Invalid Request.");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            if (ModelState.IsValid)
            {
                if (await _dbDeviceService.DeleteAsync(id))
                {
                    return Ok($"Device deleted successfully.");
                }
                return NotFound("Device not found.");
            }
            return BadRequest("Invalid Request.");
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetAsync(int id)
        {
            if (ModelState.IsValid)
            {
                Device? device = await _dbDeviceService.GetAsync(id);
                if (device != null)
                {
                    return Ok(device);
                }
                return NotFound("Device not found.");
            }
            return BadRequest("Invalid Request.");
        }

        [HttpGet("")]
        public async Task<IActionResult> GetAllAsync()
        {
            if (ModelState.IsValid)
            {
                ICollection<Device>? devices = await _dbDeviceService.GetAllAsync();
                if (devices != null)
                {
                    return Ok(devices);
                }
                return NotFound("No devices found.");
            }
            return BadRequest("Invalid Request.");
        }

        [HttpGet("By-Tenant/{tenantId}")]
        public async Task<IActionResult> GetAllByTenantIdAsync(int tenantId)
        {
            if (ModelState.IsValid)
            {
                ICollection<Device>? devices = await _dbDeviceService.GetAllByTenantIdAsync(tenantId);
                if (devices != null)
                {
                    if (devices.Any())
                    {
                        return Ok(devices);
                    }
                }
                return NotFound("No devices found for the specified tenant.");
            }
            return BadRequest("Invalid Request.");
        }

        [HttpPost("{id}/Refresh")]
        public async Task<IActionResult> RefreshAsync(int id)
        {
            if (ModelState.IsValid)
            {
                Device? device = await _dbDeviceService.GetAsync(id);
                if (device == null)
                {
                    return NotFound("Device not found.");
                }
                if (device.DeviceInfo == null)
                {
                    return NotFound("DeviceInfo not found.");
                }
                // force checkin here
                bool? checkin = await _httpDeviceService.ForceDeviceCheckinAsync(device.TenantId, device.DeviceInfo.ClientId);
                if (checkin != null)
                {
                    if (checkin == true)
                    {
                        DeviceStatus? status = await _httpDeviceService.GetDeviceStatusAsync(device.TenantId, device.DeviceInfo.ClientId);
                        if (status != null)
                        {
                            device.Status = status;
                            device = await _dbDeviceService.UpdateAsync(device);
                            if (device != null)
                            {
                                return Ok(device);
                            }
                        }
                        return Ok("Device refresh request sent successfully.");
                    }
                }
                return BadRequest("An error occurred while refreshing the device on the tenant server.");
            }
            return BadRequest("Invalid Request.");
        }
    }
}
