using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedComponents.Entities
{
    public class DeviceLog
    {
        public int Id { get; set; }
        public string? Filename { get; set; }
        public string? Function { get; set; }
        public string? Message { get; set; }
        public int Code { get; set; }
        public string? Stack_Trace { get; set; }
        public DateTime Time { get; set; }
        public LogType Type { get; set; }
        public bool IsDeleted { get; set; }
        public int DeviceId { get; set; }
        public Device? Device { get; set; }
    }

    public enum LogType
    {
        Trace,
        Debug,
        Information,
        Warning,
        Error
    }
}
