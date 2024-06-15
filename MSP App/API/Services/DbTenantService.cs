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

        public async Task<bool> CreateAsync(Tenant tenant)
        {
            if (tenant != null)
            {
                try
                {
                    // Add the tenant to the context
                    await _appDbContext.Tenants.AddAsync(tenant);

                    // Save changes to the database
                    var result = await _appDbContext.SaveChangesAsync();

                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred while creating the tenant: {ex.Message}");
                }
            }
            return false;
        }

        public async Task<bool> UpdateAsync(Tenant tenant)
        {
            if (tenant != null)
            {
                try
                {
                    var existingTenant = await _appDbContext.Tenants.FindAsync(tenant.Id);
                    if (existingTenant != null)
                    {
                        _appDbContext.Entry(existingTenant).CurrentValues.SetValues(tenant);

                        var result = await _appDbContext.SaveChangesAsync();

                        return true;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred while updating the tenant: {ex.Message}");
                }
            }
            return false;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var tenant = await _appDbContext.Tenants.FindAsync(id);
                if (tenant != null)
                {
                    _appDbContext.Tenants.Remove(tenant);
                    var result = await _appDbContext.SaveChangesAsync();

                    return true;
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
            return await _appDbContext.Tenants
                .Where(t => t.Id == id)
                .Include(t => t.TenantInfo)
                .Include(t => t.InstallationKeys)
                .Include(t => t.Devices)
                    .ThenInclude(d => d.DeviceInfo)
                        .ThenInclude(di => di.MACAddresses)
                .Include(t => t.RolePermissions)
                    .ThenInclude(rp => rp.Permission)
                .Include(t => t.RolePermissions)
                    .ThenInclude(rp => rp.Role)
                .FirstOrDefaultAsync();
        }

        public async Task<ICollection<Tenant>> GetAllAsync()
        {
            return await _appDbContext.Tenants
                .Include(t => t.TenantInfo)
                .Include(t => t.InstallationKeys)
                .Include(t => t.Devices)
                    .ThenInclude(d => d.DeviceInfo)
                        .ThenInclude(di => di.MACAddresses)
                .Include(t => t.RolePermissions)
                    .ThenInclude(rp => rp.Permission)
                .Include(t => t.RolePermissions)
                    .ThenInclude(rp => rp.Role)
                        .ThenInclude(r => r.UserRoles)
                .ToListAsync();
        }
    }
}
