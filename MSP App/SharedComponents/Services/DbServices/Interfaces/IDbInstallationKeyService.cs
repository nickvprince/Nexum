using SharedComponents.Entities.DbEntities;

namespace SharedComponents.Services.DbServices.Interfaces
{
    public interface IDbInstallationKeyService
    {
        public Task<InstallationKey?> CreateAsync(InstallationKey? installationkey);
        public Task<InstallationKey?> UpdateAsync(InstallationKey? installationkey);
        public Task<bool> DeleteAsync(int id);
        public Task<InstallationKey?> GetAsync(int id);
        public Task<InstallationKey?> GetByInstallationKeyAsync(string? installationkey);
        public Task<ICollection<InstallationKey>?> GetAllAsync();
        public Task<ICollection<InstallationKey>?> GetAllByTenantIdAsync(int tenantId);
    }
}
