using API.DataAccess;
using Microsoft.EntityFrameworkCore;
using SharedComponents.DbServices;
using SharedComponents.Entities;

namespace API.Services
{
    public class DbLogService : IDbLogService
    {
        private readonly AppDbContext _appDbContext;
        public DbLogService(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public async Task<DeviceLog?> CreateAsync(DeviceLog? log)
        {
            if (log != null)
            {
                try
                {
                    // Add the log to the context
                    await _appDbContext.DeviceLogs.AddAsync(log);

                    // Save changes to the database
                    var result = await _appDbContext.SaveChangesAsync();
                    if (result > 0)
                    {
                        return await _appDbContext.DeviceLogs
                            .Where(l => l.Id == log.Id)
                            .FirstAsync();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred while creating the log: {ex.Message}");
                }
            }
            return null;
        }

        public async Task<DeviceLog?> UpdateAsync(DeviceLog? log)
        {
            if (log != null)
            {
                try
                {
                    // Get the existing log from the context
                    var existingLog = await _appDbContext.DeviceLogs
                        .Where(l => l.Id == log.Id)
                        .FirstAsync();
                    if (existingLog != null)
                    {
                        // Update the existing log with the new values
                        _appDbContext.Entry(existingLog).CurrentValues.SetValues(log);

                        // Save changes to the database
                        var result = await _appDbContext.SaveChangesAsync();
                        if (result >= 0)
                        {
                            return await _appDbContext.DeviceLogs
                                .Where(l => l.Id == log.Id)
                                .FirstAsync();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred while updating the log: {ex.Message}");
                }
            }
            return null;
        }
        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var log = await _appDbContext.DeviceLogs
                    .Where(l => l.Id == id)
                    .FirstAsync();
                if (log != null)
                {
                    log.IsDeleted = true;
                    _appDbContext.DeviceLogs.Update(log);
                    var result = await _appDbContext.SaveChangesAsync();
                    if (result > 0)
                    {
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while deleting the log: {ex.Message}");
            }
            return false;
        }

        public async Task<DeviceLog?> GetAsync(int id)
        {
            try
            {
                var log = await _appDbContext.DeviceLogs
                    .Where(l => l.Id == id)
                    .FirstAsync();
                if (log != null)
                {
                    return log;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while getting the log: {ex.Message}");
            }
            return null;
        }

        public async Task<ICollection<DeviceLog>?> GetAllAsync()
        {
            try
            {
                var logs = await _appDbContext.DeviceLogs.ToListAsync();
                if (logs != null)
                {
                    if (logs.Any())
                    {
                        return logs;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while getting all logs: {ex.Message}");
            }
            return null;
        }

        public async Task<ICollection<DeviceLog>?> GetAllByDeviceIdAsync(int deviceId)
        {
            try
            {
                var logs = await _appDbContext.DeviceLogs
                    .Where(l => l.DeviceId == deviceId)
                    .ToListAsync();
                if (logs != null)
                {
                    if (logs.Any())
                    {
                        return logs;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while getting logs by device id: {ex.Message}");
            }
            return null;
        }

        public async Task<ICollection<DeviceLog>?> GetAllByTenantIdAsync(int tenantId)
        {
            try
            {
                var logs = await _appDbContext.DeviceLogs
                    .Where(l => l.Device.TenantId == tenantId)
                    .ToListAsync();
                if (logs != null)
                {
                    if (logs.Any())
                    { 
                        return logs;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while getting logs by tenant id: {ex.Message}");
            }
            return null;
        }
    }
}
