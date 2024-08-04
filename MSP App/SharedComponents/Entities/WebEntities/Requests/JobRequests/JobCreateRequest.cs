using System.ComponentModel.DataAnnotations;

namespace SharedComponents.Entities.WebEntities.Requests.JobRequests
{
    public class JobCreateRequest
    {
        [Required(ErrorMessage = "DeviceId is Required")]
        public int DeviceId { get; set; }
        [Required(ErrorMessage = "Name is Required")]
        public string? Name { get; set; }
        public JobInfoRequest? Settings { get; set; }
    }
}
