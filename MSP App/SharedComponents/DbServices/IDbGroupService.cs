using SharedComponents.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedComponents.DbServices
{
    public interface IDbGroupService
    {
        public Task<bool> CreateAsync(Group group);
        public Task<bool> EditAsync(Group group);
        public Task<bool> DeleteAsync(int id);
        public Task<Group> GetAsync(int id);
        public Task<List<Group>> GetAllAsync();
    }
}
