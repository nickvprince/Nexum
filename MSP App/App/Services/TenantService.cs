using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SharedComponents.Entities;
using SharedComponents.Services;
using SharedComponents.WebEntities.Requests.TenantRequests;
using System.Text;

namespace App.Services
{
    public class TenantService : BaseService, ITenantService
    {
        public TenantService(IConfiguration config, HttpClient httpClient) : base(config, httpClient)
        {
            if (_httpClient.BaseAddress != null)
            {
                _httpClient.BaseAddress = new Uri(_httpClient.BaseAddress, "Tenant/");
            }
            else
            {
                throw new InvalidOperationException("BaseAddress is not set.");
            }
        }

        public async Task<Tenant?> CreateAsync(TenantCreateRequest request)
        {
            try
            {
                var content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("", content);
                var responseData = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<Tenant>(responseData);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<Tenant?> UpdateAsync(TenantUpdateRequest request)
        {
            try
            {
                var content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
                var response = await _httpClient.PutAsync("", content);
                var responseData = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<Tenant>(responseData);
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

        public async Task<Tenant?> GetAsync(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{id}");
                var responseData = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<Tenant>(responseData);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<Tenant?> GetRichAsync(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{id}/Rich");
                var responseData = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<Tenant>(responseData);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<ICollection<Tenant>?> GetAllAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("");
                var responseData = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<ICollection<Tenant>>(responseData);
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
