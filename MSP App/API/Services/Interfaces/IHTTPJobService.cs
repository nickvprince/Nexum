using SharedComponents.Entities;

namespace API.Services.Interfaces
{
    public interface IHTTPJobService
    {
        public Task<bool> CreateJobAsync(int tenantId, DeviceJob job);
        public Task<bool> UpdateJobAsync(int tenantId, DeviceJob job);
        public Task<bool> StartJobAsync(int tenantId, int client_Id);
        public Task<bool> StopJobAsync(int tenantId, int client_Id);
        public Task<bool> ResumeJobAsync(int tenantId, int client_Id);
        public Task<bool> PauseJobAsync(int tenantId, int client_Id);
        //bool because cannot deserialize tenant server response
        public Task<bool> GetJobAsync(int tenantId, int client_Id);
    }
}
