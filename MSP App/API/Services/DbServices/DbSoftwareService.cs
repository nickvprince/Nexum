using API.DataAccess;
using Microsoft.EntityFrameworkCore;
using SharedComponents.Entities.DbEntities;
using SharedComponents.Services.DbServices.Interfaces;

namespace API.Services.DbServices
{
    public class DbSoftwareService : IDbSoftwareService
    {
        private readonly AppDbContext _appDbContext;
        public DbSoftwareService(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }
        public async Task<SoftwareFile?> CreateAsync(SoftwareFile? softwareFile)
        {
            if (softwareFile != null)
            {
                try
                {
                    await _appDbContext.SoftwareFiles.AddAsync(softwareFile);
                    var result = await _appDbContext.SaveChangesAsync();
                    if (result > 0)
                    {
                        return await _appDbContext.SoftwareFiles
                            .Where(s => s.Id == softwareFile.Id)
                            .FirstAsync();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred while creating the software: {ex.Message}");
                }
            }
            return null;
        }

        public async Task<SoftwareFile?> GetAsync(int id)
        {
            try
            {
                return await _appDbContext.SoftwareFiles
                    .Where(s => s.Id == id)
                    .FirstAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while getting the software: {ex.Message}");
            }
            return null;
        }

        public async Task<SoftwareFile?> GetLatestNexumAsync()
        {
            try
            {
                return await _appDbContext.SoftwareFiles
                    .Where(s => s.FileType == SoftwareFileType.Nexum)
                    .OrderByDescending(s => s.Version)
                    .FirstAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while getting the latest software: {ex.Message}");
            }
            return null;
        }

        public async Task<SoftwareFile?> GetLatestNexumServerAsync()
        {
            try
            {
                return await _appDbContext.SoftwareFiles
                    .Where(s => s.FileType == SoftwareFileType.NexumServer)
                    .OrderByDescending(s => s.Version)
                    .FirstAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while getting the latest software: {ex.Message}");
            }
            return null;
        }

        public async Task<SoftwareFile?> GetLatestNexumServiceAsync()
        {
            try
            {
                return await _appDbContext.SoftwareFiles
                    .Where(s => s.FileType == SoftwareFileType.NexumService)
                    .OrderByDescending(s => s.Version)
                    .FirstAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while getting the latest software: {ex.Message}");
            }
            return null;
        }
        public async Task<ICollection<SoftwareFile>?> GetAllAsync()
        {
            try
            {
                return await _appDbContext.SoftwareFiles
                    .OrderByDescending(s => s.Version)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while getting all software: {ex.Message}");
            }
            return null;
        }
    }
}
