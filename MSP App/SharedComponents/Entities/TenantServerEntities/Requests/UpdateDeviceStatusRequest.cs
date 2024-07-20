using SharedComponents.Entities.DbEntities;

namespace SharedComponents.Entities.TenantServerEntities.Requests
{
    public class UpdateDeviceStatusRequest
    {
        public int Client_Id { get; set; }
        public string? Uuid { get; set; }
        public DeviceStatus Status { get; set; }
    }
}
