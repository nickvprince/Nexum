using Newtonsoft.Json;
using SharedComponents.Entities.WebEntities.Requests.UserRequests;
using SharedComponents.Entities.WebEntities.Responses.UserResponses;
using SharedComponents.Services.APIRequestServices;
using SharedComponents.Services.APIRequestServices.Interfaces;
using System.Text;

namespace App.Services.APIRequestServices
{
    public class APIRequestUserService : BaseAPIRequestService, IAPIRequestUserService
    {
        public APIRequestUserService(IConfiguration config, HttpClient httpClient, IHttpContextAccessor httpContextAccessor) : base(config, httpClient, httpContextAccessor)
        {
            AppendBaseAddress("User/");
        }

        public async Task<UserCreateResponse?> CreateAsync(UserCreateRequest request)
        {
            try
            {
                var content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("", content);
                var responseData = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<UserCreateResponse>(responseData);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<UserUpdateResponse?> UpdateAsync(UserUpdateRequest request)
        {
            try
            {
                var content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
                var response = await _httpClient.PutAsync("", content);
                var responseData = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<UserUpdateResponse>(responseData);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<bool> DeleteAsync(string id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"{id}");
                var responseData = await response.Content.ReadAsStringAsync();
                response.EnsureSuccessStatusCode();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<UserResponse?> GetByIdAsync(string id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"By-Id/{id}");
                var responseData = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<UserResponse>(responseData);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<UserResponse?> GetByUserNameAsync(string username)
        {
            try
            {
                var response = await _httpClient.GetAsync($"By-Username/{username}");
                var responseData = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<UserResponse>(responseData);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<ICollection<UserResponse>?> GetAllAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("");
                var responseData = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<ICollection<UserResponse>>(responseData);
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
