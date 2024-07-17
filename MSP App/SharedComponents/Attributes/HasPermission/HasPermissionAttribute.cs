using Microsoft.AspNetCore.Authorization;

namespace API.Attributes.HasPermission
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public class HasPermissionAttribute : AuthorizeAttribute
    {
        public HasPermissionAttribute(string permission) : base($"HasPermission:{permission}")
        {
            Permission = permission;
        }
        public string Permission { get; }
    }
}
