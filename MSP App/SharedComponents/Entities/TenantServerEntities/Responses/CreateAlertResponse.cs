
namespace SharedComponents.Entities.TenantServerEntities.Responses
{
    public class CreateAlertResponse
    {
        public string? Name { get; set; }
        public string? Severity { get; set; }
        public string? Message { get; set; }
        public DateTime? Time { get; set; }
    }
}
