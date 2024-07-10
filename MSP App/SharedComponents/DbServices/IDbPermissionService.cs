using SharedComponents.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedComponents.DbServices
{
    public interface IDbPermissionService
    {
        public Task<Permission?> CreateAsync(Permission? permission);
        public Task<Permission?> UpdateAsync(Permission? permission);
        public Task<bool> DeleteAsync(int id);
        public Task<Permission?> GetAsync(int id);
        public Task<ICollection<Permission>?> GetAllAsync();
    }
}
