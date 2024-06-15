using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedComponents.Entities
{
    public class DeviceInfo
    {
        public int Id { get; set; }
        public int ClientId { get; set; }
        public string? Uuid { get; set; }
        public string? IpAddress { get; set; }
        public int? Port { get; set; }
        public string? Type { get; set; }
        public ICollection<string>? MacAddresses { get; set; }
        public Device? Device { get; set; }
    }
}
