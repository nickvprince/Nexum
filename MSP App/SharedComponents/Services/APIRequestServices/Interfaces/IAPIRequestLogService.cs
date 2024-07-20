using SharedComponents.Entities.DbEntities;
using SharedComponents.Entities.WebEntities.Requests.LogRequests;

namespace SharedComponents.Services.APIRequestServices.Interfaces
{
    public interface IAPIRequestLogService
    {
        public Task<DeviceLog?> CreateAsync(LogCreateRequest request);
        public Task<DeviceLog?> UpdateAsync(LogUpdateRequest request);
        public Task<DeviceLog?> AcknowledgeAsync(int id);
        public Task<bool> DeleteAsync(int id);
        public Task<DeviceLog?> GetAsync(int id);
        public Task<ICollection<DeviceLog>?> GetAllAsync();
        public Task<ICollection<DeviceLog>?> GetAllByDeviceIdAsync(int deviceId);
        public Task<ICollection<DeviceLog>?> GetAllByTenantIdAsync(int tenantId);
    }
}
