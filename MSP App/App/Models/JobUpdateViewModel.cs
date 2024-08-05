using SharedComponents.Entities.DbEntities;
using SharedComponents.Entities.WebEntities.Requests.JobRequests;

namespace App.Models
{
    public class JobUpdateViewModel
    {
        public ICollection<NASServer>? NASServers { get; set; }
        public JobUpdateRequest? JobUpdateRequest { get; set; }
    }
}
