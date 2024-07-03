using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedComponents.WebRequestEntities.RoleRequests
{
    public class RoleAssignPermissionRequest
    {
        public string? RoleId { get; set; }
        public int PermissionId { get; set; }
        public int TenantId { get; set; }
    }
}
