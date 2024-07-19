using Newtonsoft.Json;
using SharedComponents.Entities;
using SharedComponents.Services;
using SharedComponents.WebEntities.Requests.InstallationKeyRequests;
using System.Text;

namespace App.Services
{
    public class InstallationKeyService : BaseService, IInstallationKeyService
    {
        public InstallationKeyService(IConfiguration config, HttpClient httpClient, IHttpContextAccessor httpContextAccessor) : base(config, httpClient, httpContextAccessor)
        {
            AppendBaseAddress("InstallationKey/");
        }

        public async Task<InstallationKey?> CreateAsync(InstallationKeyCreateRequest request)
        {
            try
            {
                var content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("", content);
                var responseData = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<InstallationKey>(responseData);
            }
            catch (Exception)
            {
                return null;
            }
        }
        public async Task<InstallationKey?> UpdateAsync(InstallationKeyUpdateRequest request)
        {
            try
            {
                var content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
                var response = await _httpClient.PutAsync("", content);
                var responseData = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<InstallationKey>(responseData);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<bool> DeleteAsync(int id)
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
        public async Task<InstallationKey?> GetAsync(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{id}");
                var responseData = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<InstallationKey>(responseData);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<ICollection<InstallationKey>?> GetAllAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("");
                var responseData = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<ICollection<InstallationKey>>(responseData);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<ICollection<InstallationKey>?> GetAllByTenantIdAsync(int tenantId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"By-Tenant/{tenantId}");
                var responseData = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<ICollection<InstallationKey>>(responseData);
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
