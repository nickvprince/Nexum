using Newtonsoft.Json;
using SharedComponents.Entities;
using SharedComponents.Services;
using SharedComponents.WebEntities.Requests.LogRequests;
using System.Text;

namespace App.Services
{
    public class LogService : BaseService, ILogService
    {
        public LogService(IConfiguration config, HttpClient httpClient) : base(config, httpClient)
        {
            if (_httpClient.BaseAddress != null)
            {
                _httpClient.BaseAddress = new Uri(_httpClient.BaseAddress, "Log/");
            }
            else
            {
                throw new InvalidOperationException("BaseAddress is not set.");
            }
        }

        public async Task<DeviceLog?> CreateAsync(LogCreateRequest request)
        {
            try
            {
                var content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("", content);
                var responseData = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<DeviceLog>(responseData);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<DeviceLog?> UpdateAsync(LogUpdateRequest request)
        {
            try
            {
                var content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
                var response = await _httpClient.PutAsync("", content);
                var responseData = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<DeviceLog>(responseData);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<DeviceLog?> AcknowledgeAsync(int id)
        {
            try
            {
                var content = new StringContent(JsonConvert.SerializeObject(""), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"{id}/Acknowledge", content);
                var responseData = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<DeviceLog>(responseData);
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

        public async Task<DeviceLog?> GetAsync(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{id}");
                var responseData = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<DeviceLog>(responseData);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<ICollection<DeviceLog>?> GetAllAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("");
                var responseData = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<ICollection<DeviceLog>>(responseData);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<ICollection<DeviceLog>?> GetAllByDeviceIdAsync(int deviceId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"By-Device/{deviceId}");
                var responseData = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<ICollection<DeviceLog>>(responseData);
            }
            catch (Exception)
            {
                return null;
            
            }
        }

        public async Task<ICollection<DeviceLog>?> GetAllByTenantIdAsync(int tenantId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"By-Tenant/{tenantId}");
                var responseData = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<ICollection<DeviceLog>>(responseData);
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
