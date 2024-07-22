using Newtonsoft.Json;

namespace SharedComponents.Entities
{
    public class ApplicationRolePermission
    {
        public string? RoleId { get; set; }
        [JsonIgnore]
        public ApplicationRole? Role { get; set; }
        public int PermissionId { get; set; }
        [JsonIgnore]
        public Permission? Permission { get; set; }
        public int TenantId { get; set; }
        [JsonIgnore]
        public Tenant? Tenant { get; set; }
    }
}
