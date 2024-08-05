
using System.ComponentModel.DataAnnotations;

namespace SharedComponents.Entities.WebEntities.Requests.JobRequests
{

    public class JobUpdateRequest
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Name is Required")]
        public string? Name { get; set; }
        public JobInfoRequest? Settings { get; set; }
    }
}
