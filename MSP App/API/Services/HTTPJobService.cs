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

        public async Task<bool> CreateJobAsync(int tenantId, DeviceJob job)
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

        public async Task<bool> UpdateJobAsync(int tenantId, DeviceJob job)
        {
            try
            {
                if (await InitiallizeHttpClient(tenantId))
                {
                    var content = new StringContent(JsonConvert.SerializeObject(job, new InvalidJsonUtilities()), Encoding.UTF8, "application/json");
                    var response = await _httpClient.PutAsync("modify_job", content);
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

        public async Task<bool> StartJobAsync(int clientId)
        {
            try
            {
                if (await InitiallizeHttpClient(clientId))
                {
                    var content = new StringContent(JsonConvert.SerializeObject(clientId), Encoding.UTF8, "application/json");
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

        public async Task<bool> StopJobAsync(int clientId)
        {
            try
            {
                if (await InitiallizeHttpClient(clientId))
                {
                    var content = new StringContent(JsonConvert.SerializeObject(clientId), Encoding.UTF8, "application/json");
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

        public async Task<bool> ResumeJobAsync(int clientId)
        {
            try
            {
                if (await InitiallizeHttpClient(clientId))
                {
                    var content = new StringContent(JsonConvert.SerializeObject(clientId), Encoding.UTF8, "application/json");
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

        public async Task<bool> PauseJobAsync(int clientId)
        {
            try
            {
                if (await InitiallizeHttpClient(clientId))
                {
                    var content = new StringContent(JsonConvert.SerializeObject(clientId), Encoding.UTF8, "application/json");
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
    }
}
