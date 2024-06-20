using API.DataAccess;
using Microsoft.EntityFrameworkCore;
using SharedComponents.DbServices;
using SharedComponents.Entities;

namespace API.Services
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
                    // Add the software to the context
                    await _appDbContext.SoftwareFiles.AddAsync(softwareFile);

                    // Save changes to the database
                    var result = await _appDbContext.SaveChangesAsync();

                    return await _appDbContext.SoftwareFiles
                        .Where(s => s.Id == softwareFile.Id)
                        .FirstOrDefaultAsync();
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
            return await _appDbContext.SoftwareFiles
                .Where(s => s.Id == id)
                .FirstOrDefaultAsync();
        }

        public async Task<SoftwareFile?> GetLatestNexumAsync()
        {
            return await _appDbContext.SoftwareFiles
                .Where(s => s.FileType == SoftwareFileType.Nexum)
                .OrderByDescending(s => s.Version)
                .FirstOrDefaultAsync();
        }

        public async Task<SoftwareFile?> GetLatestNexumServerAsync()
        {
            return await _appDbContext.SoftwareFiles
                .Where(s => s.FileType == SoftwareFileType.NexumServer)
                .OrderByDescending(s => s.Version)
                .FirstOrDefaultAsync();
        }

        public async Task<SoftwareFile?> GetLatestNexumServiceAsync()
        {
            return await _appDbContext.SoftwareFiles
                .Where(s => s.FileType == SoftwareFileType.NexumService)
                .OrderByDescending(s => s.Version)
                .FirstOrDefaultAsync();
        }
        public async Task<ICollection<SoftwareFile>> GetAllAsync()
        {
            return await _appDbContext.SoftwareFiles
                .OrderByDescending(s => s.Version)
                .ToListAsync();
        }
    }
}
