using SharedComponents.Entities.DbEntities;

namespace SharedComponents.Entities.WebEntities.Requests.AlertRequests
{
    public class AlertUpdateRequest
    {
        public int Id { get; set; }
        public AlertSeverity Severity { get; set; }
        public string? Message { get; set; }
    }
}
