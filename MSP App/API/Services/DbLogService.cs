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
                    await _appDbContext.Logs.AddAsync(log);

                    // Save changes to the database
                    var result = await _appDbContext.SaveChangesAsync();

                    return await _appDbContext.Logs
                        .Where(l => l.Id == log.Id)
                        .FirstOrDefaultAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred while creating the log: {ex.Message}");
                }
            }
            return null;
        }

        public async Task<DeviceLog?> GetAsync(int id)
        {
            return await _appDbContext.Logs
                .Where(l => l.Id == id)
                .FirstOrDefaultAsync();
        }

        public async Task<ICollection<DeviceLog>> GetAllAsync()
        {
            return await _appDbContext.Logs
                .ToListAsync();
        }
    }
}
