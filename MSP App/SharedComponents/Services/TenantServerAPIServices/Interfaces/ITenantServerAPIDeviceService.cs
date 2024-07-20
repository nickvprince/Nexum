using SharedComponents.Entities.DbEntities;
using SharedComponents.Entities.TenantServerHttpEntities.Requests;
using SharedComponents.Entities.TenantServerHttpEntities.Responses;

namespace SharedComponents.Services.TenantServerAPIServices.Interfaces
{
    public interface ITenantServerAPIDeviceService
    {
        public Task<bool?> ForceDeviceCheckinAsync(int tenantId, int client_id);
        public Task<bool?> ForceDeviceUpdateAsync(int tenantId, int client_id);
        public Task<DeviceStatus?> GetDeviceStatusAsync(int tenantId, int client_id);
        public Task<GetDeviceFilesResponse?> GetDeviceFilesAsync(int tenantId, GetDeviceFilesRequest request);
    }
}
