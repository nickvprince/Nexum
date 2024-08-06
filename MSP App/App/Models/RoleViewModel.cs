using SharedComponents.Entities.DbEntities;

namespace App.Models
{
    public class RoleViewModel
    {
        public ICollection<Tenant>? Tenants { get; set; }
        public ICollection<ApplicationRole>? Roles { get; set; }
    }
}
