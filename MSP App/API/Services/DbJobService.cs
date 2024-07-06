using API.DataAccess;
using Microsoft.EntityFrameworkCore;
using SharedComponents.DbServices;
using SharedComponents.Entities;

namespace API.Services
{
    public class DbJobService : IDbJobService
    {
        private readonly AppDbContext _appDbContext;
        public DbJobService(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public async Task<DeviceJob?> CreateAsync(DeviceJob? job)
        {
            if (job != null)
            {
                try
                {
                    // Add the job to the context
                    await _appDbContext.DeviceJobs.AddAsync(job);

                    // Save changes to the database
                    var result = await _appDbContext.SaveChangesAsync();
                    if (result > 0)
                    {
                        return await _appDbContext.DeviceJobs
                            .Where(j => j.Id == job.Id)
                            .FirstAsync();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred while creating the job: {ex.Message}");
                }
            }
            return null;
        }

        public async Task<DeviceJob?> UpdateAsync(DeviceJob? job)
        {
            if (job != null)
            {
                try
                {
                    // Get the existing job from the context
                    var existingJob = await _appDbContext.DeviceJobs
                        .Where(j => j.Id == job.Id)
                        .FirstAsync();
                    if (existingJob != null)
                    {
                        // Update the existing job with the new values
                        _appDbContext.Entry(existingJob).CurrentValues.SetValues(job);

                        // Save changes to the database
                        var result = await _appDbContext.SaveChangesAsync();
                        if (result >= 0)
                        {
                            return await _appDbContext.DeviceJobs
                                .Where(j => j.Id == job.Id)
                                .FirstAsync();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred while updating the job: {ex.Message}");
                }
            }
            return null;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                // Get the job from the context
                var job = await _appDbContext.DeviceJobs
                    .Where(j => j.Id == id)
                    .FirstAsync();
                if (job != null)
                {
                    // Remove the job from the context
                    _appDbContext.DeviceJobs.Remove(job);

                    // Save changes to the database
                    var result = await _appDbContext.SaveChangesAsync();
                    if (result > 0)
                    {
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while deleting the job: {ex.Message}");
            }
            return false;
        }

        public async Task<DeviceJob?> GetAsync(int id)
        {
            try
            {
                var job = await _appDbContext.DeviceJobs
                    .Where(j => j.Id == id)
                    .FirstAsync();
                if (job != null)
                {
                    return job;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while getting the job: {ex.Message}");
            }
            return null;
        }

        public async Task<ICollection<DeviceJob>?> GetAllAsync()
        {
            try
            {
                var jobs = await _appDbContext.DeviceJobs.ToListAsync();
                if (jobs != null)
                {
                    if (jobs.Any())
                    {
                        return jobs;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while getting all jobs: {ex.Message}");
            }
            return null;
        }

        public async Task<ICollection<DeviceJob>?> GetAllByDeviceIdAsync(int deviceId)
        {
            try
            {
                var jobs = await _appDbContext.DeviceJobs
                    .Where(j => j.DeviceId == deviceId)
                    .ToListAsync();
                if (jobs != null)
                {
                    if (jobs.Any())
                    {
                        return jobs;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while getting all jobs by device id: {ex.Message}");
            }
            return null;
        }

        public async Task<ICollection<DeviceJob>?> GetAllByTenantIdAsync(int tenantId)
        {
            try
            {
                var jobs = await _appDbContext.DeviceJobs
                    .Where(j => j.Device.TenantId == tenantId)
                    .ToListAsync();
                if (jobs != null)
                {
                    if (jobs.Any())
                    {
                        return jobs;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while getting all jobs by tenant id: {ex.Message}");
            }
            return null;
        }
    }
}
