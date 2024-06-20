using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedComponents.Entities
{
    public class ApplicationRole
    {
        public string? Id { get; set; } = Guid.NewGuid().ToString();
        [Required]
        public string? Name { get; set; }
        [Required]
        public string? Description { get; set; }
        public ICollection<ApplicationRolePermission>? RolePermissions { get; set; }
        public ICollection<ApplicationUserRole>? UserRoles { get; set; }
    }
}