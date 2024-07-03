using SharedComponents.Entities;
using SharedComponents.WebRequestEntities.TenantRequests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedComponents.Services
{
    public interface ITenantService
    {
        public Task<Tenant?> CreateAsync(TenantCreateRequest request);
        public Task<Tenant?> UpdateAsync(TenantUpdateRequest request);
        public Task<bool> DeleteAsync(int id);
        public Task<Tenant?> GetAsync(int id);
        public Task<Tenant?> GetRichAsync(int id);
        public Task<ICollection<Tenant>?> GetAllAsync();
    }
}
