using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SharedComponents.Entities
{
    public class ApplicationUserRole
    {
        public string? UserId { get; set; }
        [JsonIgnore]
        public ApplicationUser? User { get; set; }
        public string? RoleId { get; set; }
        [JsonIgnore]
        public ApplicationRole? Role { get; set; }
        public bool IsActive { get; set; }
    }
}
