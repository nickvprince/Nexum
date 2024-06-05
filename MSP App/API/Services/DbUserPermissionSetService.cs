using API.DataAccess;
using Microsoft.EntityFrameworkCore;
using SharedComponents.DbServices;
using SharedComponents.Entities;

namespace API.Services
{
    public class DbUserPermissionSetService : IDbUserPermissionSetService
    {
        private readonly AppDbContext _appDbContext;
        
        public DbUserPermissionSetService(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public Task<bool> AddAsync(UserPermissionSet userPermissionSet)
        {
            throw new NotImplementedException();
        }
        public Task<bool> EditAsync(UserPermissionSet userPermissionSet)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeleteAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<UserPermissionSet> GetAsync(int id)
        {
            throw new NotImplementedException();
        }

        public async Task<ICollection<UserPermissionSet>> GetAllAsync()
        {
            return await _appDbContext.UserPermissionSets
                .Include(ups => ups.User)
                .Include(ups => ups.PermissionSet)
                    .ThenInclude(ps => ps.Permission)
                .Include(ups => ups.PermissionSet)
                    .ThenInclude(ps => ps.Tenant)
                .ToListAsync();
        }
    }
}
