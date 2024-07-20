using API.DataAccess;
using Microsoft.EntityFrameworkCore;
using SharedComponents.Entities.DbEntities;
using SharedComponents.Services.DbServices.Interfaces;

namespace API.Services.DbServices
{
    public class DbTenantService : IDbTenantService
    {
        private readonly AppDbContext _appDbContext;
        public DbTenantService(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public async Task<Tenant?> CreateAsync(Tenant? tenant)
        {
            if (tenant != null)
            {
                tenant.ApiKey = Guid.NewGuid().ToString();
                try
                {
                    // Add the tenant to the context
                    await _appDbContext.Tenants.AddAsync(tenant);

                    // Save changes to the database
                    var result = await _appDbContext.SaveChangesAsync();
                    if (result > 0)
                    {
                        return await _appDbContext.Tenants
                            .Where(t => t.Id == tenant.Id)
                            .Include(t => t.TenantInfo)
                            .FirstAsync();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred while creating the tenant: {ex.Message}");
                }
            }
            return null;
        }

        public async Task<Tenant?> UpdateAsync(Tenant? tenant)
        {
            if (tenant != null)
            {
                try
                {
                    var existingTenant = await _appDbContext.Tenants
                        .Where(t => t.Id == tenant.Id)
                        .FirstAsync();
                    if (existingTenant != null)
                    {
                        _appDbContext.Entry(existingTenant).CurrentValues.SetValues(tenant);

                        var result = await _appDbContext.SaveChangesAsync();
                        if (result >= 0)
                        {
                            return await _appDbContext.Tenants
                                .Where(t => t.Id == tenant.Id)
                                .Include(t => t.TenantInfo)
                                .FirstAsync();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred while updating the tenant: {ex.Message}");
                }
            }
            return null;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var tenant = await _appDbContext.Tenants
                    .Where(t => t.Id == id)
                    .FirstAsync();
                if (tenant != null)
                {
                    _appDbContext.Tenants.Remove(tenant);
                    var result = await _appDbContext.SaveChangesAsync();
                    if (result > 0)
                    {
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while deleting the tenant: {ex.Message}");
            }
            return false;
        }

        public async Task<Tenant?> GetAsync(int id)
        {
            try
            {
                var tenant = await _appDbContext.Tenants
                    .Where(t => t.Id == id)
                    .Include(t => t.TenantInfo)
                    .FirstAsync();
                if (tenant != null)
                {
                    return tenant;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while getting the tenant: {ex.Message}");
            }
            return null;
        }

        public async Task<Tenant?> GetRichAsync(int id)
        {
            try
            {
                var tenant = await _appDbContext.Tenants
                    .Where(t => t.Id == id)
                    .Include(t => t.TenantInfo)
                    .Include(t => t.InstallationKeys)
                    .Include(t => t.Devices!)
                        .ThenInclude(d => d.DeviceInfo!)
                            .ThenInclude(di => di.MACAddresses)
                    .FirstAsync();
                if (tenant != null)
                {
                    return tenant;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while getting the tenant: {ex.Message}");
            }
            return null;
        }

        public async Task<Tenant?> GetByApiKeyAsync(string? apikey)
        {
            try
            {
                var tenant = await _appDbContext.Tenants
                    .Where(t => t.ApiKey == apikey)
                    .Include(t => t.TenantInfo)
                    .FirstAsync();
                if (tenant != null)
                {
                    return tenant;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while getting the tenant: {ex.Message}");
            }
            return null;
        }

        public async Task<ICollection<Tenant>?> GetAllAsync()
        {
            try
            {
                var tenants = await _appDbContext.Tenants
                    .Include(t => t.TenantInfo)
                    .ToListAsync();
                if (tenants != null)
                {
                    if (tenants.Any())
                    {
                        return tenants;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while getting all tenants: {ex.Message}");
            }
            return null;
        }
    }
}
