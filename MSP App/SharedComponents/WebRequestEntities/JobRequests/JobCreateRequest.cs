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
    public class JobInfoRequest
    {
        public int BackupServerId { get; set; }
        public DeviceJobType? Type { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public DeviceJobSchedule? Schedule { get; set; }
    }

    public class JobScheduleRequest
    {
        public bool Sunday { get; set; }
        public bool Monday { get; set; }
        public bool Tuesday { get; set; }
        public bool Wednesday { get; set; }
        public bool Thursday { get; set; }
        public bool Friday { get; set; }
        public bool Saturday { get; set; }
    }
}
