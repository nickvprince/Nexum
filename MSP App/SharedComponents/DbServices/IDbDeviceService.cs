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
        public Task<bool> CreateAsync(Device device);
        public Task<bool> UpdateAsync(Device device);
        public Task<bool> DeleteAsync(int id);
        public Task<Device?> GetAsync(int id);
        public Task<ICollection<Device>> GetAllAsync();
    }
}
