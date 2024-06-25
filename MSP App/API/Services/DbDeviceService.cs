using API.DataAccess;
using Microsoft.EntityFrameworkCore;
using SharedComponents.DbServices;
using SharedComponents.Entities;

namespace API.Services
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
                    // Add the device to the context
                    await _appDbContext.Devices.AddAsync(device);

                    // Save changes to the database
                    var result = await _appDbContext.SaveChangesAsync();

                    return await _appDbContext.Devices
                        .Where(d => d.Id == device.Id)
                        .Include(d => d.DeviceInfo)
                            .ThenInclude(di => di.MACAddresses)
                        .FirstOrDefaultAsync(); ;
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
                    var existingDevice = await _appDbContext.Devices.FindAsync(device.Id);
                    if (existingDevice != null)
                    {
                        _appDbContext.Entry(existingDevice).CurrentValues.SetValues(device);

                        var result = await _appDbContext.SaveChangesAsync();

                        return await _appDbContext.Devices
                            .Where(d => d.Id == device.Id)
                            .Include(d => d.DeviceInfo)
                                .ThenInclude(di => di.MACAddresses)
                            .FirstOrDefaultAsync();
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
                var device = await _appDbContext.Devices.FindAsync(id);
                if (device != null)
                {
                    _appDbContext.Devices.Remove(device);
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

        public async Task<Device?> GetAsync(int id)
        {
            return await _appDbContext.Devices
                .Where(d => d.Id == id)
                .Include(d => d.DeviceInfo)
                    .ThenInclude(di => di.MACAddresses)
                .FirstOrDefaultAsync();
        }

        public async Task<Device?> GetByClientIdAndUuidAsync(int tenantId, int clientId, string? uuid)
        {
            if (uuid == null)
            {
                return null;
            }

            return await _appDbContext.Devices
                .Include(d => d.DeviceInfo)
                    .ThenInclude(di => di.MACAddresses)
                .Where( d => d.DeviceInfo.ClientId == clientId && d.DeviceInfo.Uuid == uuid && d.TenantId == tenantId)
                .FirstOrDefaultAsync();
        }

        public async Task<ICollection<Device>> GetAllAsync()
        {
            return await _appDbContext.Devices
                .Include(d => d.DeviceInfo)
                    .ThenInclude(di => di.MACAddresses)
                .ToListAsync();
        }

    }
}
