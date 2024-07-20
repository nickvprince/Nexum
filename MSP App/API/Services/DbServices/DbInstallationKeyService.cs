using API.DataAccess;
using Microsoft.EntityFrameworkCore;
using SharedComponents.Entities.DbEntities;
using SharedComponents.Services.DbServices.Interfaces;

namespace API.Services.DbServices
{
    public class DbInstallationKeyService : IDbInstallationKeyService
    {
        private readonly AppDbContext _appDbContext;
        public DbInstallationKeyService(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public async Task<InstallationKey?> CreateAsync(InstallationKey? installationKey)
        {
            if (installationKey != null)
            {
                try
                {
                    await _appDbContext.InstallationKeys.AddAsync(installationKey);
                    var result = await _appDbContext.SaveChangesAsync();
                    if (result > 0)
                    {
                        return await _appDbContext.InstallationKeys
                            .Where(i => i.Id == installationKey.Id)
                            .FirstAsync();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred while creating the installation key: {ex.Message}");
                }
            }
            return null;
        }

        public async Task<InstallationKey?> UpdateAsync(InstallationKey? installationkey)
        {
            if (installationkey != null)
            {
                try
                {
                    var existingInstallationKey = await _appDbContext.InstallationKeys
                        .Where(i => i.Id == installationkey.Id)
                        .FirstAsync();
                    if (existingInstallationKey != null)
                    {
                        _appDbContext.Entry(existingInstallationKey).CurrentValues.SetValues(installationkey);

                        var result = await _appDbContext.SaveChangesAsync();
                        if (result >= 0)
                        {
                            return await _appDbContext.InstallationKeys
                                .Where(i => i.Id == installationkey.Id)
                                .FirstAsync();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred while updating the installation key: {ex.Message}");
                }
            }
            return null;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var installationKey = await _appDbContext.InstallationKeys
                    .Where(i => i.Id == id)
                    .FirstAsync();
                if (installationKey != null)
                {
                    installationKey.IsDeleted = true;
                    _appDbContext.InstallationKeys.Update(installationKey);
                    var result = await _appDbContext.SaveChangesAsync();
                    if (result > 0)
                    {
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while deleting the installation key: {ex.Message}");
            }
            return false;
        }

        public async Task<InstallationKey?> GetAsync(int id)
        {
            try
            {
                var installationKey = await _appDbContext.InstallationKeys
                    .Where(i => i.Id == id)
                    .FirstAsync();
                if (installationKey != null)
                {
                    return installationKey;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while getting the installation key: {ex.Message}");
            }
            return null;
        }

        public async Task<InstallationKey?> GetByInstallationKeyAsync(string? installationkey)
        {
            try
            {
                var installationKey = await _appDbContext.InstallationKeys
                    .Where(i => i.Key == installationkey)
                    .FirstAsync();
                if (installationKey != null)
                {
                    return installationKey;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while getting the installation key: {ex.Message}");
            }
            return null;
        }

        public async Task<ICollection<InstallationKey>?> GetAllAsync()
        {
            try
            {
                var installationKeys = await _appDbContext.InstallationKeys.ToListAsync();
                if (installationKeys != null)
                {
                    if (installationKeys.Any())
                    {
                        return installationKeys;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while getting all installation keys: {ex.Message}");
            }
            return null;
        }

        public async Task<ICollection<InstallationKey>?> GetAllByTenantIdAsync(int tenantId)
        {
            try
            {
                var installationKeys = await _appDbContext.InstallationKeys
                    .Where(i => i.TenantId == tenantId)
                    .ToListAsync();
                if (installationKeys != null)
                {
                    if (installationKeys.Any())
                    {
                        return installationKeys;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while getting all installation keys: {ex.Message}");
            }
            return null;
        }
    }
}
