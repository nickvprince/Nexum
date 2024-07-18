using API.Attributes.HasPermission;
using API.DataAccess;
using API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using SharedComponents.DbServices;
using SharedComponents.JWTToken.Services;
using System.Reflection;

namespace API.Services
{
    public class AuthService : IAuthService
    {
        private readonly IJWTService _jwtService;
        private readonly IDbRoleService _dbRoleService;

        public AuthService(IJWTService jwtService, IDbRoleService dbRoleService)
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
                    var permission = roles
                        .SelectMany(r => r.RolePermissions)
                        .Where(rp => rp.Permission.Name == permissionName && rp.TenantId == tenantId)
                        .ToList();
                    if (permission != null)
                    {
                        if (permission.Any())
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        public Task<ICollection<int>> GetUserAccessibleTenantsAsync(string token)
        {
            throw new NotImplementedException();
        }
    }
}
