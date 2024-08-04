using Newtonsoft.Json;
using SharedComponents.Entities.DbEntities;
using SharedComponents.Services.APIRequestServices.Interfaces;
using SharedComponents.Services.DbServices.Interfaces;
using SharedComponents.Services.TenantServerAPIServices;
using SharedComponents.Services.TenantServerAPIServices.Interfaces;
using SharedComponents.Utilities;
using System.Text;

namespace API.Services.TenantServerAPIServices
{
    public class TenantServerAPIJobService : BaseTenantServerAPIService, ITenantServerAPIJobService
    {
        public TenantServerAPIJobService(IConfiguration config, HttpClient httpClient, IDbTenantService dbTenantService) : base(config, httpClient, dbTenantService)
        {
            //Use if tenant server api is updated with proper routes
            /*if (_httpClient.BaseAddress != null)
            {
                _httpClient.BaseAddress = new Uri(_httpClient.BaseAddress, "Job/");
            }
            else
            {
                throw new InvalidOperationException("BaseAddress is not set.");
            }*/
        }

        public async Task<bool> CreateAsync(int tenantId, DeviceJob job)
        {
            try
            {
                if (await InitiallizeHttpClient(tenantId))
                {
                    var content = new StringContent(JsonConvert.SerializeObject(job, new InvalidJsonUtilities()), Encoding.UTF8, "application/json");
                    var response = await _httpClient.PostAsync("modify_job", content);
                    var responseData = await response.Content.ReadAsStringAsync();
                    response.EnsureSuccessStatusCode();
                    return true;
                }
            }
            catch (Exception)
            {
                Console.WriteLine("Error creating job on the server.");
            }
            return false;
        }

        public async Task<bool> UpdateAsync(int tenantId, DeviceJob job)
        {
            try
            {
                if (await InitiallizeHttpClient(tenantId))
                {
                    var content = new StringContent(JsonConvert.SerializeObject(job, new InvalidJsonUtilities()), Encoding.UTF8, "application/json");
                    var response = await _httpClient.PostAsync("modify_job", content);
                    var responseData = await response.Content.ReadAsStringAsync();
                    response.EnsureSuccessStatusCode();
                    return true;
                }
            }
            catch (Exception)
            {
                Console.WriteLine("Error updating job on the server.");
            }
            return false;
        }

        public async Task<bool> DeleteAsync(int tenantId, int clientId, int id)
        {
            try
            {
                if (await InitiallizeHttpClient(tenantId))
                {
                    var response = await _httpClient.DeleteAsync($"delete_job/{clientId}?job_id={id}");
                    var responseData = await response.Content.ReadAsStringAsync();
                    response.EnsureSuccessStatusCode();
                    return true;
                }
            }
            catch (Exception)
            {
                Console.WriteLine("Error deleting job on the server.");
            }
            return false;
        }

        public async Task<bool> StartAsync(int tenantId, int clientId)
        {
            try
            {
                if (await InitiallizeHttpClient(tenantId))
                {
                    int client_Id = clientId;
                    var content = new StringContent(JsonConvert.SerializeObject(new { client_id = clientId }), Encoding.UTF8, "application/json");
                    var response = await _httpClient.PostAsync("start_job", content);
                    var responseData = await response.Content.ReadAsStringAsync();
                    response.EnsureSuccessStatusCode();
                    return true;
                }
            }
            catch (Exception)
            {
                Console.WriteLine("Error starting job on the server.");
            }
            return false;
        }

        public async Task<bool> StopAsync(int tenantId, int clientId)
        {
            try
            {
                if (await InitiallizeHttpClient(tenantId))
                {
                    int client_Id = clientId;
                    var content = new StringContent(JsonConvert.SerializeObject(new { client_id = clientId }), Encoding.UTF8, "application/json");
                    var response = await _httpClient.PostAsync("kill_job", content);
                    var responseData = await response.Content.ReadAsStringAsync();
                    response.EnsureSuccessStatusCode();
                    return true;
                }
            }
            catch (Exception)
            {
                Console.WriteLine("Error stopping job on the server.");
            }
            return false;
        }

        public async Task<bool> ResumeAsync(int tenantId, int clientId)
        {
            try
            {
                if (await InitiallizeHttpClient(tenantId))
                {
                    int client_Id = clientId;
                    var content = new StringContent(JsonConvert.SerializeObject(new { client_id = clientId }), Encoding.UTF8, "application/json");
                    var response = await _httpClient.PostAsync("enable_job", content);
                    var responseData = await response.Content.ReadAsStringAsync();
                    response.EnsureSuccessStatusCode();
                    return true;
                }
            }
            catch (Exception)
            {
                Console.WriteLine("Error resuming job on the server.");
            }
            return false;
        }

        public async Task<bool> PauseAsync(int tenantId, int clientId)
        {
            try
            {
                if (await InitiallizeHttpClient(tenantId))
                {
                    int client_Id = clientId;
                    var content = new StringContent(JsonConvert.SerializeObject(new { client_id = clientId }), Encoding.UTF8, "application/json");
                    var response = await _httpClient.PostAsync("stop_job", content);
                    var responseData = await response.Content.ReadAsStringAsync();
                    response.EnsureSuccessStatusCode();
                    return true;
                }
            }
            catch (Exception)
            {
                Console.WriteLine("Error pausing job on the server.");
            }
            return false;
        }
        //bool because cannot deserialize tenant server response
        public async Task<bool> GetAsync(int tenantId, int clientId)
        {
            try
            {
                if (await InitiallizeHttpClient(tenantId))
                {
                    var response = await _httpClient.GetAsync($"get_job/{clientId}");
                    var responseData = await response.Content.ReadAsStringAsync();
                    response.EnsureSuccessStatusCode();
                    return true;
                }
            }
            catch (Exception)
            {
                Console.WriteLine("Error getting job from the server.");
            }
            return false;
        }
        //cannot deserialize tenant server response (Not implemented)
        public async Task<bool> GetAllAsync(int tenantId, int clientId)
        {
            try
            {
                if (await InitiallizeHttpClient(tenantId))
                {
                    var response = await _httpClient.GetAsync($"get_jobs/{clientId}");
                    var responseData = await response.Content.ReadAsStringAsync();
                    response.EnsureSuccessStatusCode();
                    return true;
                }
            }
            catch (Exception)
            {
                Console.WriteLine("Error getting all jobs from the server.");
            }
            return false;
        }

        public async Task<DeviceJobStatus?> GetStatusAsync(int tenantId, int clientId, int id)
        {
            try
            {
                if (await InitiallizeHttpClient(tenantId))
                {
                    var response = await _httpClient.GetAsync($"get_job_status/{clientId}?job_id={id}");
                    var responseData = await response.Content.ReadAsStringAsync();
                    response.EnsureSuccessStatusCode();
                    return JsonConvert.DeserializeObject<DeviceJobStatus>(responseData);
                }
            }
            catch (Exception)
            {
                Console.WriteLine("Error getting job status from the server.");
            }
            return null;
        }
    }
}
