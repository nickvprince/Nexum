using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SharedComponents.Entities;
using SharedComponents.Services;
using System.Text.Json;

namespace App.Services
{
    public class UserService : BaseService, IUserService
    {
        public UserService(IConfiguration config, HttpClient httpClient) : base(config, httpClient)
        {
        }

        public Task<bool> DeleteAsync(string username)
        {
            throw new NotImplementedException();
        }

        public Task<bool> EditAsync(User user)
        {
            throw new NotImplementedException();
        }

        public Task<bool> CreateAsync(User user)
        {
            throw new NotImplementedException();
        }

        public async Task<User?> GetAsync(string username)
        {
            var response = await _httpClient.GetAsync($"api/User/Get/{username}");
            var responseData = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<User>(responseData);
        }

        public async Task<ICollection<User>?> GetAllAsync()
        {
            var response = await _httpClient.GetAsync("api/User/Get");
            var responseData = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<ICollection<User>>(responseData);
        }
    }
}
