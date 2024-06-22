using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        public int DeviceId { get; set; }
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
