using SharedComponents.Entities;

namespace API.Services.Interfaces
{
    public interface IHTTPJobService
    {
        public Task<bool> CreateAsync(int tenantId, DeviceJob job);
        public Task<bool> UpdateAsync(int tenantId, DeviceJob job);
        public Task<bool> DeleteAsync(int tenantId, int client_Id, int id);
        public Task<bool> StartAsync(int tenantId, int client_Id);
        public Task<bool> StopAsync(int tenantId, int client_Id);
        public Task<bool> ResumeAsync(int tenantId, int client_Id);
        public Task<bool> PauseAsync(int tenantId, int client_Id);
        //bool because cannot deserialize tenant server response
        public Task<bool> GetAsync(int tenantId, int client_Id);
        //cannot deserialize tenant server response (Not implemented)
        public Task<bool> GetAllAsync(int tenantId, int client_Id);
        public Task<DeviceJobStatus?> GetStatusAsync(int tenantId, int client_Id, int id);
    }
}
