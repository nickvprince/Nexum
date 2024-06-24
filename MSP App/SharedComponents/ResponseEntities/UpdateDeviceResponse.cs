using SharedComponents.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedComponents.ResponseEntities
{
    public class UpdateDeviceResponse
    {
        public string? Name { get; set; }
        public string? IpAddress { get; set; }
        public int Port { get; set; }
        public ICollection<MACAddressResponse>? MACAddresses { get; set; }
        public string? Type { get; set; }
    }
}
