using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedComponents.Entities
{
    public class DeviceJobInfo
    {
        public int Id { get; set; }
        public int BackupServerId { get; set; }
        public DeviceJobType Type { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public DeviceJobSchedule? Schedule { get; set; }
        public int UpdateInterval { get; set; }
        public bool Sampling { get; set; }
        public int Retention { get; set; }
        [Required]
        public int DeviceJobId { get; set; }
        [JsonIgnore]
        public DeviceJob? DeviceJob { get; set; }
        public int NASServerId { get; set; }
        [JsonIgnore]
        public NASServer? NASServer { get; set; }
    }

    public enum DeviceJobType
    {
        Backup,
        Restore
    }
}
