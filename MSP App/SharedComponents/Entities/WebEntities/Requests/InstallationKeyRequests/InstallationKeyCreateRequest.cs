using SharedComponents.Entities.DbEntities;

namespace SharedComponents.Entities.WebEntities.Requests.InstallationKeyRequests
{
    public class InstallationKeyCreateRequest
    {
        public int TenantId { get; set; }
        public InstallationKeyType? Type { get; set; }
    }
}
