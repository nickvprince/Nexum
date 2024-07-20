using SharedComponents.Entities.WebEntities.Requests.AuthRequests;
using SharedComponents.Entities.WebEntities.Responses.AuthResponses;

namespace SharedComponents.Services.APIRequestServices.Interfaces
{
    public interface IAPIRequestAuthService
    {
        public Task<AuthLoginResponse?> LoginAsync(AuthLoginRequest request);
        public Task<AuthLoginResponse?> RefreshAsync(AuthRefreshRequest request);
    }
}
