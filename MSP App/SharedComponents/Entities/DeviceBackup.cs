using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedComponents.Entities
{
    public class DeviceBackup
    {
        public int Id { get; set; }
        public string? Filename { get; set; }
        public string? Path { get; set; }
        public DateTime? Date { get; set; }
        public int DeviceId { get; set; }
        [JsonIgnore]
        public Device? Device { get; set; }
        public int NASServerId { get; set; }
        [JsonIgnore]
        public NASServer? NASServer { get; set; }
    }
}
