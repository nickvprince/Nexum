using API.DataAccess;
using Microsoft.EntityFrameworkCore;
using SharedComponents.DbServices;
using SharedComponents.Entities;
using SharedComponents.Services;

namespace API.Services
{
    public class DbTenantService : IDbTenantService
    {
        private readonly AppDbContext _appDbContext;
        public DbTenantService(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public Task<bool> CreateAsync(Tenant tenant)
        {
            throw new NotImplementedException();
        }

        public Task<bool> EditAsync(Tenant tenant)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeleteAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<Tenant> GetAsync(int id)
        {
            throw new NotImplementedException();
        }

        public async Task<ICollection<Tenant>> GetAllAsync()
        {
            return await _appDbContext.Tenants
                .Include(t => t.UserTenants)
                .Include(t => t.Devices)
                .ToListAsync();
        }
    }
}
