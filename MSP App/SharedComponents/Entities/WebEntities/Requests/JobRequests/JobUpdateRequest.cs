
namespace SharedComponents.Entities.WebEntities.Requests.JobRequests
{

    public class JobUpdateRequest
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public JobInfoRequest? Settings { get; set; }
    }
}
