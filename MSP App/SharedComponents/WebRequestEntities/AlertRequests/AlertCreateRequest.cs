using SharedComponents.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedComponents.WebRequestEntities.AlertRequests
{
    public class AlertCreateRequest
    {
        public int DeviceId { get; set; }
        public AlertSeverity Severity { get; set; }
        public string? Message { get; set; }
        public DateTime Time { get; set; }
    }
}
