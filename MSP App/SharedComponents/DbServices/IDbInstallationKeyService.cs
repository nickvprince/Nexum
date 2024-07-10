using SharedComponents.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedComponents.DbServices
{
    public interface IDbInstallationKeyService
    {
        public Task<InstallationKey?> CreateAsync(int tenantId);
        public Task<InstallationKey?> UpdateAsync(InstallationKey? installationkey);
        public Task<bool> DeleteAsync(int id);
        public Task<InstallationKey?> GetAsync(int id);
        public Task<InstallationKey?> GetByInstallationKeyAsync(string? installationkey);
        public Task<ICollection<InstallationKey>?> GetAllAsync();
        public Task<ICollection<InstallationKey>?> GetAllByTenantIdAsync(int tenantId);
    }
}
