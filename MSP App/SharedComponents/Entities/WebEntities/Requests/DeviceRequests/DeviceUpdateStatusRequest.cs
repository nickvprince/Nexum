using SharedComponents.Entities.DbEntities;

namespace SharedComponents.Entities.WebEntities.Requests.DeviceRequests
{
    public class DeviceUpdateStatusRequest
    {
        public int Id { get; set; }
        public DeviceStatus Status { get; set; }
    }
}
