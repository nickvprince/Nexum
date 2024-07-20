
namespace SharedComponents.Entities.DbEntities
{
    public class DeviceLog
    {
        public int Id { get; set; }
        public string? Filename { get; set; }
        public string? Function { get; set; }
        public string? Message { get; set; }
        public int Code { get; set; }
        public string? Stack_Trace { get; set; }
        public DateTime Time { get; set; }
        public LogType Type { get; set; }
        public bool Acknowledged { get; set; }
        public bool IsDeleted { get; set; }
        public int DeviceId { get; set; }
        [System.Text.Json.Serialization.JsonIgnore]
        public Device? Device { get; set; }
    }

    public enum LogType
    {
        Trace,
        Debug,
        Information,
        Warning,
        Error,
        Critical
    }
}
