
namespace SharedComponents.Entities.WebEntities.Requests.JobRequests
{
    public class JobCreateRequest
    {
        public int DeviceId { get; set; }
        public string? Name { get; set; }
        public JobInfoRequest? Settings { get; set; }
    }
}
