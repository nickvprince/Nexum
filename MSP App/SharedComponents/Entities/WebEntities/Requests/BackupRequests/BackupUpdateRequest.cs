
namespace SharedComponents.Entities.WebEntities.Requests.BackupRequests
{
    public class BackupUpdateRequest
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Path { get; set; }
        public DateTime? Date { get; set; }
    }
}