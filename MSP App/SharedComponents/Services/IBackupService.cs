using SharedComponents.Entities;
using SharedComponents.WebRequestEntities.BackupRequests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedComponents.Services
{
    public interface IBackupService
    {
        public Task<DeviceBackup?> CreateAsync(BackupCreateRequest request);
        public Task<DeviceBackup?> UpdateAsync(BackupUpdateRequest request);
        public Task<bool> DeleteAsync(int id);
        public Task<DeviceBackup?> GetAsync(int id);
        public Task<ICollection<DeviceBackup>?> GetAllAsync();
        public Task<ICollection<DeviceBackup>?> GetAllByDeviceIdAsync(int deviceId);
        public Task<ICollection<DeviceBackup>?> GetAllByTenantIdAsync(int tenantId);
    }
}
