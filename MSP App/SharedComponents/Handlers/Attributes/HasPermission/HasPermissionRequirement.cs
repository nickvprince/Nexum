using Microsoft.AspNetCore.Authorization;
using SharedComponents.Entities.DbEntities;

namespace SharedComponents.Handlers.Attributes.HasPermission
{
    public class HasPermissionRequirement : IAuthorizationRequirement
    {
        public HasPermissionRequirement(string permission, PermissionType type)
        {
            Permission = permission;
            Type = type;
        }

        public string Permission { get; }
        public PermissionType Type { get; }
    }
}
