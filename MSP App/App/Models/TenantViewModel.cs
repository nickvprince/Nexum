using SharedComponents.Entities;

namespace App.Models
{
    public class TenantViewModel
    {
        public ICollection<Tenant>? Tenants { get; set; }
    }
}
