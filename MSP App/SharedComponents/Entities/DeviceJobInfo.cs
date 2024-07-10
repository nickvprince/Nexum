using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace SharedComponents.Entities
{
    public class DeviceJobInfo
    {
        public int Id { get; set; }
        public int? BackupServerId { get; set; }
        public DeviceJobType Type { get; set; }
        private DateTime? _startTime { get; set; }
        private DateTime? _endTime { get; set; }
        public DeviceJobSchedule? Schedule { get; set; }
        public int UpdateInterval { get; set; }
        public int retryCount { get; set; }
        public bool Sampling { get; set; }
        public int Retention { get; set; }
        [Required]
        public int DeviceJobId { get; set; }
        [JsonIgnore]
        public DeviceJob? DeviceJob { get; set; }

        // Public accessor for StartTime that only handles hours and minutes
        public string? StartTime
        {
            get => _startTime?.ToString("HH:mm") ?? null;
            set
            {
                if (DateTime.TryParseExact(value, "HH:mm", null, System.Globalization.DateTimeStyles.None, out var parsedTime))
                {
                    _startTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day,
                                              parsedTime.Hour, parsedTime.Minute, 0);
                }
                else
                {
                    throw new ArgumentException("Invalid time format. Please use HH:mm.");
                }
            }
        }

        // Public accessor for EndTime that only handles hours and minutes
        public string? EndTime
        {
            get => _endTime?.ToString("HH:mm") ?? null;
            set
            {
                if (DateTime.TryParseExact(value, "HH:mm", null, System.Globalization.DateTimeStyles.None, out var parsedTime))
                {
                    _endTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day,
                                            parsedTime.Hour, parsedTime.Minute, 0);
                }
                else
                {
                    throw new ArgumentException("Invalid time format. Please use HH:mm.");
                }
            }
        }
    }

    public enum DeviceJobType
    {
        Backup,
        Restore
    }
}
