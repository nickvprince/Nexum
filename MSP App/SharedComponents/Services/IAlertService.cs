using SharedComponents.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedComponents.Services
{
    public interface IAlertService
    {
        public Task<DeviceAlert?> CreateAsync(DeviceAlert alert);
        public Task<DeviceAlert?> EditAsync(DeviceAlert alert);
        public Task<bool> DeleteAsync(int id);
        public Task<DeviceAlert?> GetAsync(int id);
        public Task<ICollection<DeviceAlert>?> GetAllAsync();
    }
}
