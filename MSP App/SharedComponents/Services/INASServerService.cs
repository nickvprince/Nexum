using SharedComponents.Entities;
using SharedComponents.WebRequestEntities.NASServerRequests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedComponents.Services
{
    public interface INASServerService
    {
        public Task<NASServer?> CreateAsync(NASServerCreateRequest request);
        public Task<NASServer?> UpdateAsync(NASServerUpdateRequest request);
        public Task<bool> DeleteAsync(int id);
        public Task<NASServer?> GetAsync(int id);
        public Task<ICollection<NASServer>?> GetAllAsync();
        public Task<ICollection<NASServer>?> GetAllByTenantIdAsync(int tenantId);
    }
}
