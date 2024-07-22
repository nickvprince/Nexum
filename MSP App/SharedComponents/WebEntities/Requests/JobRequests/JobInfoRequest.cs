using SharedComponents.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedComponents.WebEntities.Requests.JobRequests
{
    public class JobInfoRequest
    {
        public int BackupServerId { get; set; }
        public DeviceJobType Type { get; set; }
        private DateTime? _startTime { get; set; }
        private DateTime? _endTime { get; set; }
        public JobScheduleRequest? Schedule { get; set; }
        public int UpdateInterval { get; set; }
        public int retryCount { get; set; }
        public bool Sampling { get; set; }
        public int Retention { get; set; }
        // Public accessor for StartTime that only handles hours and minutes
        [Required]
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
        [Required]
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
