using SharedComponents.Entities.DbEntities;

namespace SharedComponents.Services.DbServices.Interfaces
{
    public interface IDbNASServerService
    {
        public Task<NASServer?> CreateAsync(NASServer? nasServer);
        public Task<NASServer?> UpdateAsync(NASServer? nasServer);
        public Task<bool> DeleteAsync(int id);
        public Task<NASServer?> GetAsync(int id);
        public Task<NASServer?> GetByBackupServerIdAsync(int tenantId, int backupServerId);
        public Task<ICollection<NASServer>?> GetAllAsync();
        public Task<ICollection<NASServer>?> GetAllByTenantIdAsync(int tenantId);
        public Task<ICollection<NASServer>?> GetAllByDeviceIdAsync(int deviceId);
    }
}
