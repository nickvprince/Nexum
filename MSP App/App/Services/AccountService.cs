using App.Models;
using SharedComponents.Entities;
using SharedComponents.RequestEntities;
using SharedComponents.Services;

namespace App.Services
{
    public class AccountService : BaseService, IAccountService
    {
        public AccountService(IConfiguration config, HttpClient httpClient) : base(config, httpClient)
        {
        }

        public Task<bool> EditAsync(User user)
        {
            throw new NotImplementedException();
        }

        public Task<User> GetUserAsync(string username)
        {
            throw new NotImplementedException();
        }

        public async Task<User> LoginAsync(string username, string password)
        {
            LoginRequest loginRequest = new LoginRequest
            {
                Username = username,
                Password = password
            };

            var response = await _httpClient.PostAsJsonAsync("api/Auth/Login", loginRequest);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<User>();
            }
            return null;
        }
    }
}
