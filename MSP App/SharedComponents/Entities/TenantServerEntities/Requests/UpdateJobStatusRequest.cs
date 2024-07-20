using SharedComponents.Entities.DbEntities;

namespace SharedComponents.Entities.TenantServerEntities.Requests
{
    public class UpdateJobStatusRequest
    {
        public int Client_Id { get; set; }
        public string? Uuid { get; set; }
        public DeviceJobStatus Status { get; set; }
        public int? Progress { get; set; }
    }
}
