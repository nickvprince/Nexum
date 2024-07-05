using SharedComponents.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedComponents.DbServices
{
    public interface IDbBackupService
    {
        public Task<DeviceBackup?> CreateAsync(DeviceBackup? backup);
        public Task<DeviceBackup?> UpdateAsync(DeviceBackup? backup);
        public Task<bool> DeleteAsync(int id);
        public Task<DeviceBackup?> GetAsync(int id);
        public Task<ICollection<DeviceBackup>?> GetAllAsync();
        public Task<ICollection<DeviceBackup>?> GetAllByDeviceIdAsync(int deviceId);
        public Task<ICollection<DeviceBackup>?> GetAllByTenantIdAsync(int tenantId);
    }
}
