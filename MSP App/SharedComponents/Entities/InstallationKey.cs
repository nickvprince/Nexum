using Newtonsoft.Json;

namespace SharedComponents.Entities
{
    public class InstallationKey
    {
        public int Id { get; set; }
        public string? Key { get; set; }
        public InstallationKeyType Type { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public int TenantId { get; set; }
        [JsonIgnore]
        public Tenant? Tenant { get; set; }
    }

    public enum InstallationKeyType
    {
        Server,
        Device,
        Uninstall
    }
}
