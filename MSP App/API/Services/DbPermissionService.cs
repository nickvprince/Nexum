using API.DataAccess;
using Microsoft.EntityFrameworkCore;
using SharedComponents.DbServices;
using SharedComponents.Entities;

namespace API.Services
{
    public class DbPermissionService : IDbPermissionService
    {
        private readonly AppDbContext _appDbContext;
        
        public DbPermissionService(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public Task<bool> AddAsync(Permission permission)
        {
            throw new NotImplementedException();
        }
        public Task<bool> EditAsync(Permission permission)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeleteAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<Permission> GetAsync(int id)
        {
            throw new NotImplementedException();
        }

        public async Task<List<Permission>> GetAllAsync()
        {
            return await _appDbContext.Permissions.ToListAsync();
        }
    }
}
