using SharedComponents.Entities;
using SharedComponents.WebRequestEntities.InstallationKeyRequests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedComponents.Services
{
    public interface IInstallationKeyService
    {
        public Task<InstallationKey?> CreateAsync(InstallationKeyCreateRequest request);
        public Task<InstallationKey?> UpdateAsync(InstallationKeyUpdateRequest request);
        public Task<bool> DeleteAsync(int id);
        public Task<InstallationKey?> GetAsync(int id);
        public Task<ICollection<InstallationKey>?> GetAllAsync();
        public Task<ICollection<InstallationKey>?> GetAllByTenantIdAsync(int tenantId);
    }
}
