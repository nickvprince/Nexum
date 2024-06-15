﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedComponents.Entities
{
    public class Permission
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public int TenantId { get; set; }
        public Tenant? Tenant { get; set; }
        public ICollection<ApplicationRolePermission>? RolePermissions { get; set; }
    }
}
