using Newtonsoft.Json;
using SharedComponents.Entities;
using SharedComponents.Services;
using SharedComponents.WebEntities.Requests.BackupRequests;
using System.Text;

namespace App.Services
{
    public class BackupService : BaseService, IBackupService
    {
        public BackupService(IConfiguration config, HttpClient httpClient, IHttpContextAccessor httpContextAccessor) : base(config, httpClient, httpContextAccessor)
        {
            AppendBaseAddress("Backup/");
        }

        public async Task<DeviceBackup?> CreateAsync(BackupCreateRequest request)
        {
            try
            {
                var content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("", content);
                var responseData = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<DeviceBackup>(responseData);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<DeviceBackup?> UpdateAsync(BackupUpdateRequest request)
        {
            try
            {
                var content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
                var response = await _httpClient.PutAsync("", content);
                var responseData = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<DeviceBackup>(responseData);
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

        public async Task<DeviceBackup?> GetAsync(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{id}");
                var responseData = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<DeviceBackup>(responseData);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<ICollection<DeviceBackup>?> GetAllAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("");
                var responseData = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<ICollection<DeviceBackup>>(responseData);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<ICollection<DeviceBackup>?> GetAllByDeviceIdAsync(int deviceId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"By-Device/{deviceId}");
                var responseData = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<ICollection<DeviceBackup>>(responseData);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<ICollection<DeviceBackup>?> GetAllByTenantIdAsync(int tenantId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"By-Tenant/{tenantId}");
                var responseData = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<ICollection<DeviceBackup>>(responseData);
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
