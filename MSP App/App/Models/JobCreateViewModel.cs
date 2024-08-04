using SharedComponents.Entities.DbEntities;
using SharedComponents.Entities.WebEntities.Requests.JobRequests;

namespace App.Models
{
    public class JobCreateViewModel
    {
        public ICollection<NASServer>? NASServers { get; set; }
        public JobCreateRequest? JobCreateRequest { get; set; }
    }
}
