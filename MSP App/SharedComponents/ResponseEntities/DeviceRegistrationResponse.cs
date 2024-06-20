using SharedComponents.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedComponents.ResponseEntities
{
    public class DeviceRegistrationResponse
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public int Client_Id { get; set; }
        public string? Uuid { get; set; }
        public string? IpAddress { get; set; }
        public int? Port { get; set; }
        public DeviceType? Type { get; set; }
        public ICollection<MACAddressResponse>? MACAddresses { get; set; }
        public bool IsVerified { get; set; }
    }
}
