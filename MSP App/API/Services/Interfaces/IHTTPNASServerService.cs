using SharedComponents.RequestEntities.HTTP;
using SharedComponents.ResponseEntities.HTTP;
using SharedComponents.WebRequestEntities.NASServerRequests;

namespace API.Services.Interfaces
{
    public interface IHTTPNASServerService
    {
        public Task<CreateNASServerResponse?> CreateAsync(int tenantId, CreateNASServerRequest? request);
        public Task<bool?> UpdateAsync(int tenantId, UpdateNASServerRequest? request);
    }
}
