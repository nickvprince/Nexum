
namespace SharedComponents.Entities.TenantServerEntities.Responses
{
    public class CheckForUpdatesResponse
    {
        public bool NexumUpdateAvailable { get; set; }
        public bool NexumServerUpdateAvailable { get; set; }
        public bool NexumServiceUpdateAvailable { get; set; }
    }
}
