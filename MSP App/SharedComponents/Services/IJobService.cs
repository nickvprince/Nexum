using SharedComponents.Entities;
using SharedComponents.WebRequestEntities.DeviceRequests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedComponents.Services
{
    public interface IJobService
    {
        public Task<DeviceJob?> CreateAsync(DeviceCreateRequest request);
        public Task<DeviceJob?> UpdateAsync(DeviceUpdateRequest request);
        public Task<bool> DeleteAsync(int id);
        public Task<DeviceJob?> GetAsync(int id);
        public Task<ICollection<DeviceJob>?> GetAllAsync();
        public Task<ICollection<DeviceJob>?> GetAllByDeviceIdAsync(int deviceId);
        public Task<ICollection<DeviceJob>?> GetAllByTenantIdAsync(int tenantId);
        public Task<ICollection<DeviceJob>?> GetAllByNASServerIdAsync(int nasServerId);
    }
}
