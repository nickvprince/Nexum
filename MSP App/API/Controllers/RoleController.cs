using Microsoft.AspNetCore.Mvc;
using SharedComponents.Entities.DbEntities;
using SharedComponents.Entities.WebEntities.Requests.RoleRequests;
using SharedComponents.Handlers.Attributes.HasPermission;
using SharedComponents.Services.DbServices.Interfaces;

namespace API.Controllers
{
    /// <summary>
    /// Controller for managing roles.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "v1-Web")]
    public class RoleController : ControllerBase
    {
        private readonly IDbRoleService _dbRoleService;
        private readonly IDbUserService _dbUserService;
        private readonly IDbPermissionService _dbPermissionService;
        private readonly IDbTenantService _dbTenantService;

        /// <summary>
        /// Initializes a new instance of the <see cref="RoleController"/> class.
        /// </summary>
        /// <param name="dbRoleService">The role service.</param>
        /// <param name="dbUserService">The user service.</param>
        /// <param name="dbPermissionService">The permission service.</param>
        /// <param name="dbTenantService">The tenant service.</param>
        public RoleController(IDbRoleService dbRoleService, IDbUserService dbUserService,
            IDbPermissionService dbPermissionService, IDbTenantService dbTenantService)
        {
            _dbRoleService = dbRoleService;
            _dbUserService = dbUserService;
            _dbPermissionService = dbPermissionService;
            _dbTenantService = dbTenantService;
        }

        /// <summary>
        /// Creates a new role.
        /// </summary>
        /// <param name="request">The role create request.</param>
        /// <returns>An action result containing the created role.</returns>
        [HttpPost("")]
        [HasPermission("Role.Create.Permission", PermissionType.System)]
        public async Task<IActionResult> CreateAsync([FromBody] RoleCreateRequest request)
        {
            if (ModelState.IsValid)
            {
                ApplicationRole? role = new ApplicationRole
                {
                    Name = request.Name,
                    Description = request.Description
                };
                role = await _dbRoleService.CreateAsync(role);
                if (role != null)
                {
                    return Ok(role);
                }
                return BadRequest("An error occurred while creating the role.");
            }
            return BadRequest("Invalid Request.");
        }

        /// <summary>
        /// Updates an existing role.
        /// </summary>
        /// <param name="request">The role update request.</param>
        /// <returns>An action result containing the updated role.</returns>
        [HttpPut("")]
        [HasPermission("Role.Update.Permission", PermissionType.System)]
        public async Task<IActionResult> UpdateAsync([FromBody] RoleUpdateRequest request)
        {
            if (ModelState.IsValid)
            {
                ApplicationRole? role = await _dbRoleService.GetAsync(request.Id);
                if (role == null)
                {
                    return NotFound("Role not found.");
                }
                role.Name = request.Name;
                role.Description = request.Description;
                role = await _dbRoleService.UpdateAsync(role);
                if (role != null)
                {
                    return Ok(role);
                }
                return BadRequest("An error occurred while updating the role.");
            }
            return BadRequest("Invalid Request.");
        }

        /// <summary>
        /// Deletes a role by ID.
        /// </summary>
        /// <param name="id">The ID of the role to delete.</param>
        /// <returns>An action result indicating the outcome of the deletion.</returns>
        [HttpDelete("{id}")]
        [HasPermission("Role.Delete.Permission", PermissionType.System)]
        public async Task<IActionResult> DeleteAsync(string? id)
        {
            if (ModelState.IsValid)
            {
                if (await _dbRoleService.DeleteAsync(id))
                {
                    return Ok($"Role deleted successfully.");
                }
                return NotFound("Role not found.");
            }
            return BadRequest("Invalid Request.");
        }

        /// <summary>
        /// Gets a role by ID.
        /// </summary>
        /// <param name="id">The ID of the role to retrieve.</param>
        /// <returns>An action result containing the role.</returns>
        [HttpGet("{id}")]
        [HasPermission("Role.Get.Permission", PermissionType.System)]
        public async Task<IActionResult> GetAsync(string? id)
        {
            if (ModelState.IsValid)
            {
                ApplicationRole? role = await _dbRoleService.GetAsync(id);
                if (role != null)
                {
                    return Ok(role);
                }
                return NotFound("Role not found.");
            }
            return BadRequest("Invalid Request.");
        }

        /// <summary>
        /// Gets all roles.
        /// </summary>
        /// <returns>An action result containing all roles.</returns>
        [HttpGet("")]
        [HasPermission("Role.Get.Permission", PermissionType.System)]
        public async Task<IActionResult> GetAllAsync()
        {
            if (ModelState.IsValid)
            {
                ICollection<ApplicationRole>? roles = await _dbRoleService.GetAllAsync();
                if (roles != null)
                {
                    return Ok(roles);
                }
                return NotFound("No roles found.");
            }
            return BadRequest("Invalid Request.");
        }

        /// <summary>
        /// Gets all roles by user ID.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <returns>An action result containing the roles assigned to the user.</returns>
        [HttpGet("By-User/{userId}")]
        [HasPermission("Role.Get.Permission", PermissionType.System)]
        public async Task<IActionResult> GetAllByUserIdAsync(string? userId)
        {
            if (ModelState.IsValid)
            {
                ApplicationUser? user = await _dbUserService.GetByIdAsync(userId);
                if (user == null)
                {
                    return NotFound("User not found.");
                }
                ICollection<ApplicationRole>? roles = await _dbRoleService.GetAllByUserIdAsync(userId);
                if (roles != null)
                {
                    return Ok(roles);
                }
                return NotFound("No roles found.");
            }
            return BadRequest("Invalid Request.");
        }

        /// <summary>
        /// Gets all user roles by user ID.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <returns>An action result containing the user roles assigned to the user.</returns>
        [HttpGet("UserRole/{userId}")]
        [HasPermission("Role.Get.Permission", PermissionType.System)]
        public async Task<IActionResult> GetAllUserRolesByUserIdAsync(string? userId)
        {
            if (ModelState.IsValid)
            {
                ApplicationUser? user = await _dbUserService.GetByIdAsync(userId);
                if (user == null)
                {
                    return NotFound("User not found.");
                }
                ICollection<ApplicationUserRole>? userRoles = await _dbRoleService.GetAllUserRolesByUserIdAsync(userId);
                if (userRoles != null)
                {
                    return Ok(userRoles);
                }
                return NotFound("No roles found.");
            }
            return BadRequest("Invalid Request.");
        }

        /// <summary>
        /// Gets all role permissions by role ID.
        /// </summary>
        /// <param name="roleId">The ID of the role.</param>
        /// <returns>An action result containing the permissions assigned to the role.</returns>
        [HttpGet("Permission/{roleId}")]
        [HasPermission("Role.Get.Permission", PermissionType.System)]
        public async Task<IActionResult> GetAllRolePermissionsByIdAsync(string? roleId)
        {
            if (ModelState.IsValid)
            {
                ApplicationRole? role = await _dbRoleService.GetAsync(roleId);
                if (role == null)
                {
                    return NotFound("Role not found.");
                }
                ICollection<ApplicationRolePermission>? rolePermissions = await _dbRoleService.GetAllRolePermissionByIdAsync(roleId);
                if (rolePermissions != null)
                {
                    return Ok(rolePermissions);
                }
                return NotFound("No role permissions found.");
            }
            return BadRequest("Invalid Request.");
        }

        /// <summary>
        /// Assigns a role to a user.
        /// </summary>
        /// <param name="request">The role assign request.</param>
        /// <returns>An action result indicating the outcome of the assignment.</returns>
        [HttpPost("Assign")]
        [HasPermission("Role.Update.Permission", PermissionType.System)]
        public async Task<IActionResult> AssignAsync([FromBody] RoleAssignRequest request)
        {
            if (ModelState.IsValid)
            {
                ApplicationUser? user = await _dbUserService.GetByIdAsync(request.UserId);
                if (user == null)
                {
                    return NotFound("User not found.");
                }
                ApplicationRole? role = await _dbRoleService.GetAsync(request.RoleId);
                if(role == null)
                {
                    return NotFound("Role not found.");
                }
                ICollection<ApplicationRole>? userRoles = await _dbRoleService.GetAllByUserIdAsync(request.UserId);
                if (userRoles != null)
                {
                    if (userRoles.Any(r => r.Id == request.RoleId))
                    {
                        return BadRequest("User already has the role.");
                    }
                }
                ApplicationUserRole? userRole = new ApplicationUserRole
                {
                    UserId = user.Id,
                    RoleId = role.Id,
                    IsActive = false
                };
                if (await _dbRoleService.AssignAsync(userRole))
                {
                    return Ok("Role assigned successfully.");
                }
                return BadRequest("An error occurred while assigning the role.");
            }
            return BadRequest("Invalid Request.");
        }

        /// <summary>
        /// Unassigns a role from a user.
        /// </summary>
        /// <param name="request">The role unassign request.</param>
        /// <returns>An action result indicating the outcome of the unassignment.</returns>
        [HttpPost("Unassign")]
        [HasPermission("Role.Update.Permission", PermissionType.System)]
        public async Task<IActionResult> UnassignAsync([FromBody] RoleUnassignRequest request)
        {
            if (ModelState.IsValid)
            {
                ApplicationUser? user = await _dbUserService.GetByIdAsync(request.UserId);
                if (user == null)
                {
                    return NotFound("User not found.");
                }
                ApplicationRole? role = await _dbRoleService.GetAsync(request.RoleId);
                if (role == null)
                {
                    return NotFound("Role not found.");
                }
                ICollection<ApplicationRole>? assignedRoles = await _dbRoleService.GetAllByUserIdAsync(request.UserId);
                if (assignedRoles != null)
                {
                    if (!assignedRoles.Any(r => r.Id == request.RoleId))
                    {
                        return BadRequest("User does not have this role.");
                    }
                    ICollection<ApplicationUserRole>? userRoles = await _dbRoleService.GetAllUserRolesByUserIdAsync(request.UserId);
                    if (userRoles != null)
                    {
                        ApplicationUserRole? userRole = userRoles.FirstOrDefault(ur => ur.RoleId == request.RoleId);
                        if(userRole != null)
                        {
                            if (await _dbRoleService.UnassignAsync(userRole))
                            {
                                return Ok("Role unassigned successfully.");
                            }
                        }
                    }
                }
                return BadRequest("An error occurred while unassigning the role.");
            }
            return BadRequest("Invalid Request.");
        }

        /// <summary>
        /// Assigns a permission to a role.
        /// </summary>
        /// <param name="request">The role assign permission request.</param>
        /// <returns>An action result indicating the outcome of the assignment.</returns>
        [HttpPost("Assign-Permission")]
        [HasPermission("Role.Update.Permission", PermissionType.System)]
        public async Task<IActionResult> AssignPermissionAsync([FromBody] RoleAssignPermissionRequest request)
        {
            if (ModelState.IsValid)
            {
                ApplicationRole? role = await _dbRoleService.GetAsync(request.RoleId);
                if (role == null)
                {
                    return NotFound("Role not found.");
                }
                Permission? permission = await _dbPermissionService.GetAsync(request.PermissionId);
                if (permission == null)
                {
                    return NotFound("Permission not found.");
                }
                Tenant? tenant = await _dbTenantService.GetAsync(request.TenantId);
                if (tenant == null)
                {
                    return NotFound("Tenant not found.");
                }
                ICollection<ApplicationRolePermission>? rolePermissions = await _dbRoleService.GetAllRolePermissionByIdAsync(request.RoleId);
                if (rolePermissions != null)
                {
                    if (rolePermissions.Any(rp => rp.PermissionId == request.PermissionId && rp.TenantId == request.TenantId))
                    {
                        return BadRequest("Role already has the permission.");
                    }
                }
                ApplicationRolePermission? rolePermission = new ApplicationRolePermission
                {
                    RoleId = request.RoleId,
                    PermissionId = request.PermissionId,
                    TenantId = request.TenantId
                };
                if (await _dbRoleService.AssignPermissionAsync(rolePermission))
                {
                    return Ok("Permission assigned successfully.");
                }
                return BadRequest("An error occurred while assigning the permission.");
            }
            return BadRequest("Invalid Request.");
        }

        /// <summary>
        /// Unassigns a permission from a role.
        /// </summary>
        /// <param name="request">The role unassign permission request.</param>
        /// <returns>An action result indicating the outcome of the unassignment.</returns>
        [HttpPost("Unassign-Permission")]
        [HasPermission("Role.Update.Permission", PermissionType.System)]
        public async Task<IActionResult> UnassignPermissionAsync([FromBody] RoleUnassignPermissionRequest request)
        {
            if (ModelState.IsValid)
            {
                ApplicationRole? role = await _dbRoleService.GetAsync(request.RoleId);
                if (role == null)
                {
                    return NotFound("Role not found.");
                }
                Permission? permission = await _dbPermissionService.GetAsync(request.PermissionId);
                if (permission == null)
                {
                    return NotFound("Permission not found.");
                }
                Tenant? tenant = await _dbTenantService.GetAsync(request.TenantId);
                if (tenant == null)
                {
                    return NotFound("Tenant not found.");
                }
                ICollection<ApplicationRolePermission>? rolePermissions = await _dbRoleService.GetAllRolePermissionByIdAsync(request.RoleId);
                if (rolePermissions == null)
                {
                    return BadRequest("Role does not have any permissions assigned.");
                }
                ApplicationRolePermission? rolePermission = rolePermissions.FirstOrDefault(rp => rp.PermissionId == request.PermissionId && rp.TenantId == request.TenantId);
                if (rolePermission == null)
                {
                    return BadRequest("Role does not have the permission.");
                }
                if (await _dbRoleService.UnassignPermissionAsync(rolePermission))
                {
                    return Ok("Permission unassigned successfully.");
                }
                return BadRequest("An error occurred while unassigning the permission.");
            }
            return BadRequest("Invalid Request.");
        }
    }
}
