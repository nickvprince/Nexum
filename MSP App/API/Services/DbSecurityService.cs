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
        public async Task<bool> ValidateAPIKey(string? apikey)
        {
            if (!string.IsNullOrEmpty(apikey))
            {
                try
                {
                    Tenant? tenant = await _appDbContext.Tenants
                        .Where(t => t.ApiKey == apikey)
                        .FirstAsync();
                    if (tenant != null)
                    {
                        return true;
                    }
                }
                catch
                {
                    return false;
                }
            }
            return false;
        }
    }
}
