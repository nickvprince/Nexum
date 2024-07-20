
namespace SharedComponents.Entities.TenantServerEntities.Requests
{
    public class VerifyInstallationRequest
    {
        public int Client_Id { get; set; }
        public string? Uuid { get; set; }
        public string? InstallationKey { get; set; }
    }
}
