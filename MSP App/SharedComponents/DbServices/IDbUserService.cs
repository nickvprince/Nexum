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
        public Task<bool> CreateAsync(ApplicationUser? user, string? password);
        public Task<bool> UpdateAsync(ApplicationUser? user);
        public Task<bool> UpdatePasswordAsync(string? username, string? currentPassword, string? newPassword);
        public Task<bool> DeleteAsync(string? id);
        public Task<ApplicationUser?> GetByIdAsync(string? id);
        public Task<ApplicationUser?> GetByUserNameAsync(string? username);
        public Task<ICollection<ApplicationUser>?> GetAllAsync();
        public Task<bool> CheckPasswordAsync(string? username, string? password);
    }
}
