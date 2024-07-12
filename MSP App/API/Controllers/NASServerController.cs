using API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SharedComponents.Entities;
using SharedComponents.RequestEntities.HTTP;
using SharedComponents.ResponseEntities.HTTP;
using SharedComponents.Utilities;
using SharedComponents.WebEntities.Requests.NASServerRequests;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "v1-Web")]
    public class NASServerController : ControllerBase
    {
        private readonly DbNASServerService _dbNASServerService;
        private readonly DbJobService _dbJobService;
        private readonly DbTenantService _dbTenantService;
        private readonly HTTPNASServerService _httpNASServerService;

        public NASServerController(DbNASServerService dbNASServerService, DbJobService dbJobService, DbTenantService dbTenantService, HTTPNASServerService httpNASServerService)
        {
            _dbNASServerService = dbNASServerService;
            _dbJobService = dbJobService;
            _dbTenantService = dbTenantService;
            _httpNASServerService = httpNASServerService;
        }

        [HttpPost("")]
        public async Task<IActionResult> CreateAsync([FromBody] NASServerCreateRequest request)
        {
            if (ModelState.IsValid)
            {
                Tenant? tenant = await _dbTenantService.GetAsync(request.TenantId);
                if (tenant == null)
                {
                    return NotFound("Tenant not found.");
                }
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
                        NASUsername = request.NASUsername,
                        NASPassword = SecurityUtilities.Encrypt(SecurityUtilities.Shuffle(tenant.ApiKey, tenant.ApiKeyServer),request.NASPassword)
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
                    return BadRequest("An error occurred while creating the NAS Server on the tenant server.");
                }
                return BadRequest("An error occurred while creating the NAS Server.");
            }
            return BadRequest("Invalid request.");
        }

        [HttpPut("")]
        public async Task<IActionResult> UpdateAsync([FromBody] NASServerUpdateRequest request)
        {
            if (ModelState.IsValid)
            {
                NASServer? nasServer = await _dbNASServerService.GetAsync(request.Id);
                if (nasServer == null)
                {
                    return NotFound("NAS Server not found.");
                }
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
                        NASUsername = request.NASUsername,
                        NASPassword = SecurityUtilities.Encrypt(SecurityUtilities.Shuffle(tenant.ApiKey, tenant.ApiKeyServer), request.NASPassword)
                    };
                    bool? serverResponse = await _httpNASServerService.UpdateAsync(nasServer.TenantId, serverRequest);
                    if (serverResponse == true)
                    {
                        return Ok(nasServer);
                    }
                    
                    return BadRequest("An error occurred while updating the NAS Server on the tenant server.");
                }
                return BadRequest("An error occurred while updating the NAS Server.");
            }
            return BadRequest("Invalid request.");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            if (ModelState.IsValid)
            {
                NASServer? nasServer = await _dbNASServerService.GetAsync(id);
                if (nasServer == null)
                {
                    return NotFound("NAS Server not found.");
                }
                ICollection<DeviceJob>? jobs = await _dbJobService.GetAllByBackupServerIdAsync(nasServer.TenantId, nasServer.BackupServerId);
                if (jobs != null) 
                {
                    foreach (DeviceJob job in jobs)
                    {
                        if (job.Status == DeviceJobStatus.InProgress || job.Status == DeviceJobStatus.Restarting)
                        {
                            return BadRequest("Cannot delete NAS Server while jobs are running.");
                        }
                    }
                }
                if (await _dbNASServerService.DeleteAsync(id))
                {
                    DeleteNASServerRequest serverRequest = new DeleteNASServerRequest
                    {
                        Id = nasServer.BackupServerId
                    };
                    bool? serverResponse = await _httpNASServerService.DeleteAsync(nasServer.TenantId, serverRequest);
                    if (serverResponse == true)
                    {
                        return Ok("NAS Server deleted.");
                    }
                    return BadRequest("An error occurred while deleting the NAS Server on the tenant server.");
                }
                return BadRequest("An error occurred while deleting the NAS Server.");
            }
            return BadRequest("Invalid request.");
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetAsync(int id)
        {
            if (ModelState.IsValid)
            {
                NASServer? nasServer = await _dbNASServerService.GetAsync(id);
                if (nasServer != null)
                {
                    return Ok(nasServer);
                }
                return NotFound("NAS Server not found.");
            }
            return BadRequest("Invalid request.");
        }

        [HttpGet("")]
        public async Task<IActionResult> GetAllAsync()
        {
            if (ModelState.IsValid)
            {
                ICollection<NASServer>? nasServers = await _dbNASServerService.GetAllAsync();
                if (nasServers != null)
                {
                    if (nasServers.Any())
                    {
                        return Ok(nasServers);
                    }
                }
                return NotFound("No NAS Servers found.");
            }
            return BadRequest("Invalid request.");
        }

        [HttpGet("By-Tenant/{tenantId}")]
        public async Task<IActionResult> GetAllByTenantIdAsync(int tenantId)
        {
            if (ModelState.IsValid)
            {
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

        [HttpGet("By-Device/{deviceId}")]
        public async Task<IActionResult> GetAllByDeviceIdAsync(int deviceId)
        {
            if (ModelState.IsValid)
            {
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
    }
}
