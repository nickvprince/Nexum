using API.DataAccess;
using Microsoft.EntityFrameworkCore;
using SharedComponents.Entities;

namespace API.Services.Interfaces
{
    public abstract class BaseHTTPService
    {
        public readonly HttpClient _httpClient;
        public readonly IConfiguration _config;
        public readonly AppDbContext _appDbContext;

        public BaseHTTPService(IConfiguration config, HttpClient httpClient, AppDbContext appDbContext)
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
                    if (tenant.ApiBaseUrl != null && tenant.ApiBasePort != null)
                    {
                        string? apiUrl = tenant.ApiBaseUrl + ":" + tenant.ApiBasePort;
                        if (apiUrl != null && Uri.TryCreate(apiUrl, UriKind.Absolute, out var baseUri))
                        {
                            _httpClient.BaseAddress = baseUri;
                            return true;
                        }
                        if (!string.IsNullOrEmpty(tenant.ApiKeyServer))
                        {
                            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("apikey", tenant.ApiKey);
                            return true;
                        }
                        else
                        {
                            Console.WriteLine("API key is not set for tenant.");
                        }
                    }
                }
            }
            catch (Exception)
            {
                Console.WriteLine("Error initializing API HttpClient.");
            }
            return false;
        }
    }
}
