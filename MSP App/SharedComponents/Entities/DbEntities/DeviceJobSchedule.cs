using System.ComponentModel.DataAnnotations;

namespace SharedComponents.Entities.DbEntities
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
        [System.Text.Json.Serialization.JsonIgnore]
        public DeviceJobInfo? DeviceJobInfo { get; set; }
    }
}
