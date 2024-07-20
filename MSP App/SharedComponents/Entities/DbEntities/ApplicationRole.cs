using System.ComponentModel.DataAnnotations;

namespace SharedComponents.Entities.DbEntities
{
    public class ApplicationRole
    {
        public string? Id { get; set; } = Guid.NewGuid().ToString();
        [Required]
        public string? Name { get; set; }
        [Required]
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        [System.Text.Json.Serialization.JsonIgnore]
        public ICollection<ApplicationRolePermission>? RolePermissions { get; set; }
        [System.Text.Json.Serialization.JsonIgnore]
        public ICollection<ApplicationUserRole>? UserRoles { get; set; }
    }
}