using SharedComponents.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedComponents.WebRequestEntities.JobRequests
{
    public class JobUpdateStatusRequest
    {
        public int Id { get; set; }
        public DeviceJobStatus Status { get; set; }
        public int? Progress { get; set; }
    }
}
