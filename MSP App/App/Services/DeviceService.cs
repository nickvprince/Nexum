using Newtonsoft.Json;
using SharedComponents.Entities;
using SharedComponents.Services;
using SharedComponents.WebRequestEntities.DeviceRequests;
using System.Text;

namespace App.Services
{
    public class DeviceService : BaseService, IDeviceService
    {
        public DeviceService(IConfiguration config, HttpClient httpClient) : base(config, httpClient)
        {
            if (_httpClient.BaseAddress != null)
            {
                _httpClient.BaseAddress = new Uri(_httpClient.BaseAddress, "Device/");
            }
            else
            {
                throw new InvalidOperationException("BaseAddress is not set.");
            }
        }

        public async Task<Device?> CreateAsync(DeviceCreateRequest request)
        {
            try
            {
                var content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("", content);
                var responseData = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<Device>(responseData);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<Device?> UpdateAsync(DeviceUpdateRequest request)
        {
            try
            {
                var content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
                var response = await _httpClient.PutAsync("", content);
                var responseData = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<Device>(responseData);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<Device?> UpdateStatusAsync(DeviceUpdateStatusRequest request)
        {
            try
            {
                var content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
                var response = await _httpClient.PutAsync("Status", content);
                var responseData = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<Device>(responseData);
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

        public async Task<Device?> GetAsync(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{id}");
                var responseData = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<Device>(responseData);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<ICollection<Device>?> GetAllAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("");
                var responseData = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<ICollection<Device>>(responseData);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<ICollection<Device>?> GetAllByTenantIdAsync(int tenantId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"By-Tenant/{tenantId}");
                var responseData = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<ICollection<Device>>(responseData);
            }
            catch (Exception)
            {
                return null;
            }
        }

    }
}
