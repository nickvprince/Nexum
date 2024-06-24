using SharedComponents.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedComponents.DbServices
{
    public interface IDbLogService
    {
        public Task<DeviceLog?> CreateAsync(DeviceLog log);
        public Task<bool> DeleteAsync(int id);
        public Task<DeviceLog?> GetAsync(int id);
        public Task<ICollection<DeviceLog>> GetAllAsync();
    }
}
