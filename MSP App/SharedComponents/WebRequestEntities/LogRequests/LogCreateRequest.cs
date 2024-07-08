using SharedComponents.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedComponents.WebRequestEntities.LogRequests
{
    public class LogCreateRequest
    {
        public int DeviceId { get; set; }
        public string? Filename { get; set; }
        public string? Function { get; set; }
        public string? Message { get; set; }
        public int Code { get; set; }
        public string? Stack_Trace { get; set; }
        public DateTime Time { get; set; }
        public LogType Type { get; set; }
    }
}
