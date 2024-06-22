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

        public async Task<DeviceAlert?> CreateAsync(DeviceAlert alert)
        {
            if (alert != null)
            {
                try
                {
                    // Add the alert to the context
                    await _appDbContext.Alerts.AddAsync(alert);

                    // Save changes to the database
                    var result = await _appDbContext.SaveChangesAsync();

                    return await _appDbContext.Alerts
                        .Where(a => a.Id == alert.Id)
                        .FirstOrDefaultAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred while creating the alert: {ex.Message}");
                }
            }
            return null;
        }

        public async Task<DeviceAlert?> GetAsync(int id)
        {
            return await _appDbContext.Alerts
                .Where(a => a.Id == id)
                .FirstOrDefaultAsync();
        }

        public async Task<ICollection<DeviceAlert>> GetAllAsync()
        {
            return await _appDbContext.Alerts
                .ToListAsync();
        }
    }
}
