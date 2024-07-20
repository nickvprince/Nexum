using SharedComponents.Entities.DbEntities;
using SharedComponents.Entities.WebEntities.Requests.TenantRequests;

namespace SharedComponents.Services.APIRequestServices.Interfaces
{
    public interface IAPIRequestTenantService
    {
        public Task<Tenant?> CreateAsync(TenantCreateRequest request);
        public Task<Tenant?> UpdateAsync(TenantUpdateRequest request);
        public Task<bool> DeleteAsync(int id);
        public Task<Tenant?> GetAsync(int id);
        public Task<Tenant?> GetRichAsync(int id);
        public Task<ICollection<Tenant>?> GetAllAsync();
    }
}
