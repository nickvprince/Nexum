using SharedComponents.Entities.DbEntities;

namespace SharedComponents.Services.DbServices.Interfaces
{
    public interface IDbLogService
    {
        public Task<DeviceLog?> CreateAsync(DeviceLog log);
        public Task<DeviceLog?> UpdateAsync(DeviceLog log);
        public Task<bool> DeleteAsync(int id);
        public Task<DeviceLog?> GetAsync(int id);
        public Task<ICollection<DeviceLog>?> GetAllAsync();
        public Task<ICollection<DeviceLog>?> GetAllByDeviceIdAsync(int deviceId);
        public Task<ICollection<DeviceLog>?> GetAllByTenantIdAsync(int tenantId);
    }
}
