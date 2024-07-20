using SharedComponents.Entities.DbEntities;

namespace SharedComponents.Entities.WebEntities.Requests.JobRequests
{
    public class JobUpdateStatusRequest
    {
        public int Id { get; set; }
        public DeviceJobStatus Status { get; set; }
        public int? Progress { get; set; }
    }
}
