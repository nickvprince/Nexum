using SharedComponents.Entities.DbEntities;

namespace App.Models
{
    public class TenantViewModel
    {
        public ICollection<Tenant>? Tenants { get; set; }
    }
}
