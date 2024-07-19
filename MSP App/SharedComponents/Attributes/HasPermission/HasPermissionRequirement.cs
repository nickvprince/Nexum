using Microsoft.AspNetCore.Authorization;
using SharedComponents.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedComponents.Attributes.HasPermission
{
    public class HasPermissionRequirement : IAuthorizationRequirement
    {
        public HasPermissionRequirement(string permission, PermissionType type)
        {
            Permission = permission;
            Type = type;
        }

        public string Permission { get; }
        public PermissionType Type { get; }
    }
}
