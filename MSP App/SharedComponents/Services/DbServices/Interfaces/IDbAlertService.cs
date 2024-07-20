using SharedComponents.Entities.DbEntities;

namespace SharedComponents.Services.DbServices.Interfaces
{
    public interface IDbAlertService
    {
        public Task<DeviceAlert?> CreateAsync(DeviceAlert? alert);
        public Task<DeviceAlert?> UpdateAsync(DeviceAlert? alert);
        public Task<bool> DeleteAsync(int id);
        public Task<DeviceAlert?> GetAsync(int id);
        public Task<ICollection<DeviceAlert>?> GetAllAsync();
        public Task<ICollection<DeviceAlert>?> GetAllByDeviceIdAsync(int deviceId);
        public Task<ICollection<DeviceAlert>?> GetAllByTenantIdAsync(int tenantId);
    }
}
