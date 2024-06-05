using SharedComponents.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedComponents.DbServices
{
    public interface IDbUserPermissionSetService
    {
        public Task<bool> AddAsync(UserPermissionSet userPermissionSet);
        public Task<bool> EditAsync(UserPermissionSet UserPermissionSet);
        public Task<bool> DeleteAsync(int id);
        public Task<UserPermissionSet> GetAsync(int id);
        public Task<ICollection<UserPermissionSet>> GetAllAsync();
    }
}
