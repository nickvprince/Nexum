using SharedComponents.Entities.DbEntities;
using System.ComponentModel.DataAnnotations;

namespace SharedComponents.Entities.WebEntities.Requests.JobRequests
{
    public class JobInfoRequest
    {
        public int BackupServerId { get; set; }
        [Required(ErrorMessage = "Type is Required")]
        public DeviceJobType Type { get; set; }
        private DateTime? _startTime { get; set; }
        private DateTime? _endTime { get; set; }
        [Required(ErrorMessage = "Schedule is Required")]
        public JobScheduleRequest? Schedule { get; set; }
        [Required(ErrorMessage = "Update Interval is Required")]
        [Range(1, int.MaxValue, ErrorMessage = "Update Interval must be greater than 0")]
        public int UpdateInterval { get; set; }
        [Required(ErrorMessage = "Retry Count is Required")]
        [Range(0, int.MaxValue, ErrorMessage = "Retry Count must be greater than or equal to 0")]
        public int retryCount { get; set; }
        [Required(ErrorMessage = "Sampling is Required")]
        public bool Sampling { get; set; }
        [Required(ErrorMessage = "Retention is Required")]
        [Range(0, int.MaxValue, ErrorMessage = "Retention must be greater than or equal to 0")]
        public int Retention { get; set; }
        // Public accessor for StartTime that only handles hours and minutes
        [Required(ErrorMessage = "Start Time is Required")]
        [RegularExpression(@"^(?:[01]\d|2[0-3]):[0-5]\d$", ErrorMessage = "Start time must be in the format HH:mm.")]
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
            }
        }

        // Public accessor for EndTime that only handles hours and minutes
        [Required(ErrorMessage = "End Time is Required")]
        [RegularExpression(@"^(?:[01]\d|2[0-3]):[0-5]\d$", ErrorMessage = "Start time must be in the format HH:mm.")]
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
            }
        }
    }
}
