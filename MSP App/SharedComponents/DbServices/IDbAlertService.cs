using SharedComponents.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedComponents.DbServices
{
    public interface IDbAlertService
    {
        public Task<DeviceAlert?> CreateAsync(DeviceAlert alert);
        public Task<DeviceAlert?> GetAsync(int id);
        public Task<ICollection<DeviceAlert>> GetAllAsync();
    }
}
