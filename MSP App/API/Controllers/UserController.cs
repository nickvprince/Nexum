using API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SharedComponents.DbServices;
using SharedComponents.Entities;
using SharedComponents.Utilities;
using SharedComponents.WebEntities.Requests.UserRequests;
using SharedComponents.WebEntities.Responses.UserResponses;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "v1-Web")]
    public class UserController : ControllerBase
    {
        private readonly IDbUserService _dbUserService;

        public UserController(IDbUserService dbUserService)
        {
            _dbUserService = dbUserService;
        }

        [HttpPost("")]
        public async Task<IActionResult> CreateAsync([FromBody] UserCreateRequest request)
        {
            if (ModelState.IsValid)
            {
                
                ApplicationUser user = new ApplicationUser
                {
                    UserName = request.UserName,
                    Email = request.Email,
                    FirstName = request.FirstName,
                    LastName = request.LastName
                };
                if (request.TenantId == null)
                {
                    user.Type = AccountType.MSP;
                }
                else
                {
                    user.Type = AccountType.Tenant;
                    user.TenantId = request.TenantId;
                }
                string? defaultPassword = SecurityUtilities.GenerateDefaultRandomPassword();
                if (await _dbUserService.CreateAsync(user, defaultPassword))
                {
                    UserCreateResponse response = new UserCreateResponse
                    {
                        UserName = user.UserName,
                        Password = defaultPassword
                    };
                    return Ok(response);
                }
                return BadRequest("An error occurred while creating the user.");
            }
            return BadRequest("Invalid request.");
        }

        [HttpPut("")]
        public async Task<IActionResult> UpdateAsync([FromBody] UserUpdateRequest request)
        {
            if (ModelState.IsValid)
            {
                ApplicationUser? user = await _dbUserService.GetByUserNameAsync(request.UserName);
                if (user == null)
                {
                    return NotFound("User not found.");
                }
                if (string.IsNullOrEmpty(request.CurrentPassword))
                {
                    return BadRequest("Current password is required.");
                }
                if(!await _dbUserService.CheckPasswordAsync(user.UserName, request.CurrentPassword))
                {
                    return BadRequest("Invalid password.");
                }
                user.FirstName = request.FirstName;
                user.LastName = request.LastName;
                user.Email = request.Email;
                if (await _dbUserService.UpdateAsync(user))
                {
                    UserUpdateResponse response = new UserUpdateResponse
                    {
                        UserName = user.UserName,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        Email = user.Email
                    };
                    return Ok(response);
                }
                return BadRequest("An error occurred while updating the user.");
            }
            return BadRequest("Invalid request.");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAsync(string id)
        {
            if (ModelState.IsValid)
            {
                if (id != null)
                {
                    if (await _dbUserService.DeleteAsync(id))
                    {
                        return Ok("User deleted successfully.");
                    }
                    return BadRequest("An error occurred while deleting the user.");
                }
                return BadRequest("Invalid request.");
            }
            return BadRequest("Invalid request.");
        }

        [HttpGet("By-Id/{id}")]
        public async Task<IActionResult> GetByIdAsync(string id)
        {
            if (ModelState.IsValid)
            {
                if (!string.IsNullOrEmpty(id))
                {
                    ApplicationUser? user = await _dbUserService.GetByIdAsync(id);
                    if (user != null)
                    {
                        UserResponse response = new UserResponse
                        {
                            Id = user.Id,
                            UserName = user.UserName,
                            FirstName = user.FirstName,
                            LastName = user.LastName,
                            Email = user.Email,
                            Type = user.Type
                        };
                        return Ok(response);
                    }
                    return NotFound("User not found.");
                }
            }
            return BadRequest("Invalid request.");
        }

        [HttpGet("By-Username/{username}")]
        public async Task<IActionResult> GetByUserNameAsync(string username)
        {
            if (ModelState.IsValid)
            {
                if (!string.IsNullOrEmpty(username))
                {
                    ApplicationUser? user = await _dbUserService.GetByUserNameAsync(username);
                    if (user != null)
                    {
                        UserResponse response = new UserResponse
                        {
                            Id = user.Id,
                            UserName = user.UserName,
                            FirstName = user.FirstName,
                            LastName = user.LastName,
                            Email = user.Email,
                            Type = user.Type
                        };
                        return Ok(response);
                    }
                    return NotFound("User not found.");
                }
            }
            return BadRequest("Invalid request.");
        }

        [Authorize()]
        [HttpGet("")]
        public async Task<IActionResult> GetAllAsync()
        {
            if (ModelState.IsValid)
            {
                ICollection<ApplicationUser>? users = await _dbUserService.GetAllAsync();
                if (users != null)
                {
                    if (users.Any())
                    {
                        ICollection<UserResponse> response = users.Select(u => new UserResponse
                        {
                            Id = u.Id,
                            UserName = u.UserName,
                            FirstName = u.FirstName,
                            LastName = u.LastName,
                            Email = u.Email,
                            Type = u.Type
                        }).ToList();
                        return Ok(response);
                    }
                }
                return NotFound("No users found.");
            }
            return BadRequest("Invalid request.");
        }
    }
}
