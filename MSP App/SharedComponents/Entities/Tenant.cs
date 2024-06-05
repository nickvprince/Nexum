using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace SharedComponents.Entities
{
    public class Tenant
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public int ContactInfoId { get; set; }
        public ContactInfo? ContactInfo { get; set; }
        public string? ApiKey { get; set; }
        public bool IsActive { get; set; }
        public ICollection<UserTenant>? UserTenants { get; set; }
        public ICollection<Device>? Devices { get; set; }
    }
}
