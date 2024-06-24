using SharedComponents.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedComponents.RequestEntities
{
    public class UpdateDeviceStatusRequest
    {
        public int Client_Id { get; set; }
        public string? Uuid { get; set; }
        public DeviceStatus Status { get; set; }
    }
}
