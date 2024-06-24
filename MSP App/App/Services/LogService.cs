using Newtonsoft.Json;
using SharedComponents.Entities;
using SharedComponents.Services;
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

        public async Task<DeviceLog?> CreateAsync(DeviceLog log)
        {
            try
            {
                var content = new StringContent(JsonConvert.SerializeObject(log), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("", content);
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
    }
}
