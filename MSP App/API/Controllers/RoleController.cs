using API.Services;
using Azure.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SharedComponents.Entities;
using SharedComponents.WebEntities.Requests.RoleRequests;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "v1-Web")]
    public class RoleController : ControllerBase
    {
        private readonly DbRoleService _dbRoleService;
        private readonly DbUserService _dbUserService;
        private readonly DbPermissionService _dbPermissionService;
        private readonly DbTenantService _dbTenantService;

        public RoleController(DbRoleService dbRoleService, DbUserService dbUserService, 
            DbPermissionService dbPermissionService, DbTenantService dbTenantService)
        {
            _dbRoleService = dbRoleService;
            _dbUserService = dbUserService;
            _dbPermissionService = dbPermissionService;
            _dbTenantService = dbTenantService;
        }

        [HttpPost("")]
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

        [HttpPut("")]
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

        [HttpDelete("{id}")]
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

        [HttpGet("{id}")]
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

        [HttpGet("")]
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

        [HttpGet("By-User/{userId}")]
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

        [HttpGet("UserRole/{userId}")]
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

        [HttpGet("Permission/{roleId}")]
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

        [HttpPost("Assign")]
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

        [HttpPost("Unassign")]
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

        [HttpPost("Assign-Permission")]
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

        [HttpPost("Unassign-Permission")]
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
