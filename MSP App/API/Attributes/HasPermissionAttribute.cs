using Microsoft.AspNetCore.Authorization;

namespace API.Attributes
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public class HasPermissionAttribute : AuthorizeAttribute
    {
        public HasPermissionAttribute(string permission) : base("HasPermission")
        {
            Permission = permission;
        }
        public string Permission { get; }
    }
}
