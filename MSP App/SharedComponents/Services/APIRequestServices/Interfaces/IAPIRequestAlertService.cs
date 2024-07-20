using SharedComponents.Entities.DbEntities;
using SharedComponents.Entities.WebEntities.Requests.AlertRequests;

namespace SharedComponents.Services.APIRequestServices.Interfaces
{
    public interface IAPIRequestAlertService
    {
        public Task<DeviceAlert?> CreateAsync(AlertCreateRequest request);
        public Task<DeviceAlert?> UpdateAsync(AlertUpdateRequest request);
        public Task<DeviceAlert?> AcknowledgeAsync(int id);
        public Task<bool> DeleteAsync(int id);
        public Task<DeviceAlert?> GetAsync(int id);
        public Task<ICollection<DeviceAlert>?> GetAllAsync();
        public Task<ICollection<DeviceAlert>?> GetAllByDeviceIdAsync(int deviceId);
        public Task<ICollection<DeviceAlert>?> GetAllByTenantIdAsync(int tenantId);
    }
}
