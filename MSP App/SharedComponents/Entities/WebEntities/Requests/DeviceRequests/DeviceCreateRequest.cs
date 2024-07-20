using SharedComponents.Entities.DbEntities;

namespace SharedComponents.Entities.WebEntities.Requests.DeviceRequests
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
