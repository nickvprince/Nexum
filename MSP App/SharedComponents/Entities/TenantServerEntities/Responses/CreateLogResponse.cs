
namespace SharedComponents.Entities.TenantServerEntities.Responses
{
    public class CreateLogResponse
    {
        public string? Name { get; set; }
        public string? Filename { get; set; }
        public string? Function { get; set; }
        public string? Message { get; set; }
        public int Code { get; set; }
        public string? Stack_Trace { get; set; }
        public DateTime Time { get; set; }
        public string? Type { get; set; }
    }
}
