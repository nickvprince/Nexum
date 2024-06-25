using SharedComponents.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedComponents.DbServices
{
    public interface IDbDeviceService
    {
        public Task<Device?> CreateAsync(Device? device);
        public Task<Device?> UpdateAsync(Device? device);
        public Task<bool> DeleteAsync(int id);
        public Task<Device?> GetAsync(int id);
        public Task<Device?> GetByClientIdAndUuidAsync(int tenantId, int clientId, string? uuid);
        public Task<ICollection<Device>> GetAllAsync();
    }
}
