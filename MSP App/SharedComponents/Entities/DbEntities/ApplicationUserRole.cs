
namespace SharedComponents.Entities.DbEntities
{
    public class ApplicationUserRole
    {
        public string? UserId { get; set; }
        [System.Text.Json.Serialization.JsonIgnore]
        public ApplicationUser? User { get; set; }
        public string? RoleId { get; set; }
        [System.Text.Json.Serialization.JsonIgnore]
        public ApplicationRole? Role { get; set; }
        public bool IsActive { get; set; }
    }
}
