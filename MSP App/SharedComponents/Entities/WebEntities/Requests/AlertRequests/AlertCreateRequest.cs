using SharedComponents.Entities.DbEntities;

namespace SharedComponents.Entities.WebEntities.Requests.AlertRequests
{
    public class AlertCreateRequest
    {
        public int DeviceId { get; set; }
        public AlertSeverity Severity { get; set; }
        public string? Message { get; set; }
        public DateTime Time { get; set; }
    }
}
