using SharedComponents.Entities.DbEntities;

namespace SharedComponents.Services.DbServices.Interfaces
{
    public interface IDbBackupService
    {
        public Task<DeviceBackup?> CreateAsync(DeviceBackup? backup);
        public Task<DeviceBackup?> UpdateAsync(DeviceBackup? backup);
        public Task<bool> DeleteAsync(int id);
        public Task<DeviceBackup?> GetAsync(int id);
        public Task<ICollection<DeviceBackup>?> GetAllAsync();
        public Task<ICollection<DeviceBackup>?> GetAllByClientIdAndUuidAsync(int tenantId, int clientId, string? Uuid);
        public Task<ICollection<DeviceBackup>?> GetAllByTenantIdAsync(int tenantId);
    }
}
