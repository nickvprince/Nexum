using SharedComponents.Entities;
using SharedComponents.WebRequestEntities.DeviceRequests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedComponents.Services
{
    public interface IDeviceService
    {
        public Task<Device?> CreateAsync(DeviceCreateRequest request);
        public Task<Device?> UpdateAsync(DeviceUpdateRequest request);
        public Task<Device?> UpdateStatusAsync(DeviceUpdateStatusRequest request);
        public Task<bool> DeleteAsync(int id);
        public Task<Device?> GetAsync(int id);
        public Task<ICollection<Device>?> GetAllAsync();
        public Task<ICollection<Device>?> GetAllByTenantIdAsync(int tenantId);
    }
}
