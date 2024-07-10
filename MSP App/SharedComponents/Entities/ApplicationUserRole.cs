using Newtonsoft.Json;

namespace SharedComponents.Entities
{
    public class ApplicationUserRole
    {
        public string? UserId { get; set; }
        [JsonIgnore]
        public ApplicationUser? User { get; set; }
        public string? RoleId { get; set; }
        [JsonIgnore]
        public ApplicationRole? Role { get; set; }
        public bool IsActive { get; set; }
    }
}
