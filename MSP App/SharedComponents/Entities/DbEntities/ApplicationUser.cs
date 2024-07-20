using Microsoft.AspNetCore.Identity;

namespace SharedComponents.Entities.DbEntities
{
    public class ApplicationUser : IdentityUser
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public AccountType Type { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime RefreshTokenExpiryTime { get; set; }
        public int? TenantId { get; set; }
        [System.Text.Json.Serialization.JsonIgnore]
        public ICollection<ApplicationUserRole>? UserRoles { get; set; }
    }

    public enum AccountType
    {
        MSP,
        Tenant
    }
}
