using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SharedComponents.Entities
{
    public class MACAddress
    {
        public int Id { get; set; }
        [Required]
        public string? Address { get; set; }
        public int DeviceInfoId { get; set; }
        public DeviceInfo? DeviceInfo { get; set; }
    }
}
