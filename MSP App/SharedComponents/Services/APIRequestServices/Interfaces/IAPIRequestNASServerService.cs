using SharedComponents.Entities.DbEntities;
using SharedComponents.Entities.WebEntities.Requests.NASServerRequests;

namespace SharedComponents.Services.APIRequestServices.Interfaces
{
    public interface IAPIRequestNASServerService
    {
        public Task<NASServer?> CreateAsync(NASServerCreateRequest request);
        public Task<NASServer?> UpdateAsync(NASServerUpdateRequest request);
        public Task<bool> DeleteAsync(int id);
        public Task<NASServer?> GetAsync(int id);
        public Task<ICollection<NASServer>?> GetAllAsync();
        public Task<ICollection<NASServer>?> GetAllByTenantIdAsync(int tenantId);
        public Task<ICollection<NASServer>?> GetAllByDeviceIdAsync(int deviceId);
    }
}