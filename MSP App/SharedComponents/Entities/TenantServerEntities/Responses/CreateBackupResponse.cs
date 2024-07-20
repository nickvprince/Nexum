
namespace SharedComponents.Entities.TenantServerEntities.Responses
{
    public class CreateBackupResponse
    {
        public string? NASServerName { get; set; }
        public string? Filename { get; set; }
        public string? Path { get; set; }
        public DateTime? Date { get; set; }
    }
}
