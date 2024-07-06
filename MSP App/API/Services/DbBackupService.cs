using API.DataAccess;
using Microsoft.EntityFrameworkCore;
using SharedComponents.DbServices;
using SharedComponents.Entities;

namespace API.Services
{
    public class DbBackupService : IDbBackupService
    {
        public readonly AppDbContext _appDbContext;
        public DbBackupService(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }
        public async Task<DeviceBackup?> CreateAsync(DeviceBackup? backup)
        {
            if (backup != null)
            {
                try
                {
                    await _appDbContext.DeviceBackups.AddAsync(backup);
                    var result = await _appDbContext.SaveChangesAsync();
                    if (result > 0)
                    {
                        return await _appDbContext.DeviceBackups
                            .Where(b => b.Id == backup.Id)
                            .FirstAsync();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred while creating the backup: {ex.Message}");
                }
            }
            return null;
        }

        public async Task<DeviceBackup?> UpdateAsync(DeviceBackup? backup)
        {
            if (backup != null)
            {
                try
                {
                    var existingBackup = await _appDbContext.DeviceBackups
                        .Where(b => b.Id == backup.Id)
                        .FirstAsync();
                    if (existingBackup != null)
                    {
                        _appDbContext.Entry(existingBackup).CurrentValues.SetValues(backup);
                        var result = await _appDbContext.SaveChangesAsync();
                        if (result >= 0)
                        {
                            return await _appDbContext.DeviceBackups
                                .Where(b => b.Id == backup.Id)
                                .FirstAsync();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred while updating the backup: {ex.Message}");
                }
            }
            return null;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var backup = await _appDbContext.DeviceBackups
                    .Where(b => b.Id == id)
                    .FirstAsync();
                if (backup != null)
                {
                    _appDbContext.DeviceBackups.Remove(backup);
                    var result = await _appDbContext.SaveChangesAsync();
                    if (result > 0) 
                    {
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while deleting the backup: {ex.Message}");
            }
            return false;
        }

        public async Task<DeviceBackup?> GetAsync(int id)
        {
            try
            {
                var backup = await _appDbContext.DeviceBackups
                    .Where(b => b.Id == id)
                    .FirstAsync();
                if (backup != null)
                {
                    return backup;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while getting the backup: {ex.Message}");
            }
            return null;
        }

        public async Task<ICollection<DeviceBackup>?> GetAllAsync()
        {
            try
            {
                var backups = await _appDbContext.DeviceBackups.ToListAsync();
                if (backups != null)
                {
                    if (backups.Any())
                    {
                        return backups;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while getting all backups: {ex.Message}");
            }
            return null;
        }

        public async Task<ICollection<DeviceBackup>?> GetAllByClientIdAndUuidAsync(int tenantId, int clientId, string? Uuid)
        {
            try
            {
                var backups = await _appDbContext.DeviceBackups
                    .Where(b => b.TenantId == tenantId && b.Client_Id == clientId && b.Uuid == Uuid)
                    .ToListAsync();
                if (backups != null)
                {
                    if (backups.Any())
                    {
                        return backups;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while getting backups by device id: {ex.Message}");
            }
            return null;
        }

        public async Task<ICollection<DeviceBackup>?> GetAllByTenantIdAsync(int tenantId)
        {
            try
            {
                var backups = await _appDbContext.DeviceBackups
                    .Where(b => b.TenantId == tenantId)
                    .ToListAsync();
                if (backups != null)
                {
                    if (backups.Any())
                    {
                        return backups;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while getting backups by tenant id: {ex.Message}");
            }
            return null;
        }
    }
}
