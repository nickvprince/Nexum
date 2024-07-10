using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;

namespace SharedComponents.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public AccountType Type { get; set; }
        public int? TenantId { get; set; }
        [JsonIgnore]
        public ICollection<ApplicationUserRole>? UserRoles { get; set; }
    }

    public enum AccountType
    {
        MSP,
        Tenant
    }
}
