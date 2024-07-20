using SharedComponents.Entities.DbEntities;
using SharedComponents.Entities.WebEntities.Requests.InstallationKeyRequests;

namespace SharedComponents.Services.APIRequestServices.Interfaces
{
    public interface IAPIRequestInstallationKeyService
    {
        public Task<InstallationKey?> CreateAsync(InstallationKeyCreateRequest request);
        public Task<InstallationKey?> UpdateAsync(InstallationKeyUpdateRequest request);
        public Task<bool> DeleteAsync(int id);
        public Task<InstallationKey?> GetAsync(int id);
        public Task<ICollection<InstallationKey>?> GetAllAsync();
        public Task<ICollection<InstallationKey>?> GetAllByTenantIdAsync(int tenantId);
    }
}
