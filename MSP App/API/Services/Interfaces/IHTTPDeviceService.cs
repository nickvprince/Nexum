using SharedComponents.RequestEntities.HTTP;
using SharedComponents.ResponseEntities.HTTP;

namespace API.Services.Interfaces
{
    public interface IHTTPDeviceService
    {
        public Task<bool?> ForceDeviceCheckinAsync(int tenantId, int client_id);
        public Task<bool?> ForceDeviceUpdateAsync(int tenantId, int client_id);
        public Task<GetDeviceFilesResponse?> GetDeviceFilesAsync(int tenantId, GetDeviceFilesRequest request);
    }
}
