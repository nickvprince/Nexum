using SharedComponents.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedComponents.DbServices
{
    public interface IDbJobService
    {
        public Task<DeviceJob?> CreateAsync(DeviceJob? job);
        public Task<DeviceJob?> UpdateAsync(DeviceJob? job);
        public Task<bool> DeleteAsync(int id);
        public Task<DeviceJob?> GetAsync(int id);
        public Task<ICollection<DeviceJob>?> GetAllAsync();
        public Task<ICollection<DeviceJob>?> GetAllByDeviceIdAsync(int deviceId);
        public Task<ICollection<DeviceJob>?> GetAllByTenantIdAsync(int tenantId);
        public Task<ICollection<DeviceJob>?> GetAllByBackupServerIdAsync(int tenantId, int backupServerId);
    }
}
