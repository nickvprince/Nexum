using Microsoft.AspNetCore.Mvc;
using SharedComponents.Entities.DbEntities;
using SharedComponents.Entities.WebEntities.Requests.DeviceRequests;
using SharedComponents.Handlers.Attributes.HasPermission;
using SharedComponents.Handlers.Results;
using SharedComponents.Services.APIServices.Interfaces;
using SharedComponents.Services.DbServices.Interfaces;
using SharedComponents.Services.TenantServerAPIServices.Interfaces;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "v1-Web")]
    public class DeviceController : ControllerBase
    {
        private readonly IDbDeviceService _dbDeviceService;
        private readonly IDbTenantService _dbTenantService;
        private readonly IDbInstallationKeyService _dbInstallationKeyService;
        private readonly ITenantServerAPIDeviceService _httpDeviceService;
        private readonly IAPIAuthService _authService;

        public DeviceController(IDbDeviceService dbDeviceService, IDbTenantService dbTenantService,
            IDbInstallationKeyService dbInstallationKeyService, ITenantServerAPIDeviceService httpDeviceService,
            IAPIAuthService authService)
        {
            _dbDeviceService = dbDeviceService;
            _dbTenantService = dbTenantService;
            _dbInstallationKeyService = dbInstallationKeyService;
            _httpDeviceService = httpDeviceService;
            _authService = authService;
        }

        [HttpPost("")]
        [HasPermission("Device.Create.Permission", PermissionType.Tenant)]
        public async Task<IActionResult> CreateAsync([FromBody] DeviceCreateRequest request)
        {
            if (ModelState.IsValid)
            {
                // Authentication check using roles + permissions
                if (!await _authService.UserHasPermissionAsync<DeviceController>(Request.Headers["Authorization"].ToString(), request.TenantId))
                {
                    return new CustomForbidResult("User do not have access to this feature for the specified tenant");
                }
                // --- End of authentication check ---
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

                if(installationKey.Type != InstallationKeyType.Server && installationKey.Type != InstallationKeyType.Device)
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
        [HasPermission("Device.Update.Permission", PermissionType.Tenant)]
        public async Task<IActionResult> UpdateAsync([FromBody] DeviceUpdateRequest request)
        {
            if (ModelState.IsValid)
            {
                Device? device = await _dbDeviceService.GetAsync(request.Id);
                if (device == null)
                {
                    return NotFound("Device not found.");
                }
                // Authentication check using roles + permissions
                if (!await _authService.UserHasPermissionAsync<DeviceController>(Request.Headers["Authorization"].ToString(), device.TenantId))
                {
                    return new CustomForbidResult("User do not have access to this feature for the specified tenant");
                }
                // --- End of authentication check ---
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
                            if(server.Id != request.Id)
                            {
                                return BadRequest("Server already registered.");
                            }
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
        [HasPermission("Device.Update-Status.Permission", PermissionType.Tenant)]
        public async Task<IActionResult> UpdateStatusAsync([FromBody] DeviceUpdateStatusRequest request)
        {
            if (ModelState.IsValid)
            {
                Device? device = await _dbDeviceService.GetAsync(request.Id);
                if (device == null)
                {
                    return NotFound("Device not found.");
                }
                // Authentication check using roles + permissions
                if (!await _authService.UserHasPermissionAsync<DeviceController>(Request.Headers["Authorization"].ToString(), device.TenantId))
                {
                    return new CustomForbidResult("User do not have access to this feature for the specified tenant");
                }
                // --- End of authentication check ---
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
        [HasPermission("Device.Delete.Permission", PermissionType.Tenant)]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            if (ModelState.IsValid)
            {
                Device? device = await _dbDeviceService.GetAsync(id);
                if (device == null)
                {
                    return NotFound("Device not found.");
                }
                // Authentication check using roles + permissions
                if (!await _authService.UserHasPermissionAsync<DeviceController>(Request.Headers["Authorization"].ToString(), device.TenantId))
                {
                    return new CustomForbidResult("User do not have access to this feature for the specified tenant");
                }
                // --- End of authentication check ---
                if (await _dbDeviceService.DeleteAsync(id))
                {
                    return Ok($"Device deleted successfully.");
                }
                return NotFound("Device not found.");
            }
            return BadRequest("Invalid Request.");
        }

        [HttpGet("{id}")]
        [HasPermission("Device.Get.Permission", PermissionType.Tenant)]
        public async Task<IActionResult> GetAsync(int id)
        {
            if (ModelState.IsValid)
            {
                Device? device = await _dbDeviceService.GetAsync(id);
                if (device != null)
                {
                    // Authentication check using roles + permissions
                    if (!await _authService.UserHasPermissionAsync<DeviceController>(Request.Headers["Authorization"].ToString(), device.TenantId))
                    {
                        return new CustomForbidResult("User do not have access to this feature for the specified tenant");
                    }
                    // --- End of authentication check ---
                    return Ok(device);
                }
                return NotFound("Device not found.");
            }
            return BadRequest("Invalid Request.");
        }

        [HttpGet("")]
        [HasPermission("Device.Get-All.Permission", PermissionType.Tenant)]
        public async Task<IActionResult> GetAllAsync()
        {
            if (ModelState.IsValid)
            {
                var tenantIds = await _authService.GetUserAccessibleTenantsAsync(Request.Headers["Authorization"].ToString());
                if (tenantIds == null)
                {
                    return new CustomForbidResult("User does not have any tenant permissions");
                }
                List<Device> devices = new List<Device>();
                foreach (var tenantId in tenantIds)
                {
                    if (tenantId != null)
                    {
                        var tenantDevices = await _dbDeviceService.GetAllByTenantIdAsync((int)tenantId);
                        if (tenantDevices != null)
                        {
                            devices.AddRange(tenantDevices);
                        }
                    }
                }
                if (devices != null)
                {
                    if (devices.Any())
                    {
                        return Ok(devices.Distinct());
                    }
                }
                return NotFound("No devices found.");
            }
            return BadRequest("Invalid Request.");
        }

        [HttpGet("By-Tenant/{tenantId}")]
        [HasPermission("Device.Get-By-Tenant.Permission", PermissionType.Tenant)]
        public async Task<IActionResult> GetAllByTenantIdAsync(int tenantId)
        {
            if (ModelState.IsValid)
            {
                // Authentication check using roles + permissions
                if (!await _authService.UserHasPermissionAsync<DeviceController>(Request.Headers["Authorization"].ToString(), tenantId))
                {
                    return new CustomForbidResult("User do not have access to this feature for the specified tenant");
                }
                // --- End of authentication check ---
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
        [HasPermission("Device.Refresh.Permission", PermissionType.Tenant)]
        public async Task<IActionResult> RefreshAsync(int id)
        {
            if (ModelState.IsValid)
            {
                Device? device = await _dbDeviceService.GetAsync(id);
                if (device == null)
                {
                    return NotFound("Device not found.");
                }
                // Authentication check using roles + permissions
                if (!await _authService.UserHasPermissionAsync<DeviceController>(Request.Headers["Authorization"].ToString(), device.TenantId))
                {
                    return new CustomForbidResult("User do not have access to this feature for the specified tenant");
                }
                // --- End of authentication check ---
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
                            return BadRequest("An error occurred while updating the device status.");
                        }
                        return BadRequest("An error occurred while retrieving the device status from the tenant server.");
                    }
                }
                return BadRequest("An error occurred while refreshing the device on the tenant server.");
            }
            return BadRequest("Invalid Request.");
        }
    }
}
