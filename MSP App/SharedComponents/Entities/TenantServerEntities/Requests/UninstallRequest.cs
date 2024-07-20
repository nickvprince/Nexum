
namespace SharedComponents.Entities.TenantServerEntities.Requests
{
    public class UninstallRequest
    {
        public int Client_Id { get; set; }
        public string? Uuid { get; set; }
        public string? UninstallationKey { get; set; }
    }
}
