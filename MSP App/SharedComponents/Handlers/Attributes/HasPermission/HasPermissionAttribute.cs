using Microsoft.AspNetCore.Authorization;
using SharedComponents.Entities.DbEntities;

namespace SharedComponents.Handlers.Attributes.HasPermission
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public class HasPermissionAttribute : AuthorizeAttribute
    {
        public HasPermissionAttribute(string permission, PermissionType type) : base($"HasPermission:{permission}:{type}")
        {
            Permission = permission;
            Type = type;
        }
        public string Permission { get; }
        public PermissionType Type { get; }
    }
}
