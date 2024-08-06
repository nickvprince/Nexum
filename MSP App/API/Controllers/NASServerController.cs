using Microsoft.AspNetCore.Mvc;
using SharedComponents.Entities.DbEntities;
using SharedComponents.Entities.WebEntities.Requests.NASServerRequests;
using SharedComponents.Handlers.Attributes.HasPermission;
using SharedComponents.Services.DbServices.Interfaces;
using SharedComponents.Utilities;
using SharedComponents.Handlers.Results;
using SharedComponents.Entities.TenantServerHttpEntities.Requests;
using SharedComponents.Entities.TenantServerHttpEntities.Responses;
using SharedComponents.Services.APIServices.Interfaces;
using SharedComponents.Services.TenantServerAPIServices.Interfaces;
using Azure.Core;

namespace API.Controllers
{
    /// <summary>
    /// Controller for managing NAS servers.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "v1-Web")]
    public class NASServerController : ControllerBase
    {
        private readonly IDbNASServerService _dbNASServerService;
        private readonly IDbJobService _dbJobService;
        private readonly IDbTenantService _dbTenantService;
        private readonly IDbDeviceService _dbDeviceService;
        private readonly ITenantServerAPINASServerService _httpNASServerService;
        private readonly ITenantServerAPIDeviceService _httpDeviceService;
        private readonly IAPIAuthService _authService;

        /// <summary>
        /// Initializes a new instance of the <see cref="NASServerController"/> class.
        /// </summary>
        /// <param name="dbNASServerService">The NAS server service.</param>
        /// <param name="dbJobService">The job service.</param>
        /// <param name="dbTenantService">The tenant service.</param>
        /// <param name="dbDeviceService">The device service.</param>
        /// <param name="httpNASServerService">The HTTP NAS server service.</param>
        /// <param name="authService">The authentication service.</param>
        public NASServerController(IDbNASServerService dbNASServerService, IDbJobService dbJobService,
            IDbTenantService dbTenantService, IDbDeviceService dbDeviceService,
            ITenantServerAPINASServerService httpNASServerService, ITenantServerAPIDeviceService httpDeviceService,
            IAPIAuthService authService)
        {
            _dbNASServerService = dbNASServerService;
            _dbJobService = dbJobService;
            _dbTenantService = dbTenantService;
            _dbDeviceService = dbDeviceService;
            _httpNASServerService = httpNASServerService;
            _httpDeviceService = httpDeviceService;
            _authService = authService;
        }

        /// <summary>
        /// Creates a new NAS server.
        /// </summary>
        /// <param name="request">The NAS server create request.</param>
        /// <returns>An action result containing the created NAS server.</returns>
        [HttpPost("")]
        [HasPermission("NASServer.Create.Permission", PermissionType.Tenant)]
        public async Task<IActionResult> CreateAsync([FromBody] NASServerCreateRequest request)
        {
            if (ModelState.IsValid)
            {
                Tenant? tenant = await _dbTenantService.GetAsync(request.TenantId);
                if (tenant == null)
                {
                    return NotFound("Tenant not found.");
                }
                // Authentication check using roles + permissions
                if (!await _authService.UserHasPermissionAsync<NASServerController>(Request.Headers["Authorization"].ToString(), tenant.Id))
                {
                    return new CustomForbidResult("User do not have access to this feature for the specified tenant");
                }
                // --- End of authentication check ---
                NASServer? nasServer = new NASServer
                {
                    Name = request.Name,
                    Path = request.Path,
                    TenantId = tenant.Id
                };
                nasServer = await _dbNASServerService.CreateAsync(nasServer);
                if (nasServer != null)
                {
                    CreateNASServerRequest serverRequest = new CreateNASServerRequest
                    {
                        Name = nasServer.Name,
                        Path = nasServer.Path,
                        Username = request.NASUsername,
                        Password = SecurityUtilities.Encrypt(SecurityUtilities.Shuffle(tenant.ApiKey, tenant.ApiKeyServer),request.NASPassword)
                    };
                    CreateNASServerResponse? serverResponse = await _httpNASServerService.CreateAsync(tenant.Id, serverRequest);
                    if (serverResponse != null)
                    {
                        nasServer.BackupServerId = serverResponse.Id;
                        nasServer = await _dbNASServerService.UpdateAsync(nasServer);
                        if (nasServer != null)
                        {
                            return Ok(nasServer);
                        }
                        return BadRequest("An error occurred while updating the NAS Server.");
                    }
                    if (!await _dbNASServerService.DeleteAsync(nasServer.Id))
                    {
                        Console.WriteLine("An error occurred while deleting the NAS Server.");
                    }
                    tenant.Devices = await _dbDeviceService.GetAllByTenantIdAsync(tenant.Id);
                    if (tenant.Devices != null)
                    {
                        Device? server = tenant.Devices.Where(d => d.DeviceInfo.Type == DeviceType.Server).FirstOrDefault();
                        if (server != null)
                        {
                            if ((bool)!await _httpDeviceService.ForceDeviceCheckinAsync(request.TenantId, server.DeviceInfo.ClientId))
                            {
                                foreach (var device in tenant.Devices)
                                {
                                    device.Status = DeviceStatus.Offline;
                                    await _dbDeviceService.UpdateAsync(device);
                                }
                            }
                        }
                    }
                    return BadRequest("An error occurred while creating the NAS Server on the tenant server.");
                }
                return BadRequest("An error occurred while creating the NAS Server.");
            }
            return BadRequest("Invalid request.");
        }

        /// <summary>
        /// Updates an existing NAS server.
        /// </summary>
        /// <param name="request">The NAS server update request.</param>
        /// <returns>An action result containing the updated NAS server.</returns>
        [HttpPut("")]
        [HasPermission("NASServer.Update.Permission", PermissionType.Tenant)]
        public async Task<IActionResult> UpdateAsync([FromBody] NASServerUpdateRequest request)
        {
            if (ModelState.IsValid)
            {
                NASServer? nasServer = await _dbNASServerService.GetAsync(request.Id);
                if (nasServer == null)
                {
                    return NotFound("NAS Server not found.");
                }
                // Authentication check using roles + permissions
                if (!await _authService.UserHasPermissionAsync<NASServerController>(Request.Headers["Authorization"].ToString(), nasServer.TenantId))
                {
                    return new CustomForbidResult("User do not have access to this feature for the specified tenant");
                }
                // --- End of authentication check ---
                Tenant? tenant = await _dbTenantService.GetAsync(nasServer.TenantId);
                if (tenant == null)
                {
                    return NotFound("Tenant not found.");
                }
                nasServer.Name = request.Name;
                nasServer.Path = request.Path;
                nasServer = await _dbNASServerService.UpdateAsync(nasServer);
                if (nasServer != null)
                {
                    UpdateNASServerRequest serverRequest = new UpdateNASServerRequest
                    {
                        Id = nasServer.BackupServerId,
                        Name = nasServer.Name,
                        Path = nasServer.Path,
                        Username = request.NASUsername,
                        Password = SecurityUtilities.Encrypt(SecurityUtilities.Shuffle(tenant.ApiKey, tenant.ApiKeyServer), request.NASPassword)
                    };
                    bool? serverResponse = await _httpNASServerService.UpdateAsync(nasServer.TenantId, serverRequest);
                    if (serverResponse == true)
                    {
                        return Ok(nasServer);
                    }
                    tenant.Devices = await _dbDeviceService.GetAllByTenantIdAsync(tenant.Id);
                    if (tenant.Devices != null)
                    {
                        Device? server = tenant.Devices.Where(d => d.DeviceInfo.Type == DeviceType.Server).FirstOrDefault();
                        if (server != null)
                        {
                            if ((bool)!await _httpDeviceService.ForceDeviceCheckinAsync(tenant.Id, server.DeviceInfo.ClientId))
                            {
                                foreach (var device in tenant.Devices)
                                {
                                    device.Status = DeviceStatus.Offline;
                                    await _dbDeviceService.UpdateAsync(device);
                                }
                            }
                        }
                    }
                    return BadRequest("An error occurred while updating the NAS Server on the tenant server.");
                }
                return BadRequest("An error occurred while updating the NAS Server.");
            }
            return BadRequest("Invalid request.");
        }

        /// <summary>
        /// Deletes a NAS server by ID.
        /// </summary>
        /// <param name="id">The ID of the NAS server to delete.</param>
        /// <returns>An action result indicating the outcome of the deletion.</returns>
        [HttpDelete("{id}")]
        [HasPermission("NASServer.Delete.Permission", PermissionType.Tenant)]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            if (ModelState.IsValid)
            {
                NASServer? nasServer = await _dbNASServerService.GetAsync(id);
                if (nasServer == null)
                {
                    return NotFound("NAS Server not found.");
                }
                // Authentication check using roles + permissions
                if (!await _authService.UserHasPermissionAsync<NASServerController>(Request.Headers["Authorization"].ToString(), nasServer.TenantId))
                {
                    return new CustomForbidResult("User do not have access to this feature for the specified tenant");
                }
                // --- End of authentication check ---
                ICollection<DeviceJob>? jobs = await _dbJobService.GetAllByBackupServerIdAsync(nasServer.TenantId, nasServer.BackupServerId);
                if (jobs != null) 
                {
                    foreach (DeviceJob job in jobs)
                    {
                        if (job.Status == DeviceJobStatus.InProgress || job.Status == DeviceJobStatus.Restarting || job.Status == DeviceJobStatus.Paused)
                        {
                            return BadRequest("Cannot delete NAS Server while jobs are running.");
                        }
                    }
                }
                DeleteNASServerRequest serverRequest = new DeleteNASServerRequest
                {
                    Id = nasServer.BackupServerId
                };
                bool? serverResponse = await _httpNASServerService.DeleteAsync(nasServer.TenantId, serverRequest);
                if (serverResponse == true)
                {
                    if (await _dbNASServerService.DeleteAsync(id))
                    {
                        return Ok("NAS Server deleted.");
                    }
                    ICollection<Device> devices = await _dbDeviceService.GetAllByTenantIdAsync(nasServer.Id);
                    if (devices != null)
                    {
                        Device? server = devices.Where(d => d.DeviceInfo.Type == DeviceType.Server).FirstOrDefault();
                        if (server != null)
                        {
                            if ((bool)!await _httpDeviceService.ForceDeviceCheckinAsync(nasServer.TenantId, server.DeviceInfo.ClientId))
                            {
                                foreach (var device in devices)
                                {
                                    device.Status = DeviceStatus.Offline;
                                    await _dbDeviceService.UpdateAsync(device);
                                }
                            }
                        }
                    }
                    return BadRequest("An error occurred while deleting the NAS Server.");
                }
                return BadRequest("An error occurred while deleting the NAS Server on the tenant server.");
            }
            return BadRequest("Invalid request.");
        }

        /// <summary>
        /// Gets a NAS server by ID.
        /// </summary>
        /// <param name="id">The ID of the NAS server to retrieve.</param>
        /// <returns>An action result containing the NAS server.</returns>
        [HttpGet("{id}")]
        [HasPermission("NASServer.Get.Permission", PermissionType.Tenant)]
        public async Task<IActionResult> GetAsync(int id)
        {
            if (ModelState.IsValid)
            {
                NASServer? nasServer = await _dbNASServerService.GetAsync(id);
                if (nasServer != null)
                {
                    // Authentication check using roles + permissions
                    if (!await _authService.UserHasPermissionAsync<NASServerController>(Request.Headers["Authorization"].ToString(), nasServer.TenantId))
                    {
                        return new CustomForbidResult("User do not have access to this feature for the specified tenant");
                    }
                    // --- End of authentication check ---
                    return Ok(nasServer);
                }
                return NotFound("NAS Server not found.");
            }
            return BadRequest("Invalid request.");
        }

        /// <summary>
        /// Gets all NAS servers accessible by the authenticated user.
        /// </summary>
        /// <returns>An action result containing all NAS servers accessible by the user.</returns>
        [HttpGet("")]
        [HasPermission("NASServer.Get.Permission", PermissionType.Tenant)]
        public async Task<IActionResult> GetAllAsync()
        {
            if (ModelState.IsValid)
            {
                var tenantIds = await _authService.GetUserAccessibleTenantsAsync(Request.Headers["Authorization"].ToString());
                if (tenantIds == null)
                {
                    return new CustomForbidResult("User does not have any tenant permissions");
                }
                List<NASServer>? nasServers = new List<NASServer>();
                foreach (var tenantId in tenantIds)
                {
                    if (tenantId != null)
                    {
                        var tenantNASServers = await _dbNASServerService.GetAllByTenantIdAsync((int)tenantId);
                        if (tenantNASServers != null)
                        {
                            nasServers.AddRange(tenantNASServers);
                        }
                    }
                }
                if (nasServers != null)
                {
                    if (nasServers.Any())
                    {
                        return Ok(nasServers.Distinct());
                    }
                }
                return NotFound("No NAS Servers found.");
            }
            return BadRequest("Invalid request.");
        }

        /// <summary>
        /// Gets all NAS servers for a specific device.
        /// </summary>
        /// <param name="deviceId">The ID of the device.</param>
        /// <returns>An action result containing the NAS servers for the device.</returns>
        [HttpGet("By-Device/{deviceId}")]
        [HasPermission("NASServer.Get.Permission", PermissionType.Tenant)]
        public async Task<IActionResult> GetAllByDeviceIdAsync(int deviceId)
        {
            if (ModelState.IsValid)
            {
                Device? device = await _dbDeviceService.GetAsync(deviceId);
                if (device == null)
                {
                    return NotFound("Device not found.");
                }
                // Authentication check using roles + permissions
                if (!await _authService.UserHasPermissionAsync<NASServerController>(Request.Headers["Authorization"].ToString(), device.TenantId))
                {
                    return new CustomForbidResult("User do not have access to this feature for the specified tenant");
                }
                // --- End of authentication check ---
                ICollection<NASServer>? nasServers = await _dbNASServerService.GetAllByDeviceIdAsync(deviceId);
                if (nasServers != null)
                {
                    if (nasServers.Any())
                    {
                        return Ok(nasServers);
                    }
                }
                return NotFound("No NAS Servers found for the device.");
            }
            return BadRequest("Invalid request.");
        }

        /// <summary>
        /// Gets all NAS servers for a specific tenant.
        /// </summary>
        /// <param name="tenantId">The ID of the tenant.</param>
        /// <returns>An action result containing the NAS servers for the tenant.</returns>
        [HttpGet("By-Tenant/{tenantId}")]
        [HasPermission("NASServer.Get.Permission", PermissionType.Tenant)]
        public async Task<IActionResult> GetAllByTenantIdAsync(int tenantId)
        {
            if (ModelState.IsValid)
            {
                // Authentication check using roles + permissions
                if (!await _authService.UserHasPermissionAsync<NASServerController>(Request.Headers["Authorization"].ToString(), tenantId))
                {
                    return new CustomForbidResult("User do not have access to this feature for the specified tenant");
                }
                // --- End of authentication check ---
                ICollection<NASServer>? nasServers = await _dbNASServerService.GetAllByTenantIdAsync(tenantId);
                if (nasServers != null)
                {
                    if (nasServers.Any())
                    {
                        return Ok(nasServers);
                    }
                }
                return NotFound("No NAS Servers found for the tenant.");
            }
            return BadRequest("Invalid request.");
        }
    }
}
