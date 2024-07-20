using SharedComponents.Entities.DbEntities;

namespace SharedComponents.Entities.WebEntities.Requests.InstallationKeyRequests
{
    public class InstallationKeyUpdateRequest
    {
        public int Id { get; set; }
        public InstallationKeyType? Type { get; set; }
        public bool IsActive { get; set; }
    }
}
