using SharedComponents.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedComponents.Services
{
    public interface IGroupService
    {
        public Task<bool> CreateAsync(Group permission);
        public Task<bool> EditAsync(Group permission);
        public Task<bool> DeleteAsync(int id);
        public Task<Group> GetAsync(int id);
        public Task<List<Group>> GetAllAsync();
    }
}
