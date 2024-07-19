
namespace SharedComponents.Entities
{
    public class NASServer
    {
        public int Id { get; set; }
        public int BackupServerId { get; set; }
        public string? Name { get; set; }
        public string? Path { get; set; }
        public int TenantId { get; set; }
        [System.Text.Json.Serialization.JsonIgnore]
        public Tenant? Tenant { get; set; }
        public ICollection<DeviceBackup>? Backups { get; set; }
    }
}
