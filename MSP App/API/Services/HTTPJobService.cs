using API.DataAccess;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using SharedComponents.Entities;
using SharedComponents.Utilities;
using System.Net.Http;
using System.Text;

namespace API.Services
{
    public class HTTPJobService
    {
        public readonly HttpClient _httpClient;
        public readonly IConfiguration _config;
        public readonly AppDbContext _appDbContext;
        public HTTPJobService(IConfiguration config, HttpClient httpClient, AppDbContext appDbContext)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _appDbContext = appDbContext ?? throw new ArgumentNullException(nameof(appDbContext));
        }

        public async Task<bool> InitiallizeHttpClient(int tenantId)
        {
            try
            {
                Tenant? tenant = await _appDbContext.Tenants
                .Where(t => t.Id == tenantId)
                .FirstAsync();
                if (tenant != null)
                {
                    string? apiUrl = tenant.ApiBaseUrl + ":" + tenant.ApiBasePort;
                    if (apiUrl != null && Uri.TryCreate(apiUrl, UriKind.Absolute, out var baseUri))
                    {
                        _httpClient.BaseAddress = baseUri;
                        return true;
                    }
                    if (!string.IsNullOrEmpty(tenant.ApiKey))
                    {
                        _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("apikey", tenant.ApiKey);
                    }
                    else
                    {
                        Console.WriteLine("API key is not set for tenant.");
                        return false;
                    }
                }
            }
            catch (Exception)
            {
                Console.WriteLine("Error initializing API HttpClient.");
            }
            return false;
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
    }
}
