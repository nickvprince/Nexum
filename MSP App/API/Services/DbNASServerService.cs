using API.DataAccess;
using Microsoft.EntityFrameworkCore;
using SharedComponents.DbServices;
using SharedComponents.Entities;

namespace API.Services
{
    public class DbNASServerService : IDbNASServerService
    {
        public readonly AppDbContext _appDbContext;
        public DbNASServerService(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public async Task<NASServer?> CreateAsync(NASServer? nasServer)
        {
            if (nasServer != null)
            {
                try
                {
                    await _appDbContext.NASServers.AddAsync(nasServer);
                    var result = await _appDbContext.SaveChangesAsync();
                    if (result > 0)
                    {
                        return await _appDbContext.NASServers
                            .Where(n => n.Id == nasServer.Id)
                            .FirstAsync();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred while creating the NAS Server: {ex.Message}");
                }
            }
            return null;
        }
        public async Task<NASServer?> UpdateAsync(NASServer? nasServer)
        {
            if(nasServer != null)
            {
                try
                {
                    var existingNASServer = await _appDbContext.NASServers
                        .Where(n => n.Id == nasServer.Id)
                        .FirstAsync();
                    if (existingNASServer != null)
                    {
                        _appDbContext.Entry(existingNASServer).CurrentValues.SetValues(nasServer);
                        var result = await _appDbContext.SaveChangesAsync();
                        if (result >= 0)
                        {
                            return await _appDbContext.NASServers
                                .Where(n => n.Id == nasServer.Id)
                                .FirstAsync();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred while updating the NAS Server: {ex.Message}");
                }
            }
            return null;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var nasServer = await _appDbContext.NASServers
                    .Where(n => n.Id == id)
                    .FirstAsync();
                if (nasServer != null)
                {
                    _appDbContext.NASServers.Remove(nasServer);
                    var result = await _appDbContext.SaveChangesAsync();
                    if (result > 0)
                    {
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while deleting the NAS Server: {ex.Message}");
            }
            return false;
        }

        public async Task<NASServer?> GetAsync(int id)
        {
            try
            {
                var nasServer = await _appDbContext.NASServers
                    .Where(n => n.Id == id)
                    .FirstAsync();
                if (nasServer != null)
                {
                    return nasServer;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while getting the NAS Server: {ex.Message}");
            }
            return null;
        }

        public async Task<NASServer?> GetByBackupServerIdAsync(int tenantId,int backupServerId)
        {
            try
            {
                var nasServer = await _appDbContext.NASServers
                    .Where(n => n.BackupServerId == backupServerId && n.TenantId == tenantId)
                    .FirstAsync();
                if (nasServer != null)
                {
                    return nasServer;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while getting the NAS Server by Backup Server ID: {ex.Message}");
            }
            return null;
        }

        public async Task<ICollection<NASServer>?> GetAllAsync()
        {
            try
            {
                var nasServers = await _appDbContext.NASServers.ToListAsync();
                if (nasServers != null)
                {
                    if (nasServers.Any())
                    {
                        return nasServers;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while getting all NAS Servers: {ex.Message}");
            }
            return null;
        }

        public async Task<ICollection<NASServer>?> GetAllByTenantIdAsync(int tenantId)
        {
            try
            {
                var nasServers = await _appDbContext.NASServers
                    .Where(n => n.TenantId == tenantId)
                    .ToListAsync();
                if (nasServers != null)
                {
                    if (nasServers.Any())
                    {
                        return nasServers;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while getting all NAS Servers by Tenant ID: {ex.Message}");
            }
            return null;
        }

        public async Task<ICollection<NASServer>?> GetAllByDeviceIdAsync(int deviceId)
        {
            try
            {
                var device = await _appDbContext.Devices
                    .Where(d => d.Id == deviceId)
                    .FirstAsync();
                var jobs = await _appDbContext.DeviceJobs
                    .Where(j => j.DeviceId == deviceId)
                    .ToListAsync();

                var nasServersFromBackups = _appDbContext.NASServers
                    .Where(n => n.Backups.Any(b => b.Client_Id == device.DeviceInfo.ClientId && b.Uuid == device.DeviceInfo.Uuid));
                var nasServersFromJobs = _appDbContext.NASServers
                    .Where(n => jobs.Any(j => j.Settings.BackupServerId == n.BackupServerId));

                var nasServers = await nasServersFromBackups
                    .Union(nasServersFromJobs)
                    .ToListAsync();

                if (nasServers != null)
                {
                    return nasServers;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while getting all NAS Servers by Device ID: {ex.Message}");
            }
            return null;
        }
    }
}
