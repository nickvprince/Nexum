using API.Attributes.HasPermission;
using API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SharedComponents.DbServices;
using SharedComponents.Entities;
using SharedComponents.WebEntities.Requests.PermissionRequests;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "v1-Web")]
    public class PermissionController : ControllerBase
    {
        private readonly IDbPermissionService _dbPermissionService;

        public PermissionController(IDbPermissionService dbPermissionService)
        {
            _dbPermissionService = dbPermissionService;
        }
        //Functions that can be added for debugging / future purposes (working code)
        /*[HttpPost("")]
        public async Task<IActionResult> CreateAsync([FromBody] PermissionCreateRequest request)
        {
            if(ModelState.IsValid)
            {
                Permission? permission = new Permission
                {
                    Name = request.Name,
                    Description = request.Description
                };
                permission = await _dbPermissionService.CreateAsync(permission);
                if (permission != null)
                {
                    return Ok(permission);
                }
                return BadRequest("An error occurred while creating the permission.");
            }
            return BadRequest("Invalid Request.");
        }

        [HttpPut("")]
        public async Task<IActionResult> UpdateAsync([FromBody] PermissionUpdateRequest request)
        {
            if (ModelState.IsValid)
            {
                Permission? permission = await _dbPermissionService.GetAsync(request.Id);
                if (permission == null)
                {
                    return NotFound("Permission not found.");
                }
                permission.Name = request.Name;
                permission.Description = request.Description;
                permission = await _dbPermissionService.UpdateAsync(permission);
                if (permission != null)
                {
                    return Ok(permission);
                }
                return BadRequest("An error occurred while updating the permission.");
            }
            return BadRequest("Invalid Request.");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            if (ModelState.IsValid)
            {
                if (await _dbPermissionService.DeleteAsync(id))
                {
                    return Ok($"Permission deleted successfully.");
                }
                return NotFound("Permission not found.");
            }
            return BadRequest("Invalid Request.");
        }*/

        [HttpGet("{id}")]
        [HasPermission("Permission.Get.Permission", PermissionType.System)]
        public async Task<IActionResult> GetAsync(int id)
        {
            if (ModelState.IsValid)
            {
                Permission? permission = await _dbPermissionService.GetAsync(id);
                if (permission != null)
                {
                    return Ok(permission);
                }
                return NotFound("Permission not found.");
            }
            return BadRequest("Invalid Request.");
        }

        [HttpGet("")]
        [HasPermission("Permission.Get-All.Permission", PermissionType.System)]
        public async Task<IActionResult> GetAllAsync()
        {
            if (ModelState.IsValid)
            {
                ICollection<Permission>? permissions = await _dbPermissionService.GetAllAsync();
                if (permissions != null)
                {
                    return Ok(permissions);
                }
                return NotFound("No permissions found.");
            }
            return BadRequest("Invalid Request.");
        }
    }
}
