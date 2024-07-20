
namespace SharedComponents.Entities.TenantServerEntities.Responses
{
    public class UpdateDeviceResponse
    {
        public string? Name { get; set; }
        public string? IpAddress { get; set; }
        public int Port { get; set; }
        public ICollection<MACAddressResponse>? MACAddresses { get; set; }
        public string? Type { get; set; }
    }
}
