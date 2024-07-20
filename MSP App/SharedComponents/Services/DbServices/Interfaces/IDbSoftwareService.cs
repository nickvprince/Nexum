using SharedComponents.Entities.DbEntities;

namespace SharedComponents.Services.DbServices.Interfaces
{
    public interface IDbSoftwareService
    {
        public Task<SoftwareFile?> CreateAsync(SoftwareFile? softwareFile);
        public Task<SoftwareFile?> GetAsync(int id);
        public Task<SoftwareFile?> GetLatestNexumAsync();
        public Task<SoftwareFile?> GetLatestNexumServerAsync();
        public Task<SoftwareFile?> GetLatestNexumServiceAsync();
        public Task<ICollection<SoftwareFile>?> GetAllAsync();
    }
}
