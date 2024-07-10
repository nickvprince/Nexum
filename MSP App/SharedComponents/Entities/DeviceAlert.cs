using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SharedComponents.Entities
{
    public class DeviceAlert
    {
        public int Id { get; set; }
        public AlertSeverity Severity { get; set; }
        public string? Message { get; set; }
        public DateTime Time { get; set; }
        public bool Acknowledged { get; set; }
        public bool IsDeleted { get; set; }
        public int DeviceId { get; set; }
        [JsonIgnore]
        public Device? Device { get; set; }
    }

    public enum AlertSeverity
    {
        Information,
        Low,
        Medium,
        High,
        Critical
    }
}
