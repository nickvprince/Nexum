using SharedComponents.Entities;

namespace API.Services.Interfaces
{
    public interface IHTTPJobService
    {
        public Task<bool> CreateAsync(int tenantId, DeviceJob job);
        public Task<bool> UpdateAsync(int tenantId, DeviceJob job);
        public Task<bool> DeleteAsync(int tenantId, int clientId, int id);
        public Task<bool> StartAsync(int tenantId, int clientId);
        public Task<bool> StopAsync(int tenantId, int clientId);
        public Task<bool> ResumeAsync(int tenantId, int clientId);
        public Task<bool> PauseAsync(int tenantId, int clientId);
        //bool because cannot deserialize tenant server response
        public Task<bool> GetAsync(int tenantId, int clientId);
        //cannot deserialize tenant server response (Not implemented)
        public Task<bool> GetAllAsync(int tenantId, int clientId);
        public Task<DeviceJobStatus?> GetStatusAsync(int tenantId, int clientId, int id);
    }
}
