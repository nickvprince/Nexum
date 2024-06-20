using SharedComponents.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedComponents.RequestEntities
{
    public class DeviceRegistrationRequest
    {
        public string? Name { get; set; }
        public int Client_Id { get; set; }
        public string? Uuid { get; set; }
        public string? IpAddress { get; set; }
        public int Port { get; set; }
        public string? InstallationKey { get; set; }
        public ICollection<MACAddress>? MACAddresses { get; set; }
    }
}
