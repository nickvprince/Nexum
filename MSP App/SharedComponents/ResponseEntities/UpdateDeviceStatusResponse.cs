using SharedComponents.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedComponents.ResponseEntities
{
    public class UpdateDeviceStatusResponse
    {
        public string? Name { get; set; }
        public DeviceStatus? Status { get; set; }
        public string? StatusMessage { get; set; }
    }
}
