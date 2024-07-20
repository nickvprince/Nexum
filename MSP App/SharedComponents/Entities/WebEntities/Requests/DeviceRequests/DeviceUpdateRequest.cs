using SharedComponents.Entities.DbEntities;

namespace SharedComponents.Entities.WebEntities.Requests.DeviceRequests
{
    public class DeviceUpdateRequest
    {
        public int Id { get; set; }
        public string? Nickname { get; set; }
        public string? Name { get; set; }
        public string? IpAddress { get; set; }
        public int Port { get; set; }
        public string? ApiBaseUrl { get; set; }
        public int? ApiBasePort { get; set; }
        public ICollection<string>? MACAddresses { get; set; }
        public DeviceType? Type { get; set; }
    }
}
