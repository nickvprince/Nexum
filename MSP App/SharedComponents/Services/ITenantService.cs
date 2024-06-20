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
        public Task<bool> CreateAsync(Tenant tenant);
        public Task<bool> EditAsync(Tenant tenant);
        public Task<bool> DeleteAsync(string id);
        public Task<Tenant?> GetAsync(string id);
        public Task<ICollection<Tenant>?> GetAllAsync();
    }
}
