using SharedComponents.Entities.DbEntities;
using SharedComponents.Entities.WebEntities.Requests.BackupRequests;

namespace SharedComponents.Services.APIRequestServices.Interfaces
{
    public interface IAPIRequestBackupService
    {
        public Task<DeviceBackup?> CreateAsync(BackupCreateRequest request);
        public Task<DeviceBackup?> UpdateAsync(BackupUpdateRequest request);
        public Task<bool> DeleteAsync(int id);
        public Task<DeviceBackup?> GetAsync(int id);
        public Task<ICollection<DeviceBackup>?> GetAllAsync();
        public Task<ICollection<DeviceBackup>?> GetAllByDeviceIdAsync(int deviceId);
        public Task<ICollection<DeviceBackup>?> GetAllByTenantIdAsync(int tenantId);
    }
}
