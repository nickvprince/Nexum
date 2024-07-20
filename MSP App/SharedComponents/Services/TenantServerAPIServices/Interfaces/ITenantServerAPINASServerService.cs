using SharedComponents.Entities.TenantServerHttpEntities.Requests;
using SharedComponents.Entities.TenantServerHttpEntities.Responses;

namespace SharedComponents.Services.TenantServerAPIServices.Interfaces
{
    public interface ITenantServerAPINASServerService
    {
        public Task<CreateNASServerResponse?> CreateAsync(int tenantId, CreateNASServerRequest? request);
        public Task<bool?> UpdateAsync(int tenantId, UpdateNASServerRequest? request);
        public Task<bool?> DeleteAsync(int tenantId, DeleteNASServerRequest? request);
        public Task<GetNASServerResponse?> GetAsync(int tenantId, int id);
    }
}
