using SharedComponents.Entities.DbEntities;

namespace SharedComponents.Services.DbServices.Interfaces
{
    public interface IDbDeviceService
    {
        public Task<Device?> CreateAsync(Device? device);
        public Task<Device?> UpdateAsync(Device? device);
        public Task<bool> DeleteAsync(int id);
        public Task<Device?> GetAsync(int id);
        public Task<Device?> GetByClientIdAndUuidAsync(int tenantId, int clientId, string? uuid);
        public Task<ICollection<Device>?> GetAllAsync();
        public Task<ICollection<Device>?> GetAllByTenantIdAsync(int tenantId);
    }
}
