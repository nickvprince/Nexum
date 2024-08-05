using Microsoft.AspNetCore.Mvc;
using SharedComponents.Entities.DbEntities;
using SharedComponents.Entities.WebEntities.Requests.UserRequests;
using SharedComponents.Entities.WebEntities.Responses.UserResponses;
using SharedComponents.Handlers.Attributes.HasPermission;
using SharedComponents.Services.DbServices.Interfaces;
using SharedComponents.Utilities;
using System.Data;

namespace API.Controllers
{
    /// <summary>
    /// Controller for managing users.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "v1-Web")]
    public class UserController : ControllerBase
    {
        private readonly IDbUserService _dbUserService;
        private readonly IDbRoleService _dbRoleService;
        private readonly IDbTenantService _dbTenantService;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserController"/> class.
        /// </summary>
        /// <param name="dbUserService">The user service.</param>
        /// <param name="dbRoleService">The role service.</param>
        /// <param name="dbTenantService">The tenant service.</param>
        public UserController(IDbUserService dbUserService, IDbRoleService dbRoleService,
            IDbTenantService dbTenantService)
        {
            _dbUserService = dbUserService;
            _dbRoleService = dbRoleService;
            _dbTenantService = dbTenantService;
        }

        /// <summary>
        /// Creates a new user.
        /// </summary>
        /// <param name="request">The user create request.</param>
        /// <returns>An action result containing the created user.</returns>
        [HttpPost("")]
        [HasPermission("User.Create.Permission", PermissionType.System)]
        public async Task<IActionResult> CreateAsync([FromBody] UserCreateRequest request)
        {
            if (ModelState.IsValid)
            {
                Tenant? tenant = null;
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
                    tenant = await _dbTenantService.GetAsync((int)request.TenantId);
                    if (tenant == null)
                    {
                        return NotFound("Tenant not found.");
                    }
                    user.Type = AccountType.Tenant;
                    user.TenantId = tenant.Id;
                    
                }
                string? defaultPassword = SecurityUtilities.GenerateDefaultRandomPassword();
                if (await _dbUserService.CreateAsync(user, defaultPassword))
                {
                    if(user.Type == AccountType.Tenant)
                    {
                        ICollection<ApplicationRole> roles = await _dbRoleService.GetAllAsync();
                        ApplicationRole tenantViewerRole = roles.FirstOrDefault(r => r.Name != null && r.Name.Contains($"Tenant Viewer - {tenant.Name}"));
                        if (tenantViewerRole != null)
                        {
                            if (!await _dbRoleService.AssignAsync(new ApplicationUserRole
                            {
                                UserId = user.Id,
                                RoleId = tenantViewerRole.Id
                            }))
                            {
                                return BadRequest("An error occurred while assigning the role.");
                            }
                        }
                    }
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

        /// <summary>
        /// Updates an existing user.
        /// </summary>
        /// <param name="request">The user update request.</param>
        /// <returns>An action result containing the updated user.</returns>
        [HttpPut("")]
        [HasPermission("User.Update.Permission", PermissionType.System)]
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

        /// <summary>
        /// Deletes a user by ID.
        /// </summary>
        /// <param name="id">The ID of the user to delete.</param>
        /// <returns>An action result indicating the outcome of the deletion.</returns>
        [HttpDelete("{id}")]
        [HasPermission("User.Delete.Permission", PermissionType.System)]
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

        /// <summary>
        /// Gets all users.
        /// </summary>
        /// <returns>An action result containing all users.</returns>
        [HttpGet("")]
        [HasPermission("User.Get.Permission", PermissionType.System)]
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

        /// <summary>
        /// Gets a user by ID.
        /// </summary>
        /// <param name="id">The ID of the user to retrieve.</param>
        /// <returns>An action result containing the user.</returns>
        [HttpGet("By-Id/{id}")]
        [HasPermission("User.Get.Permission", PermissionType.System)]
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

        /// <summary>
        /// Gets a user by username.
        /// </summary>
        /// <param name="username">The username of the user to retrieve.</param>
        /// <returns>An action result containing the user.</returns>
        [HttpGet("By-Username/{username}")]
        [HasPermission("User.Get.Permission", PermissionType.System)]
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
    }
}
