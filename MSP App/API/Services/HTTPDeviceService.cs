using API.DataAccess;
using API.Services.Interfaces;
using Newtonsoft.Json;
using SharedComponents.Utilities;
using System.Text;

namespace API.Services
{
    public class HTTPDeviceService : BaseHTTPService, IHTTPDeviceService
    {
        public HTTPDeviceService(IConfiguration config, HttpClient httpClient, AppDbContext appDbContext) : base(config, httpClient, appDbContext)
        {
            //Use if tenant server api is updated with proper routes
            /*if (_httpClient.BaseAddress != null)
            {
                _httpClient.BaseAddress = new Uri(_httpClient.BaseAddress, "Device/");
            }
            else
            {
                throw new InvalidOperationException("BaseAddress is not set.");
            }*/
        }

        public async Task<bool> ForceDeviceCheckinAsync(int tenantId, int client_id)
        {
            try
            {
                if (await InitiallizeHttpClient(tenantId))
                {
                    var content = new StringContent(JsonConvert.SerializeObject(client_id, new InvalidJsonUtilities()), Encoding.UTF8, "application/json");
                    var response = await _httpClient.PostAsync("force_checkin", content);
                    var responseData = await response.Content.ReadAsStringAsync();
                    response.EnsureSuccessStatusCode();
                    return true;
                }
            }
            catch (Exception)
            {
                Console.WriteLine("Error forcing device checkin on the server.");
            }
            return false;
        }

        public async Task<bool> ForceDeviceUpdateAsync(int tenantId, int client_id)
        {
            try
            {
                if (await InitiallizeHttpClient(tenantId))
                {
                    var content = new StringContent(JsonConvert.SerializeObject(client_id, new InvalidJsonUtilities()), Encoding.UTF8, "application/json");
                    var response = await _httpClient.PostAsync("force_update", content);
                    var responseData = await response.Content.ReadAsStringAsync();
                    response.EnsureSuccessStatusCode();
                    return true;
                }
            }
            catch (Exception)
            {
                Console.WriteLine("Error forcing device update on the server.");
            }
            return false;
        }

        public Task<bool> GetDeviceBeatAsync(int tenantId, int client_id)
        {
            throw new NotImplementedException();
        }
    }
}
