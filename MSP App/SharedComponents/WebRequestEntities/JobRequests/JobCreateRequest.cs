using SharedComponents.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedComponents.WebRequestEntities.JobRequests
{
    public class JobCreateRequest
    {
        public int DeviceId { get; set; }
        public string? Name { get; set; }
        public JobInfoRequest? Settings { get; set; }
    }
}
