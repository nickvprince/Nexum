using API.DataAccess;
using Microsoft.EntityFrameworkCore;
using SharedComponents.DbServices;
using SharedComponents.Entities;

namespace API.Services
{
    public class DbUserService : IDbUserService
    {
        private readonly AppDbContext _appDbContext;

        public DbUserService(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public Task<bool> AddAsync(ApplicationUser user)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeleteAsync(string? id)
        {
            throw new NotImplementedException();
        }

        public Task<bool> EditAsync(ApplicationUser user)
        {
            throw new NotImplementedException();
        }

        public async Task<ApplicationUser?> GetAsync(string? id)
        {
            return await _appDbContext.Users.FirstOrDefaultAsync(u => u.Id == id);
        }
        public async Task<ICollection<ApplicationUser>> GetAllAsync()
        {
            return await _appDbContext.Users.ToListAsync();
        }
    }
}
