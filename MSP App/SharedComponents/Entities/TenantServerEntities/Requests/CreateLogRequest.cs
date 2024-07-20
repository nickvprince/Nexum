using SharedComponents.Entities.DbEntities;

namespace SharedComponents.Entities.TenantServerEntities.Requests
{
    public class CreateLogRequest
    {
        public int Client_Id { get; set; }
        public string? Uuid { get; set; }
        public string? Filename { get; set; }
        public string? Function { get; set; }
        public string? Message { get; set; }
        public int Code { get; set; }
        public string? Stack_Trace { get; set; }
        public DateTime Time { get; set; }
        public LogType Type { get; set; }
    }
}
