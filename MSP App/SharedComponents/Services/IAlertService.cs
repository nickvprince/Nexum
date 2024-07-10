using SharedComponents.Entities;
using SharedComponents.WebRequestEntities.AlertRequests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedComponents.Services
{
    public interface IAlertService
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
