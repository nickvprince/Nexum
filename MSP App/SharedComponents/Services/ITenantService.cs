using SharedComponents.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedComponents.Services
{
    public interface ITenantService
    {
        public Task<Tenant?> CreateAsync(Tenant tenant);
        public Task<Tenant?> EditAsync(Tenant tenant);
        public Task<bool> DeleteAsync(int id);
        public Task<Tenant?> GetAsync(int id);
        public Task<ICollection<Tenant>?> GetAllAsync();
    }
}
