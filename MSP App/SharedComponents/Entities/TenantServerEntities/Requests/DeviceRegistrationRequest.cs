using SharedComponents.Entities.DbEntities;

namespace SharedComponents.Entities.TenantServerEntities.Requests
{
    public class DeviceRegistrationRequest
    {
        public string? Name { get; set; }
        public int Client_Id { get; set; }
        public string? Uuid { get; set; }
        public string? IpAddress { get; set; }
        public int Port { get; set; }
        public string? InstallationKey { get; set; }
        public string? ApiBaseUrl { get; set; }
        public int? ApiBasePort { get; set; }
        public string? ApiKey { get; set; }
        public DeviceType Type { get; set; }
        public ICollection<MACAddress>? MACAddresses { get; set; }
    }
}
