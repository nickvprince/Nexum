using API.DataAccess;
using Microsoft.EntityFrameworkCore;
using SharedComponents.DbServices;
using SharedComponents.Entities;

namespace API.Services
{
    public class DbAlertService : IDbAlertService
    {
        private readonly AppDbContext _appDbContext;
        public DbAlertService(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public async Task<DeviceAlert?> CreateAsync(DeviceAlert? alert)
        {
            if (alert != null)
            {
                try
                {
                    await _appDbContext.Alerts.AddAsync(alert);
                    var result = await _appDbContext.SaveChangesAsync();
                    if (result > 0)
                    {
                        return await _appDbContext.Alerts
                            .Where(a => a.Id == alert.Id)
                            .FirstAsync();
                    }
                }
                catch (Exception ex)
                {

                    Console.WriteLine($"An error occurred while creating the alert: {ex.Message}");
                }
            }
            return null;
        }

        public async Task<DeviceAlert?> UpdateAsync(DeviceAlert? alert)
        {
            if(alert != null)
            {
                try
                {
                    var existingAlert = await _appDbContext.Alerts
                        .Where(a => a.Id == alert.Id)
                        .FirstAsync();
                    if (existingAlert != null)
                    {
                        _appDbContext.Entry(existingAlert).CurrentValues.SetValues(alert);

                        var result = await _appDbContext.SaveChangesAsync();
                        if (result >= 0)
                        {
                            return await _appDbContext.Alerts
                                .Where(a => a.Id == alert.Id)
                                .FirstAsync();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred while updating the alert: {ex.Message}");
                }
            }
            return null;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var alert = await _appDbContext.Alerts
                    .Where(i => i.Id == id)
                    .FirstAsync();
                if (alert != null)
                {
                    alert.IsDeleted = true;
                    _appDbContext.Alerts.Update(alert);
                    var result = await _appDbContext.SaveChangesAsync();
                    if (result > 0)
                    {
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while deleting the alert: {ex.Message}");
            }
            return false;
        }

        public async Task<DeviceAlert?> GetAsync(int id)
        {
            try 
            {
                var alerts = await _appDbContext.Alerts
                    .Where(a => a.Id == id)
                    .FirstAsync();
                if (alerts != null)
                {
                    return alerts;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while getting the alert: {ex.Message}");
            }
            return null;
        }

        public async Task<ICollection<DeviceAlert>?> GetAllAsync()
        {
            try
            {
                var alerts = await _appDbContext.Alerts.ToListAsync();
                if (alerts != null)
                {
                    if (alerts.Any())
                    { 
                        return alerts;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while getting all alerts: {ex.Message}");
            }
            return null;
        }

        public async Task<ICollection<DeviceAlert>?> GetAllByDeviceIdAsync(int deviceId)
        {
            try
            {
                var alerts = await _appDbContext.Alerts
                    .Where(a => a.DeviceId == deviceId)
                    .ToListAsync();
                if (alerts != null)
                {
                    if (alerts.Any())
                    {
                        return alerts;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while getting alerts by device id: {ex.Message}");
            }
            return null;
        }

        public async Task<ICollection<DeviceAlert>?> GetAllByTenantIdAsync(int tenantId)
        {
            try
            {
                var alerts = await _appDbContext.Alerts
                    .Where(a => a.Device.TenantId == tenantId)
                    .ToListAsync();
                if (alerts != null)
                {
                    if (alerts.Any())
                    {
                        return alerts;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while getting alerts by tenant id: {ex.Message}");
            }
            return null;
        }
    }
}
