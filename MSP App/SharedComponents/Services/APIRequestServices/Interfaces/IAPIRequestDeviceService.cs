using SharedComponents.Entities.DbEntities;
using SharedComponents.Entities.WebEntities.Requests.DeviceRequests;

namespace SharedComponents.Services.APIRequestServices.Interfaces
{
    public interface IAPIRequestDeviceService
    {
        public Task<Device?> CreateAsync(DeviceCreateRequest request);
        public Task<Device?> UpdateAsync(DeviceUpdateRequest request);
        public Task<Device?> UpdateStatusAsync(DeviceUpdateStatusRequest request);
        public Task<bool> DeleteAsync(int id);
        public Task<Device?> GetAsync(int id);
        public Task<ICollection<Device>?> GetAllAsync();
        public Task<ICollection<Device>?> GetAllByTenantIdAsync(int tenantId);
        public Task<bool> RefreshAsync(int id);
    }
}
