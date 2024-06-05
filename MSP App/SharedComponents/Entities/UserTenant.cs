using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedComponents.Entities
{
    public class UserTenant
    {
        public string UserId { get; set; }
        public User? User { get; set; }
        public int TenantId { get; set; }
        public Tenant? Tenant { get; set; }
    }
}
