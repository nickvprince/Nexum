using SharedComponents.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedComponents.Services
{
    public interface IUserPermissionSetService
    {
        public Task<bool> CreateAsync(UserPermissionSet userPermissionSet);
        public Task<bool> EditAsync(UserPermissionSet userPermissionSet);
        public Task<bool> DeleteAsync(int id);
        public Task<UserPermissionSet> GetAsync(int id);
        public Task<ICollection<UserPermissionSet>?> GetAllAsync();
    }
}
