using SharedComponents.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedComponents.Services
{
    public interface IUserService
    {
        public Task<bool> CreateAsync(User user);
        public Task<bool> EditAsync(User user);
        public Task<bool> DeleteAsync(string username);
        public Task<User> GetAsync(string username);
        public Task<List<User>> GetAllAsync();
    }
}
