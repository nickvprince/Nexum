using Microsoft.Extensions.Configuration;
using SharedComponents.Entities.DbEntities;
using SharedComponents.Services.APIRequestServices.Interfaces;
using SharedComponents.Services.DbServices.Interfaces;

namespace SharedComponents.Services.TenantServerAPIServices
{
    public abstract class BaseTenantServerAPIService
    {
        public readonly HttpClient _httpClient;
        public readonly IConfiguration _config;
        public readonly IDbTenantService _dbTenantService;

        public BaseTenantServerAPIService(IConfiguration config, HttpClient httpClient, IDbTenantService dbTenantService)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _dbTenantService = dbTenantService ?? throw new ArgumentNullException(nameof(dbTenantService));
        }
        public async Task<bool> InitiallizeHttpClient(int tenantId)
        {
            try
            {
                Tenant? tenant = await _dbTenantService.GetAsync(tenantId);
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
