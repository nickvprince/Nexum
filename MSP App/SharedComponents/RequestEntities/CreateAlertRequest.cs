using SharedComponents.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedComponents.RequestEntities
{
    public class CreateAlertRequest
    {
        public int Client_Id { get; set; }
        public string? Uuid { get; set; }
        public AlertSeverity Severity { get; set; }
        public string? Message { get; set; }
        public DateTime Time { get; set; }
    }
}
