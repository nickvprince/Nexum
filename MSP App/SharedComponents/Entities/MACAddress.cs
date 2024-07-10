using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace SharedComponents.Entities
{
    public class MACAddress
    {
        public int Id { get; set; }
        [Required]
        public string? Address { get; set; }
        public int DeviceInfoId { get; set; }
        [JsonIgnore]
        public DeviceInfo? DeviceInfo { get; set; }
    }
}
