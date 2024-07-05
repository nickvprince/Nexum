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
        public Task<bool> CreateAsync(ApplicationUser user);
        public Task<bool> EditAsync(ApplicationUser user);
        public Task<bool> DeleteAsync(string username);
        public Task<ApplicationUser?> GetAsync(string username);
        public Task<ICollection<ApplicationUser>?> GetAllAsync();
    }
}
