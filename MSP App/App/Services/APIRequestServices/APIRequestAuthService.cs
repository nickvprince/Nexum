using Newtonsoft.Json;
using System.Text;
using SharedComponents.Services.APIRequestServices;
using SharedComponents.Services.APIRequestServices.Interfaces;
using SharedComponents.Entities.WebEntities.Requests.AuthRequests;
using SharedComponents.Entities.WebEntities.Responses.AuthResponses;

namespace App.Services.APIRequestServices
{
    public class APIRequestAuthService : BaseAPIRequestService, IAPIRequestAuthService
    {
        public APIRequestAuthService(IConfiguration config, HttpClient httpClient, IHttpContextAccessor httpContextAccessor) : base(config, httpClient, httpContextAccessor)
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
