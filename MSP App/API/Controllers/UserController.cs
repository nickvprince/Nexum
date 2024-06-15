using API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SharedComponents.DbServices;
using SharedComponents.Entities;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly DbUserService _dbUserService;

        public UserController(DbUserService dbUserService)
        {
            _dbUserService = dbUserService;
        }

        [HttpPost("Create")]
        public IActionResult CreateUser([FromBody] object user)
        {
            //Create the user
            return Ok($"User created successfully.");
        }

        [HttpPut("Update")]
        public IActionResult UpdateUser([FromBody] object user)
        {
            //Update the user
            return Ok($"User updated successfully.");
        }

        [HttpDelete("Delete/{id}")]
        public IActionResult DeleteUser(string id)
        {
            //Delete the user
            return Ok($"User deleted successfully.");
        }

        [HttpGet("Get/{username}")]
        public async Task<IActionResult> GetUserAsync(string username)
        {
            //Get the user
            ApplicationUser? user = await _dbUserService.GetAsync(username);

            if (user != null)
            {
                return Ok(user);
            }
            return NotFound(new { message = "User not found." });
        }

        [HttpGet("Get")]
        public async Task<IActionResult> GetUsersAsync()
        {
            //Get the permissions
            ICollection<ApplicationUser> users = await _dbUserService.GetAllAsync();

            if (users.Any())
            {
                return Ok(new { User = users, Message = "" });
            }
            return NotFound(new { message = "No users found." });
        }
    }
}
