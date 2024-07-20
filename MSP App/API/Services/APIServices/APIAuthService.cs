using Microsoft.AspNetCore.Mvc;
using SharedComponents.Handlers.Attributes.HasPermission;
using SharedComponents.JWTToken.Services;
using SharedComponents.Services.APIServices.Interfaces;
using SharedComponents.Services.DbServices.Interfaces;

namespace API.Services.APIServices
{
    public class APIAuthService : IAPIAuthService
    {
        private readonly IJWTService _jwtService;
        private readonly IDbRoleService _dbRoleService;

        public APIAuthService(IJWTService jwtService, IDbRoleService dbRoleService)
        {
            _jwtService = jwtService;
            _dbRoleService = dbRoleService;
        }

        public async Task<bool> UserHasPermissionAsync<T>(string token, int tenantId, [System.Runtime.CompilerServices.CallerMemberName] string methodName = "") where T : ControllerBase
        {
            var method = typeof(T).GetMethod(methodName);
            var permissionAttribute = (HasPermissionAttribute)method?.GetCustomAttributes(typeof(HasPermissionAttribute), false).FirstOrDefault()!;
            if (permissionAttribute != null)
            {
                var permissionName = permissionAttribute.Permission;
                var userId = await _jwtService.GetUserIdFromTokenAsync(token.Replace("Bearer ", ""));
                var roles = await _dbRoleService.GetAllByUserIdAsync(userId);

                if (roles != null)
                {
                    var permissions = roles
                        .SelectMany(r => r.RolePermissions)
                        .Where(rp => rp.Permission.Name == permissionName && rp.TenantId == tenantId)
                        .ToList();
                    if (permissions != null)
                    {
                        if (permissions.Any())
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        public async Task<IEnumerable<int?>?> GetUserAccessibleTenantsAsync(string token)
        {
            var userId = await _jwtService.GetUserIdFromTokenAsync(token.Replace("Bearer ", ""));
            var roles = await _dbRoleService.GetAllByUserIdAsync(userId);
            if (roles != null)
            {
                var tenantIds = roles
                    .SelectMany(r => r.RolePermissions)
                    .Select(rp => rp.TenantId)
                    .ToList();
                if (tenantIds != null)
                {
                    if (tenantIds.Any())
                    {
                        return tenantIds.Distinct();
                    }
                }
            }
            return null;
        }
    }
}
