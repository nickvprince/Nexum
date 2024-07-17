using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

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
                var permission = parts.Length > 1 ? parts[1] : string.Empty;

                var policy = new AuthorizationPolicyBuilder();
                policy.AddRequirements(new HasPermissionRequirement(permission));
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
