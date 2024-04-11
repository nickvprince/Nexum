using SharedComponents.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedComponents.Services
{
    public interface IPermissionService
    {
        public Task<bool> CreateAsync(Permission permission);
        public Task<bool> EditAsync(Permission permission);
        public Task<bool> DeleteAsync(int id);
        public Task<Permission> GetAsync(int id);
        public Task<List<Permission>> GetAllAsync();
    }
}
