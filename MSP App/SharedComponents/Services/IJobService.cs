using SharedComponents.Entities;
using SharedComponents.WebEntities.Requests.JobRequests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedComponents.Services
{
    public interface IJobService
    {
        public Task<DeviceJob?> CreateAsync(JobCreateRequest request);
        public Task<DeviceJob?> UpdateAsync(JobUpdateRequest request);
        public Task<DeviceJob?> UpdateStatusAsync(JobUpdateStatusRequest request);
        public Task<bool> DeleteAsync(int id);
        public Task<DeviceJob?> GetAsync(int id);
        public Task<ICollection<DeviceJob>?> GetAllAsync();
        public Task<ICollection<DeviceJob>?> GetAllByDeviceIdAsync(int deviceId);
        public Task<ICollection<DeviceJob>?> GetAllByTenantIdAsync(int tenantId);
        public Task<bool> StartAsync(int id);
        public Task<bool> PauseAsync(int id);
        public Task<bool> ResumeAsync(int id);
        public Task<bool> StopAsync(int id);
        public Task<bool> RefreshAsync(int id);
    }
}
