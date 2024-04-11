using API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SharedComponents.Entities;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PermissionController : ControllerBase
    {
        private readonly DbPermissionService _dbPermissionService;

        public PermissionController(DbPermissionService dbPermissionService)
        {
            _dbPermissionService = dbPermissionService;
        }

        [HttpPost("Create")]
        public IActionResult CreatePermission([FromBody] object permission)
        {
            //Create the permission
            return Ok($"Permission created successfully.");
        }

        [HttpPut("Update")]
        public IActionResult UpdatePermission([FromBody] object permission)
        {
            //Update the permission
            return Ok($"Permission updated successfully.");
        }

        [HttpDelete("Delete/{id}")]
        public IActionResult DeletePermission(string id)
        {
            //Delete the permission
            return Ok($"Permission deleted successfully.");
        }

        [HttpGet("Get/{id}")]
        public IActionResult GetPermission(string id)
        {
            //Get the permission
            return Ok($"Retrieved permission successfully.");
        }

        [HttpGet("Get")]
        public async Task<IActionResult> GetPermissionsAsync()
        {
            //Get the permissions
            List<Permission> permissions = await _dbPermissionService.GetAllAsync();

            if (permissions.Any())
            {
                var response = new
                {
                    data = permissions,
                    message = $"Retrieved permissions successfully."
                };
                return Ok(response);
            }
            return NotFound(new { message = "No permissions found." });
        }
    }
}
