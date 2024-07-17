using API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using SharedComponents.Entities;

namespace API.Attributes.Handlers
{
    public class HasPermissionHandler : AuthorizationHandler<HasPermissionRequirement>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly DbRoleService _dbRoleService;

        public HasPermissionHandler(UserManager<ApplicationUser> userManager, DbRoleService dbRoleService)
        {
            _userManager = userManager;
            _dbRoleService = dbRoleService;
        }
        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, HasPermissionRequirement requirement)
        {
            if (context.User.Identity == null || !context.User.Identity.IsAuthenticated)
            {
                return;
            }

            var user = await _userManager.GetUserAsync(context.User);
            if (user == null)
            {
                return;
            }

            var userRoles = await _dbRoleService.GetAllUserRolesByUserIdAsync(user.Id);
            var permission = userRoles
                .SelectMany(r => r.Role.RolePermissions)
                .Select(rp => rp.Permission.Name == requirement.Permission)
                .ToList();
            if (permission == null)
            {
                return;
            }
            if (permission.Any())
            {
                context.Succeed(requirement);
                return;
            }
        }
    }

    public class HasPermissionRequirement : IAuthorizationRequirement
    {
        public HasPermissionRequirement(string permission)
        {
            Permission = permission;
        }

        public string Permission { get; }
    }
}
