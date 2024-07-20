using Newtonsoft.Json;
using SharedComponents.Entities.DbEntities;
using SharedComponents.Entities.WebEntities.Requests.LogRequests;
using SharedComponents.Services.APIRequestServices;
using SharedComponents.Services.APIRequestServices.Interfaces;
using System.Text;

namespace App.Services.APIRequestServices
{
    public class APIRequestLogService : BaseAPIRequestService, IAPIRequestLogService
    {
        public APIRequestLogService(IConfiguration config, HttpClient httpClient, IHttpContextAccessor httpContextAccessor) : base(config, httpClient, httpContextAccessor)
        {
            AppendBaseAddress("Log/");
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
