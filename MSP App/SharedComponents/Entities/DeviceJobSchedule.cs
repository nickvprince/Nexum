using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace SharedComponents.Entities
{
    public class DeviceJobSchedule
    {
        public int Id { get; set; }
        public bool Sunday { get; set; }
        public bool Monday { get; set; }
        public bool Tuesday { get; set; }
        public bool Wednesday { get; set; }
        public bool Thursday { get; set; }
        public bool Friday { get; set; }
        public bool Saturday { get; set; }
        [Required]
        public int DeviceJobInfoId { get; set; }
        [JsonIgnore]
        public DeviceJobInfo? DeviceJobInfo { get; set; }
    }
}
