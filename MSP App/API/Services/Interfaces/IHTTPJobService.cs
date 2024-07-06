using SharedComponents.Entities;

namespace API.Services.Interfaces
{
    public interface IHTTPJobService
    {
        public Task<bool> CreateJobAsync(int tenantId, DeviceJob job);
        public Task<bool> UpdateJobAsync(int tenantId, DeviceJob job);
        public Task<bool> StartJobAsync(int clientId);
        public Task<bool> StopJobAsync(int clientId);
        public Task<bool> ResumeJobAsync(int clientId);
        public Task<bool> PauseJobAsync(int clientId);
    }
}
