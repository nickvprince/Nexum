using API.DataAccess;
using Microsoft.EntityFrameworkCore;
using SharedComponents.Entities.DbEntities;
using SharedComponents.Services.DbServices.Interfaces;

namespace API.Services.DbServices
{
    public class DbDeviceService : IDbDeviceService
    {
        private readonly AppDbContext _appDbContext;
        public DbDeviceService(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public async Task<Device?> CreateAsync(Device? device)
        {
            if (device != null)
            {
                try
                {
                    await _appDbContext.Devices.AddAsync(device);
                    var result = await _appDbContext.SaveChangesAsync();
                    if (result > 0)
                    {
                        return await _appDbContext.Devices
                            .Where(d => d.Id == device.Id)
                            .Include(d => d.DeviceInfo!)
                                .ThenInclude(di => di.MACAddresses)
                            .FirstAsync();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred while creating the device: {ex.Message}");
                }
            }
            return null;
        }

        public async Task<Device?> UpdateAsync(Device? device)
        {
            if (device != null)
            {
                try
                {
                    var existingDevice = await _appDbContext.Devices
                        .Where(d => d.Id == device.Id)
                        .FirstAsync();
                    if (existingDevice != null)
                    {
                        _appDbContext.Entry(existingDevice).CurrentValues.SetValues(device);

                        var result = await _appDbContext.SaveChangesAsync();
                        if (result >= 0)
                        {
                            return await _appDbContext.Devices
                                .Where(d => d.Id == device.Id)
                                .Include(d => d.DeviceInfo!)
                                    .ThenInclude(di => di.MACAddresses)
                                .FirstAsync();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred while updating the device: {ex.Message}");
                }
            }
            return null;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var device = await _appDbContext.Devices
                    .Where(d => d.Id == id)
                    .FirstAsync();
                if (device != null)
                {
                    _appDbContext.Devices.Remove(device);
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

        public async Task<Device?> GetByClientIdAndUuidAsync(int tenantId, int clientId, string? uuid)
        {
            try
            {
                var device = await _appDbContext.Devices
                    .Where(d => d.DeviceInfo!.ClientId == clientId && d.DeviceInfo.Uuid == uuid && d.TenantId == tenantId)
                    .Include(d => d.DeviceInfo!)
                        .ThenInclude(di => di.MACAddresses)
                    .FirstAsync();
                if (device != null)
                {
                    return device;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while getting the device: {ex.Message}");
            }
            return null;
        }

        public async Task<Device?> GetAsync(int id)
        {
            try
            {
                var device = await _appDbContext.Devices
                    .Where(d => d.Id == id)
                    .Include(d => d.DeviceInfo)
                        .ThenInclude(di => di.MACAddresses)
                    .FirstAsync();
                if (device != null)
                {
                    return device;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while getting the device: {ex.Message}");
            }
            return null;
        }

        public async Task<ICollection<Device>?> GetAllAsync()
        {
            try
            {
                var devices = await _appDbContext.Devices
                    .Include(d => d.DeviceInfo)
                        .ThenInclude(di => di.MACAddresses)
                    .ToListAsync();
                if (devices != null)
                {
                    if (devices.Any())
                    {
                        return devices;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while getting all devices: {ex.Message}");
            }
            return null;
        }

        public async Task<ICollection<Device>?> GetAllByTenantIdAsync(int tenantId)
        {
            try
            {
                var devices = await _appDbContext.Devices
                    .Where(d => d.TenantId == tenantId)
                    .Include(d => d.DeviceInfo)
                        .ThenInclude(di => di.MACAddresses)
                    .ToListAsync();
                if (devices != null)
                {
                    if (devices.Any())
                    {
                        return devices;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while getting all devices by tenant id: {ex.Message}");
            }
            return null;
        }

    }
}
