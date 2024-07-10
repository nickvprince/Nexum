using API.DataAccess;
using API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using SharedComponents.Entities;
using SharedComponents.Utilities;
using System.Net.Http;
using System.Text;

namespace API.Services
{
    public class HTTPJobService : BaseHTTPService, IHTTPJobService
    {
        public HTTPJobService(IConfiguration config, HttpClient httpClient, AppDbContext appDbContext) : base(config, httpClient, appDbContext)
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

        public async Task<bool> DeleteAsync(int tenantId, int client_Id, int id)
        {
            try
            {
                if (await InitiallizeHttpClient(tenantId))
                {
                    var content = new StringContent(JsonConvert.SerializeObject(client_Id), Encoding.UTF8, "application/json");
                    var response = await _httpClient.DeleteAsync($"delete_job/{client_Id}?job_id={id}");
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

        public async Task<bool> StartAsync(int tenantId, int client_Id)
        {
            try
            {
                if (await InitiallizeHttpClient(tenantId))
                {
                    var content = new StringContent(JsonConvert.SerializeObject(client_Id), Encoding.UTF8, "application/json");
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

        public async Task<bool> StopAsync(int tenantId, int client_Id)
        {
            try
            {
                if (await InitiallizeHttpClient(tenantId))
                {
                    var content = new StringContent(JsonConvert.SerializeObject(client_Id), Encoding.UTF8, "application/json");
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

        public async Task<bool> ResumeAsync(int tenantId, int client_Id)
        {
            try
            {
                if (await InitiallizeHttpClient(tenantId))
                {
                    var content = new StringContent(JsonConvert.SerializeObject(client_Id), Encoding.UTF8, "application/json");
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

        public async Task<bool> PauseAsync(int tenantId, int client_Id)
        {
            try
            {
                if (await InitiallizeHttpClient(tenantId))
                {
                    var content = new StringContent(JsonConvert.SerializeObject(client_Id), Encoding.UTF8, "application/json");
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
        public async Task<bool> GetAsync(int tenantId, int client_Id)
        {
            try
            {
                if (await InitiallizeHttpClient(tenantId))
                {
                    var response = await _httpClient.GetAsync($"get_job/{client_Id}");
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
        public async Task<bool> GetAllAsync(int tenantId, int client_Id)
        {
            try
            {
                if (await InitiallizeHttpClient(tenantId))
                {
                    var response = await _httpClient.GetAsync($"get_jobs/{client_Id}");
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

        public async Task<DeviceJobStatus?> GetStatusAsync(int tenantId, int client_Id, int id)
        {
            try
            {
                if (await InitiallizeHttpClient(tenantId))
                {
                    var response = await _httpClient.GetAsync($"get_job_status/{client_Id}?job_id={id}");
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
