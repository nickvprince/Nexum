using System.ComponentModel.DataAnnotations;

namespace SharedComponents.Entities.DbEntities
{
    public class Tenant
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        [Required]
        public TenantInfo? TenantInfo { get; set; }
        public string? ApiKey { get; set; }
        public string? ApiBaseUrl { get; set; }
        public int? ApiBasePort { get; set; }
        public string? ApiKeyServer { get; set; }
        public bool IsActive { get; set; }
        public ICollection<InstallationKey>? InstallationKeys { get; set; }
        public ICollection<Device>? Devices { get; set; }
        [System.Text.Json.Serialization.JsonIgnore]
        public ICollection<ApplicationRolePermission>? RolePermissions { get; set; }
        public ICollection<NASServer>? NASServers { get; set; }
    }
}
