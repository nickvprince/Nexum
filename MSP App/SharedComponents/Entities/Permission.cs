
namespace SharedComponents.Entities
{
    public class Permission
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public PermissionType Type { get; set; }
        [System.Text.Json.Serialization.JsonIgnore]
        public ICollection<ApplicationRolePermission>? RolePermissions { get; set; }
    }

    public enum PermissionType
    {
        System,
        Tenant
    }
}
