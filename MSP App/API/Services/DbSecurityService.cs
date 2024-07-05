using API.DataAccess;
using Microsoft.EntityFrameworkCore;
using SharedComponents.DbServices;
using SharedComponents.Entities;

namespace API.Services
{
    public class DbSecurityService : IDbSecurityService
    {
        private readonly AppDbContext _appDbContext;
        public DbSecurityService(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }
        public async Task<bool> ValidateAPIKey(string apikey)
        {
            if (apikey != null)
            {
                Tenant? tenant = await _appDbContext.Tenants
                    .Where(t => t.ApiKey == apikey)
                    .FirstOrDefaultAsync();

                if (tenant != null)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
