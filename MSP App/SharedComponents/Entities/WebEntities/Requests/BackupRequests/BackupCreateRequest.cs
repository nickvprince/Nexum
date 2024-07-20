
namespace SharedComponents.Entities.WebEntities.Requests.BackupRequests
{
    public class BackupCreateRequest
    {
        public int DeviceId { get; set; }
        public string? Name { get; set; }
        public string? Path { get; set; }
        public DateTime? Date { get; set; }
        public int NASServerId { get; set; }
    }
}
