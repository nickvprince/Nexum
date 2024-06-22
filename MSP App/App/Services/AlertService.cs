using Newtonsoft.Json;
using SharedComponents.Entities;
using SharedComponents.Services;

namespace App.Services
{
    public class AlertService : BaseService, IAlertService
    {
        public AlertService(IConfiguration config, HttpClient httpClient) : base(config, httpClient)
        {
        }

        public Task<bool> CreateAsync(DeviceAlert alert)
        {
            throw new NotImplementedException();
        }

        public Task<bool> EditAsync(DeviceAlert alert)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeleteAsync(string id)
        {
            throw new NotImplementedException();
        }

        public Task<DeviceAlert?> GetAsync(string id)
        {
            throw new NotImplementedException();
        }

        public async Task<ICollection<DeviceAlert>?> GetAllAsync()
        {
            var response = await _httpClient.GetAsync("Alert");
            var responseData = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<ICollection<DeviceAlert>>(responseData);
        }
    }
}
