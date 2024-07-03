using SharedComponents.Entities;
using SharedComponents.ResponseEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedComponents.WebRequestEntities.DeviceRequests
{
    public class DeviceCreateRequest
    {
        public int TenantId { get; set; }
        public string? Name { get; set; }
        public int ClientId { get; set; }
        public string? Uuid { get; set; }
        public string? IpAddress { get; set; }
        public int Port { get; set; }
        public string? ApiBaseUrl { get; set; }
        public int? ApiBasePort { get; set; }
        public DeviceType? Type { get; set; }
        public ICollection<string>? MACAddresses { get; set; }
        public string? InstallationKey { get; set; }
    }
}
