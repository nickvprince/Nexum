using SharedComponents.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SharedComponents.RequestEntities.Http
{
    public class CreateJobRequest
    {
        public int Client_Id { get; set; }
        public ICollection<JobRequest>? Jobs { get; set; }
    }

    public class JobRequest
    {
        public string? Title { get; set; }
        public JobSettingsRequest? Settings { get; set; }
    }

    public class JobSettingsRequest
    {
        public int BackupServerId { get; set; }
        public string? Path { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public DayOfWeek[]? Days { get; set; }
        public string? Heartbeat_Interval { get; set; }
        public int Sampling { get; set; }
        public int Retention { get; set; }
    }
}
