using API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SharedComponents.Entities;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserPermissionSetController : ControllerBase
    {
        private readonly DbUserPermissionSetService _dbUserPermissionSetService;
        private readonly ILogger<UserPermissionSetController> _logger;

        public UserPermissionSetController(DbUserPermissionSetService dbUserPermissionSetService, ILogger<UserPermissionSetController> logger)
        {
            _dbUserPermissionSetService = dbUserPermissionSetService;
            _logger = logger;
        }

        [HttpPost("Create")]
        public IActionResult CreatePermissionSet([FromBody] object permission)
        {
            //Create the permission
            return Ok($"Permission created successfully.");
        }

        [HttpPut("Update")]
        public IActionResult UpdatePermissionSet([FromBody] object permission)
        {
            //Update the permission
            return Ok($"Permission updated successfully.");
        }

        [HttpDelete("Delete/{id}")]
        public IActionResult DeletePermissionSet(string id)
        {
            //Delete the permission
            return Ok($"Permission deleted successfully.");
        }

        [HttpGet("Get/{id}")]
        public IActionResult GetPermissionSet(string id)
        {
            //Get the permission
            return Ok($"Retrieved permission successfully.");
        }

        [HttpGet("Get")]
        public async Task<IActionResult> GetPermissionSetsAsync()
        {
            try
            {
                //Get the permissions
                ICollection<UserPermissionSet> userPermissionsSets = await _dbUserPermissionSetService.GetAllAsync();

                if (userPermissionsSets.Any())
                {
                    return Ok(userPermissionsSets);
                }
                return NotFound(new { message = "No permissions sets found." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving permission sets.");
                return StatusCode(500, new { message = "An error occurred while retrieving permission sets." });
            }
            
        }
    }
}
