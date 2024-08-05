using Microsoft.AspNetCore.Mvc;
using SharedComponents.Entities.DbEntities;
using SharedComponents.Handlers.Attributes.HasPermission;
using SharedComponents.Services.DbServices.Interfaces;

namespace API.Controllers
{
    /// <summary>
    /// Controller for managing permissions.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "v1-Web")]
    public class PermissionController : ControllerBase
    {
        private readonly IDbPermissionService _dbPermissionService;

        /// <summary>
        /// Initializes a new instance of the <see cref="PermissionController"/> class.
        /// </summary>
        /// <param name="dbPermissionService">The permission service.</param>
        public PermissionController(IDbPermissionService dbPermissionService)
        {
            _dbPermissionService = dbPermissionService;
        }
        //Functions that can be added for debugging / future purposes (working code)
        /*
        /// <summary>
        /// Creates a new permission.
        /// </summary>
        /// <param name="request">The permission create request.</param>
        /// <returns>An action result containing the created permission.</returns>
        [HttpPost("")]
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

        /// <summary>
        /// Updates an existing permission.
        /// </summary>
        /// <param name="request">The permission update request.</param>
        /// <returns>An action result containing the updated permission.</returns>
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

        /// <summary>
        /// Deletes a permission by ID.
        /// </summary>
        /// <param name="id">The ID of the permission to delete.</param>
        /// <returns>An action result indicating the outcome of the deletion.</returns>
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

        /// <summary>
        /// Gets a permission by ID.
        /// </summary>
        /// <param name="id">The ID of the permission to retrieve.</param>
        /// <returns>An action result containing the permission.</returns>
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

        /// <summary>
        /// Gets all permissions.
        /// </summary>
        /// <returns>An action result containing all permissions.</returns>
        [HttpGet("")]
        [HasPermission("Permission.Get.Permission", PermissionType.System)]
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
