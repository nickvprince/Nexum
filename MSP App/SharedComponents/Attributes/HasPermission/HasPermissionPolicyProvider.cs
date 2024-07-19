using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using SharedComponents.Attributes.HasPermission;
using SharedComponents.Entities;

namespace API.Attributes.HasPermission
{
    public class HasPermissionPolicyProvider : IAuthorizationPolicyProvider
    {
        public DefaultAuthorizationPolicyProvider FallbackPolicyProvider { get; }

        public HasPermissionPolicyProvider(IOptions<AuthorizationOptions> options)
        {
            FallbackPolicyProvider = new DefaultAuthorizationPolicyProvider(options);
        }

        public async Task<AuthorizationPolicy> GetDefaultPolicyAsync()
        {
            return await FallbackPolicyProvider.GetDefaultPolicyAsync();
        }

        public async Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
        {
            if (policyName.StartsWith("HasPermission", StringComparison.OrdinalIgnoreCase))
            {
                var parts = policyName.Split(':');
                if (parts.Length < 3 || !Enum.TryParse(parts[2], out PermissionType type))
                {
                    return null;
                }
                //var permission = parts.Length > 1 ? parts[1] : string.Empty;
                //var type = parts.Length > 2 && Enum.TryParse(parts[2], out PermissionType parsedType) ? parsedType : PermissionType.System;

                var permission = parts[1];

                var policy = new AuthorizationPolicyBuilder();
                policy.AddRequirements(new HasPermissionRequirement(permission, type));
                return await Task.Run(policy.Build);
            }
            return await FallbackPolicyProvider.GetPolicyAsync(policyName);
        }

        public async Task<AuthorizationPolicy?> GetFallbackPolicyAsync()
        {
            return await FallbackPolicyProvider.GetFallbackPolicyAsync();
        }
    }
}
