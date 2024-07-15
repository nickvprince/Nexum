using SharedComponents.Entities;
using SharedComponents.WebEntities.Requests.LogRequests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedComponents.Services
{
    public interface ILogService
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
