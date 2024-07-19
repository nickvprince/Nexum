
namespace SharedComponents.Entities
{
    public class ApplicationRolePermission
    {
        public int Id { get; set; }
        public string? RoleId { get; set; }
        [System.Text.Json.Serialization.JsonIgnore]
        public ApplicationRole? Role { get; set; }
        public int PermissionId { get; set; }
        [System.Text.Json.Serialization.JsonIgnore]
        public Permission? Permission { get; set; }
        public int? TenantId { get; set; }
        [System.Text.Json.Serialization.JsonIgnore]
        public Tenant? Tenant { get; set; }
    }
}
