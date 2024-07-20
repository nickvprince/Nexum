using SharedComponents.Entities.DbEntities;

namespace SharedComponents.Entities.TenantServerEntities.Requests
{
    public class UpdateDeviceRequest
    {
        public string? Name { get; set; }
        public int Client_Id { get; set; }
        public string? Uuid { get; set; }
        public string? IpAddress { get; set; }
        public int Port { get; set; }
        public ICollection<MACAddress>? MACAddresses { get; set; }
    }
}
