
namespace SharedComponents.Entities.TenantServerEntities.Requests
{
    public class CheckForUpdatesRequest
    {
        public string? NexumVersion { get; set; }
        public string? NexumTag { get; set; }
        public string? NexumServerVersion { get; set; }
        public string? NexumServerTag { get; set; }
        public string? NexumServiceVersion { get; set; }
        public string? NexumServiceTag { get; set; }
    }
}
