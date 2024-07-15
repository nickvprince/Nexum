using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SharedComponents.Entities
{
    public class InstallationKey
    {
        public int Id { get; set; }
        public string? Key { get; set; }
        public int TenantId { get; set; }
        public InstallationKeyType Type { get; set; }
        [JsonIgnore]
        public Tenant? Tenant { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
    }

    public enum InstallationKeyType
    {
        Server,
        Device,
        Uninstall
    }
}
