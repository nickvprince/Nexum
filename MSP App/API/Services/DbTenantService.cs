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

                    return await _appDbContext.Tenants
                        .Where(t => t.Id == tenant.Id)
                        .Include(t => t.TenantInfo)
                        .FirstOrDefaultAsync();
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
                    var existingTenant = await _appDbContext.Tenants.FindAsync(tenant.Id);
                    if (existingTenant != null)
                    {
                        _appDbContext.Entry(existingTenant).CurrentValues.SetValues(tenant);

                        var result = await _appDbContext.SaveChangesAsync();

                        return await _appDbContext.Tenants
                            .Where(t => t.Id == tenant.Id)
                            .Include(t => t.TenantInfo)
                            .FirstOrDefaultAsync();
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
                .FirstOrDefaultAsync();
        }
        public async Task<Tenant?> GetByApiKeyAsync(string? apikey)
        {
            return await _appDbContext.Tenants
                .Where(t => t.ApiKey == apikey)
                .Include(t => t.TenantInfo)
                .Include(t => t.InstallationKeys)
                .Include(t => t.Devices)
                    .ThenInclude(d => d.DeviceInfo)
                        .ThenInclude(di => di.MACAddresses)
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
                .ToListAsync();
        }

        public async Task<InstallationKey?> CreateInstallationKeyAsync(int tenantId)
        {
            var installationKey = new InstallationKey
            {
                Key = Guid.NewGuid().ToString(),
                TenantId = tenantId,
                IsActive = true
            };
            try
            {
                await _appDbContext.InstallationKeys.AddAsync(installationKey);
                var result = await _appDbContext.SaveChangesAsync();

                return await _appDbContext.InstallationKeys
                    .Where(i => i.Id == installationKey.Id)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while creating the installation key: {ex.Message}");
            }
            return null;
        }

        public async Task<InstallationKey?> UpdateInstallationKeyAsync(InstallationKey? installationkey)
        {
            if (installationkey != null)
            {
                try
                {
                    var existingInstallationKey = await _appDbContext.InstallationKeys
                        .Where(i => i.Key == installationkey.Key)
                        .FirstOrDefaultAsync();
                       
                    if (existingInstallationKey != null)
                    {
                        _appDbContext.Entry(existingInstallationKey).CurrentValues.SetValues(installationkey);

                        var result = await _appDbContext.SaveChangesAsync();

                        return await _appDbContext.InstallationKeys
                            .Where(i => i.Key == installationkey.Key)
                            .FirstOrDefaultAsync();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred while updating the installation key: {ex.Message}");
                }
            }
            return null;
        }

        public async Task<bool> DeleteInstallationKeyAsync(string? installationkey)
        {
            if (installationkey != null)
            {
                try
                {
                    var existingInstallationKey = await _appDbContext.InstallationKeys
                        .Where(i => i.Key == installationkey)
                        .FirstOrDefaultAsync();
                    if (existingInstallationKey != null)
                    {
                        existingInstallationKey.IsActive = false;
                        _appDbContext.Entry(existingInstallationKey).State = EntityState.Modified;
                        var result = await _appDbContext.SaveChangesAsync();

                        return true;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred while deleting the installation key: {ex.Message}");
                }
            }
            return false;
        }

        public async Task<InstallationKey?> GetInstallationKeyAsync(string? installationkey)
        {
            return await _appDbContext.InstallationKeys
                .Where(i => i.Key == installationkey)
                .FirstOrDefaultAsync();
        }
    }
}
