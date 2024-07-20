using System.ComponentModel.DataAnnotations;

namespace SharedComponents.Entities.DbEntities
{
    public class MACAddress
    {
        public int Id { get; set; }
        [Required]
        public string? Address { get; set; }
        public int DeviceInfoId { get; set; }
        [System.Text.Json.Serialization.JsonIgnore]
        public DeviceInfo? DeviceInfo { get; set; }
    }
}
