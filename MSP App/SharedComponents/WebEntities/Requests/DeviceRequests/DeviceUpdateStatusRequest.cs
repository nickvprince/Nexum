using SharedComponents.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedComponents.WebEntities.Requests.DeviceRequests
{
    public class DeviceUpdateStatusRequest
    {
        public int Id { get; set; }
        public DeviceStatus Status { get; set; }
    }
}
