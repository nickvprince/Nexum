using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
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
        public bool IsActive { get; set; }
        [JsonIgnore]
        public ICollection<ApplicationRolePermission>? RolePermissions { get; set; }
        [JsonIgnore]
        public ICollection<ApplicationUserRole>? UserRoles { get; set; }
    }
}