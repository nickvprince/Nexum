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

        public async Task<bool> CreateAsync(Device device)
        {
            if (device != null)
            {
                try
                {
                    // Add the device to the context
                    await _appDbContext.Devices.AddAsync(device);

                    // Save changes to the database
                    var result = await _appDbContext.SaveChangesAsync();

                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred while creating the device: {ex.Message}");
                }
            }
            return false;
        }
        public Task<bool> UpdateAsync(Device device)
        {
            throw new NotImplementedException();
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
                .Include(d => d.Tenant)
                .FirstOrDefaultAsync();
        }
        public async Task<ICollection<Device>> GetAllAsync()
        {
            return await _appDbContext.Devices
                .Include(d => d.DeviceInfo)
                .Include(d => d.Tenant)
                .ToListAsync();
        }

    }
}
