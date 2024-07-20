using SharedComponents.Entities.WebEntities.Requests.UserRequests;
using SharedComponents.Entities.WebEntities.Responses.UserResponses;

namespace SharedComponents.Services.APIRequestServices.Interfaces
{
    public interface IAPIRequestUserService
    {
        public Task<UserCreateResponse?> CreateAsync(UserCreateRequest request);
        public Task<UserUpdateResponse?> UpdateAsync(UserUpdateRequest request);
        public Task<bool> DeleteAsync(string id);
        public Task<UserResponse?> GetByIdAsync(string username);
        public Task<UserResponse?> GetByUserNameAsync(string username);
        public Task<ICollection<UserResponse>?> GetAllAsync();
    }
}
