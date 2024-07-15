using SharedComponents.Entities;
using SharedComponents.WebEntities.Requests.UserRequests;
using SharedComponents.WebEntities.Responses.UserResponses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedComponents.Services
{
    public interface IUserService
    {
        public Task<UserCreateResponse?> CreateAsync(UserCreateRequest request);
        public Task<UserUpdateResponse?> UpdateAsync(UserUpdateRequest request);
        public Task<bool> DeleteAsync(string id);
        public Task<UserResponse?> GetByIdAsync(string username);
        public Task<UserResponse?> GetByUserNameAsync(string username);
        public Task<ICollection<UserResponse>?> GetAllAsync();
    }
}
