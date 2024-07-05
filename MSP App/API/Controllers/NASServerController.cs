using API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SharedComponents.Entities;
using SharedComponents.WebRequestEntities.NASServerRequests;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "v1-Web")]
    public class NASServerController : ControllerBase
    {
        private readonly DbNASServerService _dbNASServerService;

        public NASServerController(DbNASServerService dbNASServerService)
        {
            _dbNASServerService = dbNASServerService;
        }

        [HttpPost("")]
        public async Task<IActionResult> CreateAsync([FromBody] NASServerCreateRequest request)
        {
            if (ModelState.IsValid)
            {
                NASServer? nasServer = new NASServer
                {
                    Name = request.Name,
                    //NASUsername = request.NASUsername,
                    //NASPassword = request.NASPassword,
                    Path = request.Path,
                    BackupServerId = request.BackupServerId,
                    TenantId = request.TenantId
                };
                nasServer = await _dbNASServerService.CreateAsync(nasServer);
                if (nasServer != null)
                {
                    return Ok(nasServer);
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
                nasServer.Name = request.Name;
                //nasServer.NASUsername = request.NASUsername;
                //nasServer.NASPassword = request.NASPassword;
                nasServer.Path = request.Path;
                nasServer.BackupServerId = request.BackupServerId;
                nasServer = await _dbNASServerService.UpdateAsync(nasServer);
                if (nasServer != null)
                {
                    return Ok(nasServer);
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
                if (await _dbNASServerService.DeleteAsync(id))
                {
                    return Ok("NAS Server deleted.");
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
    }
}
