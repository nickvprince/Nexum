using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SharedComponents.Entities
{
    public class NASServer
    {
        public int Id { get; set; }
        public int BackupServerId { get; set; }
        public string? Name { get; set; }
        public string? Path { get; set; }
        public int TenantId { get; set; }
        [JsonIgnore]
        public Tenant? Tenant { get; set; }
    }
}
