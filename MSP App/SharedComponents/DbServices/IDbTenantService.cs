using SharedComponents.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedComponents.DbServices
{
    public interface IDbTenantService
    {
        public Task<bool> CreateAsync(Tenant tenant);
        public Task<bool> EditAsync(Tenant tenant);
        public Task<bool> DeleteAsync(int id);
        public Task<Tenant> GetAsync(int id);
        public Task<ICollection<Tenant>> GetAllAsync();
    }
}
