using SharedComponents.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedComponents.WebRequestEntities.JobRequests
{
    public class JobInfoRequest
    {
        public int BackupServerId { get; set; }
        public DeviceJobType? Type { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public DeviceJobSchedule? Schedule { get; set; }
    }
}
