using SharedComponents.Entities.DbEntities;

namespace SharedComponents.Services.DbServices.Interfaces
{
    public interface IDbJobService
    {
        public Task<DeviceJob?> CreateAsync(DeviceJob? job);
        public Task<DeviceJob?> UpdateAsync(DeviceJob? job);
        public Task<bool> DeleteAsync(int id);
        public Task<DeviceJob?> GetAsync(int id);
        public Task<ICollection<DeviceJob>?> GetAllAsync();
        public Task<ICollection<DeviceJob>?> GetAllByDeviceIdAsync(int deviceId);
        public Task<ICollection<DeviceJob>?> GetAllByTenantIdAsync(int tenantId);
        public Task<ICollection<DeviceJob>?> GetAllByBackupServerIdAsync(int tenantId, int backupServerId);
    }
}
