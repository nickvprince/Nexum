using SharedComponents.Entities;
using SharedComponents.WebRequestEntities.PermissionRequests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedComponents.Services
{
    public interface IPermissionService
    {
        public Task<Permission?> CreateAsync(PermissionCreateRequest request);
        public Task<Permission?> UpdateAsync(PermissionUpdateRequest request);
        public Task<bool> DeleteAsync(int id);
        public Task<Permission?> GetAsync(int id);
        public Task<ICollection<Permission>?> GetAllAsync();
    }
}
