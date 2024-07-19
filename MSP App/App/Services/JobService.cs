using Newtonsoft.Json;
using SharedComponents.Entities;
using SharedComponents.Services;
using SharedComponents.WebEntities.Requests.JobRequests;
using System.Text;

namespace App.Services
{
    public class JobService : BaseService, IJobService
    {
        public JobService(IConfiguration config, HttpClient httpClient, IHttpContextAccessor httpContextAccessor) : base(config, httpClient, httpContextAccessor)
        {
            AppendBaseAddress("Job/");
        }

        public async Task<DeviceJob?> CreateAsync(JobCreateRequest request)
        {
            try
            {
                var content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("", content);
                var responseData = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<DeviceJob>(responseData);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<DeviceJob?> UpdateAsync(JobUpdateRequest request)
        {
            try
            {
                var content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
                var response = await _httpClient.PutAsync("", content);
                var responseData = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<DeviceJob>(responseData);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<DeviceJob?> UpdateStatusAsync(JobUpdateStatusRequest request)
        {
            try
            {
                var content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
                var response = await _httpClient.PutAsync("Status", content);
                var responseData = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<DeviceJob>(responseData);
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

        public async Task<DeviceJob?> GetAsync(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{id}");
                var responseData = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<DeviceJob>(responseData);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<ICollection<DeviceJob>?> GetAllAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("");
                var responseData = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<ICollection<DeviceJob>>(responseData);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<ICollection<DeviceJob>?> GetAllByDeviceIdAsync(int deviceId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"By-Device/{deviceId}");
                var responseData = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<ICollection<DeviceJob>>(responseData);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<ICollection<DeviceJob>?> GetAllByTenantIdAsync(int tenantId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"By-Tenant/{tenantId}");
                var responseData = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<ICollection<DeviceJob>>(responseData);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<bool> StartAsync(int id)
        {
            try
            {
                var response = await _httpClient.PostAsync($"{id}/Start", null);
                var responseData = await response.Content.ReadAsStringAsync();
                response.EnsureSuccessStatusCode();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> PauseAsync(int id)
        {
            try
            {
                var response = await _httpClient.PostAsync($"{id}/Pause", null);
                var responseData = await response.Content.ReadAsStringAsync();
                response.EnsureSuccessStatusCode();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> ResumeAsync(int id)
        {
            try
            {
                var response = await _httpClient.PostAsync($"{id}/Resume", null);
                var responseData = await response.Content.ReadAsStringAsync();
                response.EnsureSuccessStatusCode();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> StopAsync(int id)
        {
            try
            {
                var response = await _httpClient.PostAsync($"{id}/Stop", null);
                var responseData = await response.Content.ReadAsStringAsync();
                response.EnsureSuccessStatusCode();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> RefreshAsync(int id)
        {
            try
            {
                var response = await _httpClient.PostAsync($"{id}/Refresh", null);
                var responseData = await response.Content.ReadAsStringAsync();
                response.EnsureSuccessStatusCode();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
