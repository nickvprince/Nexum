using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedComponents.Entities
{
    public class PermissionSet
    {
        public int Id { get; set; }
        public int PermissionId { get; set; }
        public Permission? Permission { get; set; }
        public int TenantId { get; set; }
        public Tenant? Tenant { get; set; }
        public bool IsActive { get; set; }
    }
}
