using SharedComponents.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedComponents.DbServices
{
    public interface IDbUserService
    {
        public Task<bool> AddAsync(User user);
        public Task<bool> EditAsync(User user);
        public Task<bool> DeleteAsync(int id);
        public Task<User> GetAsync(int id);
        public Task<List<User>> GetAllAsync();
    }
}
