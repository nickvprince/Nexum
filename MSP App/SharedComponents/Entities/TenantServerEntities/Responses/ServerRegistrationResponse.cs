
namespace SharedComponents.Entities.TenantServerEntities.Responses
{
    public class ServerRegistrationResponse
    {
        public string? Name { get; set; }
        public int Client_Id { get; set; }
        public string? Uuid { get; set; }
        public string? IpAddress { get; set; }
        public int? Port { get; set; }
        public string? Type { get; set; }
        public ICollection<MACAddressResponse>? MACAddresses { get; set; }
        public bool IsVerified { get; set; }
    }
}
