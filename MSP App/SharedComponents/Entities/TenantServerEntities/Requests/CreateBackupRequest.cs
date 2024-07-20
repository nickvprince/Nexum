
namespace SharedComponents.Entities.TenantServerEntities.Requests
{
    public class CreateBackupRequest
    {
        public int Client_Id { get; set; }
        public string? Uuid { get; set; }
        public string? Filename { get; set; }
        public string? Path { get; set; }
        public DateTime? Date { get; set; }
        public int BackupServerId { get; set; }
    }
}
