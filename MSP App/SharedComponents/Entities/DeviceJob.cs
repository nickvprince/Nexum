using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace SharedComponents.Entities
{
    public class DeviceJob
    {
        public int Id { get; set; }
        public int JobId { get; set; }
        public string? Name { get; set; }
        public DeviceJobInfo? Settings { get; set; }
        public DeviceJobStatus Status { get; set; }
        public int? Progress { get; set; }
        [Required]
        public int? DeviceId { get; set; }
        [JsonIgnore]
        public Device? Device { get; set; }
    }

    public enum DeviceJobStatus
    {
        NotStarted,
        InProgress,
        Complete,
        Failed,
        Restarting
    }
}
