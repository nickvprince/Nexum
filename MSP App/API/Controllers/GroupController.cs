using API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SharedComponents.Entities;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GroupController : ControllerBase
    {
        private readonly DbGroupService _dbGroupService;

        public GroupController(DbGroupService dbGroupService)
        {
            _dbGroupService = dbGroupService;
        }

        [HttpPost("Create")]
        public IActionResult CreateGroup([FromBody] object group)
        {
            //Create the permission
            return Ok($"Group created successfully.");
        }

        [HttpPut("Update")]
        public IActionResult UpdateGroup([FromBody] object group)
        {
            //Update the permission
            return Ok($"Group updated successfully.");
        }

        [HttpDelete("Delete/{id}")]
        public IActionResult DeleteGroup(string id)
        {
            //Delete the permission
            return Ok($"Group deleted successfully.");
        }

        [HttpGet("Get/{id}")]
        public IActionResult GetGroup(string id)
        {
            //Get the permission
            return Ok($"Retrieved group successfully.");
        }

        [HttpGet("Get")]
        public async Task<IActionResult> GetGroupsAsync()
        {
            //Get the permissions
            List<Group> groups = await _dbGroupService.GetAllAsync();

            if (groups.Any())
            {
                var response = new
                {
                    data = groups,
                    message = $"Retrieved groups successfully."
                };
                return Ok(response);
            }
            return NotFound(new { message = "No groups found." });
        }
    }
}
