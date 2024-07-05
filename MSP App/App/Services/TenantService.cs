using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SharedComponents.Entities;
using SharedComponents.Services;
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

        public async Task<Tenant?> CreateAsync(Tenant tenant)
        {
            try
            {
                var content = new StringContent(JsonConvert.SerializeObject(tenant), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("", content);
                var responseData = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<Tenant>(responseData);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<Tenant?> EditAsync(Tenant tenant)
        {
            try
            {
                var content = new StringContent(JsonConvert.SerializeObject(tenant), Encoding.UTF8, "application/json");
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

        public async Task<ICollection<Tenant>?> GetAllAsync()
        {
            var response = await _httpClient.GetAsync("");
            var responseData = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<ICollection<Tenant>>(responseData);
        }
    }
}
