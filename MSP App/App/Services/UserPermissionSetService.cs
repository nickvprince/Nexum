using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SharedComponents.Entities;
using SharedComponents.Services;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace App.Services
{
    public class UserPermissionSetService : BaseService, IUserPermissionSetService
    {
        public UserPermissionSetService(IConfiguration config, HttpClient httpClient) : base(config, httpClient)
        {
        }
        public Task<bool> CreateAsync(UserPermissionSet permissionSet)
        {
            throw new NotImplementedException();
        }

        public Task<bool> EditAsync(UserPermissionSet permissionSet)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeleteAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<UserPermissionSet> GetAsync(int id)
        {
            throw new NotImplementedException();
        }

        public async Task<ICollection<UserPermissionSet>?> GetAllAsync()
        {
            var response = await _httpClient.GetAsync("api/UserPermissionSet/Get");
            var responseData = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<ICollection<UserPermissionSet>>(responseData);
        }
    }
}
