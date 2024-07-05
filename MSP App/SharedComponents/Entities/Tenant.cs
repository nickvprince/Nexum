using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
        [Required]
        public TenantInfo? TenantInfo { get; set; }
        public string? ApiKey { get; set; }
        public bool IsActive { get; set; }
        public ICollection<InstallationKey>? InstallationKeys { get; set; }
        public ICollection<Device>? Devices { get; set; }
        public ICollection<ApplicationRolePermission>? RolePermissions { get; set; }
    }
}
