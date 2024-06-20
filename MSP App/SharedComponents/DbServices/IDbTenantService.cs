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
        public Task<Tenant?> CreateAsync(Tenant? tenant);
        public Task<Tenant?> UpdateAsync(Tenant? tenant);
        public Task<bool> DeleteAsync(int id);
        public Task<Tenant?> GetAsync(int id);
        public Task<Tenant?> GetByApiKeyAsync(string? apikey);
        public Task<ICollection<Tenant>> GetAllAsync();
        public Task<InstallationKey?> CreateInstallationKeyAsync(int tenantId);
        public Task<InstallationKey?> UpdateInstallationKeyAsync(InstallationKey? installationkey);
        public Task<bool> DeleteInstallationKeyAsync(string? installationkey);
        public Task<InstallationKey?> GetInstallationKeyAsync(string? installationkey);
    }
}
