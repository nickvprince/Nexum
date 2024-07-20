using SharedComponents.Entities.DbEntities;

namespace SharedComponents.Entities.WebEntities.Requests.LogRequests
{
    public class LogUpdateRequest
    {
        public int Id { get; set; }
        public string? Filename { get; set; }
        public string? Function { get; set; }
        public string? Message { get; set; }
        public int Code { get; set; }
        public string? Stack_Trace { get; set; }
        public LogType Type { get; set; }
    }
}
