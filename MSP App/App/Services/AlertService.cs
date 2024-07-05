using Newtonsoft.Json;
using SharedComponents.Entities;
using SharedComponents.Services;
using System.Text;

namespace App.Services
{
    public class AlertService : BaseService, IAlertService
    {
        public AlertService(IConfiguration config, HttpClient httpClient) : base(config, httpClient)
        {
            if (_httpClient.BaseAddress != null)
            {
                _httpClient.BaseAddress = new Uri(_httpClient.BaseAddress, "Alert/");
            }
            else
            {
                throw new InvalidOperationException("BaseAddress is not set.");
            }
        }

        public async Task<DeviceAlert?> CreateAsync(DeviceAlert alert)
        {
            try
            {
                var content = new StringContent(JsonConvert.SerializeObject(alert), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("", content);
                var responseData = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<DeviceAlert>(responseData);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<DeviceAlert?> EditAsync(DeviceAlert alert)
        {
            try
            {
                var content = new StringContent(JsonConvert.SerializeObject(alert), Encoding.UTF8, "application/json");
                var response = await _httpClient.PutAsync("", content);
                var responseData = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<DeviceAlert>(responseData);
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

        public async Task<DeviceAlert?> GetAsync(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{id}");
                var responseData = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<DeviceAlert>(responseData);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<ICollection<DeviceAlert>?> GetAllAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("");
                var responseData = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<ICollection<DeviceAlert>>(responseData);
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
