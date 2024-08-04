using API.Services.DbServices;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using SharedComponents.Entities.DbEntities;
using SharedComponents.Entities.WebEntities.Requests.TenantRequests;
using SharedComponents.Handlers.Attributes.HasPermission;
using SharedComponents.Handlers.Results;
using SharedComponents.JWTToken.Services;
using SharedComponents.Services.APIServices.Interfaces;
using SharedComponents.Services.DbServices.Interfaces;

namespace API.Controllers
{
    /// <summary>
    /// Controller for managing tenants.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "v1-Web")]
    public class TenantController : ControllerBase
    {
        private readonly IDbTenantService _dbTenantService;
        private readonly IDbRoleService _dbRoleService;
        private readonly IDbPermissionService _dbPermissionService;
        private readonly IAPIAuthService _authService;
        private readonly IJWTService _jwtService;

        /// <summary>
        /// Initializes a new instance of the <see cref="TenantController"/> class.
        /// </summary>
        /// <param name="dbTenantService">The tenant service.</param>
        /// <param name="dbRoleService">The role service.</param>
        /// <param name="dbPermissionService">The permission service.</param>
        /// <param name="authService">The authentication service.</param>
        /// <param name="jwtService">The JWT service.</param>
        public TenantController(IDbTenantService dbTenantService, IDbRoleService dbRoleService,
            IDbPermissionService dbPermissionService, IAPIAuthService authService,
            IJWTService jwtService)
        {
            _dbTenantService = dbTenantService;
            _dbRoleService = dbRoleService;
            _dbPermissionService = dbPermissionService;
            _authService = authService;
            _jwtService = jwtService;
        }

        /// <summary>
        /// Creates a new tenant.
        /// </summary>
        /// <param name="request">The tenant create request.</param>
        /// <returns>An action result containing the created tenant.</returns>
        [HttpPost("")]
        [HasPermission("Tenant.Create.Permission", PermissionType.System)]
        public async Task<IActionResult> CreateAsync([FromBody] TenantCreateRequest request)
        {
            if (ModelState.IsValid)
            {
                Tenant? tenant = new Tenant
                {
                    Name = request.Name,
                    TenantInfo = new TenantInfo
                    {
                        Name = request.ContactName,
                        Email = request.ContactEmail,
                        Phone = request.ContactPhone,
                        Address = request.Address,
                        City = request.City,
                        State = request.State,
                        Zip = request.Zip,
                        Country = request.Country
                    }
                };
                tenant = await _dbTenantService.CreateAsync(tenant);
                if (tenant != null)
                {
                    ApplicationRole? adminRole = await _dbRoleService.CreateAsync(new ApplicationRole
                    {
                        Name = $"Tenant Admin - {tenant.Name}",
                        Description = $"Admin Role for the tenant: {tenant.Name}",
                        IsActive = true,
                    });
                    ApplicationRole? tenantRole = await _dbRoleService.CreateAsync(new ApplicationRole
                    {
                        Name = $"Tenant Viewer - {tenant.Name}",
                        Description = $"Tenant role for viewing information for the tenant: {tenant.Name}",
                        IsActive = true,
                    });
                    if (adminRole != null && tenantRole != null)
                    {
                        ICollection<Permission>? permissions = await _dbPermissionService.GetAllAsync();
                        if (permissions != null)
                        {
                            foreach (Permission permission in permissions.Where(p => p.Type == PermissionType.Tenant))
                            {
                                if (!await _dbRoleService.AssignPermissionAsync(new ApplicationRolePermission
                                {
                                    RoleId = adminRole.Id,
                                    PermissionId = permission.Id,
                                    TenantId = tenant.Id
                                }))
                                {
                                    return BadRequest("Error assigning permissions to the admin role.");
                                }
                            }
                            foreach (Permission permission in permissions.Where(p => p.Type == PermissionType.Tenant && p.Name.Contains("Get")))
                            {
                                if (!await _dbRoleService.AssignPermissionAsync(new ApplicationRolePermission
                                {
                                    RoleId = tenantRole.Id,
                                    PermissionId = permission.Id,
                                    TenantId = tenant.Id
                                }))
                                {
                                    return BadRequest("Error assigning permissions to the tenant role.");
                                }
                            }
                            if (!await _dbRoleService.AssignAsync(new ApplicationUserRole
                            {
                                RoleId = adminRole.Id,
                                UserId = await _jwtService.GetUserIdFromTokenAsync(Request.Headers["Authorization"].ToString().Replace("Bearer ", ""))
                            }))
                            {
                                return BadRequest("Error assigning role to the user.");
                            }
                            return Ok(tenant);
                        }
                    }
                }
                return BadRequest("An error occurred while creating the tenant.");
            }
            return BadRequest("Invalid request.");
        }

        /// <summary>
        /// Updates an existing tenant.
        /// </summary>
        /// <param name="request">The tenant update request.</param>
        /// <returns>An action result containing the updated tenant.</returns>
        [HttpPut("")]
        [HasPermission("Tenant.Update.Permission", PermissionType.Tenant)]
        public async Task<IActionResult> UpdateAsync([FromBody] TenantUpdateRequest request)
        {
            if (ModelState.IsValid)
            {
                // Authentication check using roles + permissions
                if (!await _authService.UserHasPermissionAsync<TenantController>(Request.Headers["Authorization"].ToString(), request.Id))
                {
                    return new CustomForbidResult("User do not have access to this feature for the specified tenant");
                }
                // --- End of authentication check ---
                Tenant? tenant = await _dbTenantService.GetAsync(request.Id);
                if (tenant == null)
                {
                    return NotFound("Tenant not found.");
                }
                if (tenant.TenantInfo == null)
                {
                    return NotFound("TenantInfo not found.");
                }
                tenant.Name = request.Name;
                tenant.TenantInfo.Name = request.ContactName;
                tenant.TenantInfo.Email = request.ContactEmail;
                tenant.TenantInfo.Phone = request.ContactPhone;
                tenant.TenantInfo.Address = request.Address;
                tenant.TenantInfo.City = request.City;
                tenant.TenantInfo.State = request.State;
                tenant.TenantInfo.Zip = request.Zip;
                tenant.TenantInfo.Country = request.Country;
                tenant = await _dbTenantService.UpdateAsync(tenant);
                if (tenant != null)
                {
                    return Ok(tenant);
                }
                return BadRequest("An error occurred while updating the tenant.");
            }
            return BadRequest("Invalid request.");
        }

        /// <summary>
        /// Deletes a tenant by ID.
        /// </summary>
        /// <param name="id">The ID of the tenant to delete.</param>
        /// <returns>An action result indicating the outcome of the deletion.</returns>
        [HttpDelete("{id}")]
        [HasPermission("Tenant.Delete.Permission", PermissionType.Tenant)]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            if (ModelState.IsValid)
            {
                // Authentication check using roles + permissions
                if (!await _authService.UserHasPermissionAsync<TenantController>(Request.Headers["Authorization"].ToString(), id))
                {
                    return new CustomForbidResult("User do not have access to this feature for the specified tenant");
                }
                // --- End of authentication check ---
                Tenant? tenant = await _dbTenantService.GetAsync(id);
                if (tenant == null)
                {
                    return NotFound("Tenant not found.");
                }
                if (await _dbTenantService.DeleteAsync(id))
                {
                    return Ok($"Tenant deleted successfully.");

                }
                return NotFound("Tenant not found.");
            }
            return BadRequest("Invalid request.");
        }

        /// <summary>
        /// Gets a tenant by ID.
        /// </summary>
        /// <param name="id">The ID of the tenant to retrieve.</param>
        /// <returns>An action result containing the tenant.</returns>
        [HttpGet("{id}")]
        [HasPermission("Tenant.Get.Permission", PermissionType.Tenant)]
        public async Task<IActionResult> GetAsync(int id)
        {
            if (ModelState.IsValid)
            {
                // Authentication check using roles + permissions
                if (!await _authService.UserHasPermissionAsync<TenantController>(Request.Headers["Authorization"].ToString(), id))
                {
                    return new CustomForbidResult("User do not have access to this feature for the specified tenant");
                }
                // --- End of authentication check ---
                Tenant? tenant = await _dbTenantService.GetAsync(id);
                if (tenant != null)
                {
                    return Ok(tenant);
                }
                return NotFound("Tenant not found.");
            }
            return BadRequest("Invalid request.");
        }

        /// <summary>
        /// Gets a tenant by ID with additional rich information.
        /// </summary>
        /// <param name="id">The ID of the tenant to retrieve.</param>
        /// <returns>An action result containing the tenant with rich information.</returns>
        [HttpGet("{id}/Rich")]
        [HasPermission("Tenant.Get.Permission", PermissionType.Tenant)]
        public async Task<IActionResult> GetRichAsync(int id)
        {
            if (ModelState.IsValid)
            {
                // Authentication check using roles + permissions
                if (!await _authService.UserHasPermissionAsync<TenantController>(Request.Headers["Authorization"].ToString(), id))
                {
                    return new CustomForbidResult("User do not have access to this feature for the specified tenant");
                }
                // --- End of authentication check ---
                Tenant? tenant = await _dbTenantService.GetRichAsync(id);
                if (tenant != null)
                {
                    return Ok(tenant);
                }
                return NotFound("Tenant not found.");
            }
            return BadRequest("Invalid request.");
        }

        /// <summary>
        /// Gets all tenants accessible by the current user.
        /// </summary>
        /// <returns>An action result containing all accessible tenants.</returns>
        [HttpGet("")]
        [HasPermission("Tenant.Get.Permission", PermissionType.Tenant)]
        public async Task<IActionResult> GetAllAsync()
        {
            if (ModelState.IsValid)
            {
                var tenantIds = await _authService.GetUserAccessibleTenantsAsync(Request.Headers["Authorization"].ToString());
                if (tenantIds == null)
                {
                    return new CustomForbidResult("User does not have any tenant permissions");
                }
                List<Tenant>? tenants = new List<Tenant>();
                foreach (var tenantId in tenantIds)
                {
                    if (tenantId != null)
                    {
                        var tenant = await _dbTenantService.GetAsync((int)tenantId);
                        if (tenant != null)
                        {
                            tenants.Add(tenant);
                        }
                    }
                }
                if (tenants != null)
                {
                    if (tenants.Any())
                    {
                        return Ok(tenants);
                    }
                }
                return NotFound("No tenants found.");
            }
            return BadRequest("Invalid request.");
        }
    }
}
