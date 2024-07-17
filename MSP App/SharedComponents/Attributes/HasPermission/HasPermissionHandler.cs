using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using SharedComponents.DbServices;
using SharedComponents.Entities;

namespace API.Attributes.HasPermission
{
    public class HasPermissionHandler : AuthorizationHandler<HasPermissionRequirement>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IDbRoleService _dbRoleService;

        public HasPermissionHandler(UserManager<ApplicationUser> userManager, IDbRoleService dbRoleService)
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

            var roles = await _dbRoleService.GetAllByUserIdAsync(user.Id);
            var permission = roles
                .SelectMany(r => r.RolePermissions)
                .Where(rp => rp.Permission.Name == requirement.Permission)
                .Select(rp => rp.Permission.Name)
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
