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
        public Task<bool> CreateAsync(DeviceAlert alert);
        public Task<bool> EditAsync(DeviceAlert alert);
        public Task<bool> DeleteAsync(string id);
        public Task<DeviceAlert?> GetAsync(string id);
        public Task<ICollection<DeviceAlert>?> GetAllAsync();
    }
}
