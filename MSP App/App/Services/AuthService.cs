using App.Models;
using Newtonsoft.Json.Linq;
using SharedComponents.Entities;
using SharedComponents.Services;
using System.Text.Json;
using SharedComponents.WebEntities.Requests.AuthRequests;
using Newtonsoft.Json;
using System.Text;
using SharedComponents.WebEntities.Responses.AuthResponses;

namespace App.Services
{
    public class AuthService : BaseService, IAuthService
    {
        public AuthService(IConfiguration config, HttpClient httpClient, IHttpContextAccessor httpContextAccessor) : base(config, httpClient, httpContextAccessor)
        {
            AppendBaseAddress("Auth/");
        }

        public async Task<AuthLoginResponse?> LoginAsync(AuthLoginRequest request)
        {
            try 
            {
                var content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("Login", content);
                var responseData = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<AuthLoginResponse>(responseData);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<AuthLoginResponse?> RefreshAsync(AuthRefreshRequest request)
        {
            try
            {
                var content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("Refresh", content);
                var responseData = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<AuthLoginResponse>(responseData);
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
