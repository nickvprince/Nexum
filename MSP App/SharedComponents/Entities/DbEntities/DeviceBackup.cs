
namespace SharedComponents.Entities.DbEntities
{
    public class DeviceBackup
    {
        public int Id { get; set; }
        public string? Filename { get; set; }
        public string? Path { get; set; }
        public DateTime? Date { get; set; }
        public int Client_Id { get; set; }
        public string? Uuid { get; set; }
        public int TenantId { get; set; }
        public int NASServerId { get; set; }
        [System.Text.Json.Serialization.JsonIgnore]
        public NASServer? NASServer { get; set; }
    }
}
