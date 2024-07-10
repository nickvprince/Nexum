using Newtonsoft.Json;

namespace SharedComponents.Entities
{
    public class Permission
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        [JsonIgnore]
        public ICollection<ApplicationRolePermission>? RolePermissions { get; set; }
    }
}
