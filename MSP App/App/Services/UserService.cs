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

        public Task<bool> EditAsync(ApplicationUser user)
        {
            throw new NotImplementedException();
        }

        public Task<bool> CreateAsync(ApplicationUser user)
        {
            throw new NotImplementedException();
        }

        public async Task<ApplicationUser?> GetAsync(string username)
        {
            var response = await _httpClient.GetAsync($"api/User/{username}");
            var responseData = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<ApplicationUser>(responseData);
        }

        public async Task<ICollection<ApplicationUser>?> GetAllAsync()
        {
            var response = await _httpClient.GetAsync("api/User");
            var responseData = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<ICollection<ApplicationUser>>(responseData);
        }
    }
}
