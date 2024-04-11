using Microsoft.Extensions.Configuration;

namespace SharedComponents.Services
{
    public abstract class BaseService
    {
        public readonly HttpClient _httpClient;
        public readonly IConfiguration _config;
        public BaseService(IConfiguration config, HttpClient httpClient)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));

            string? apiBaseUrl = _config.GetSection("WebAppSettings")?.GetValue<string>("APIBaseUri") + ":" + 
                _config.GetSection("WebAppSettings")?.GetValue<string>("APIBasePort");

            // Attempt to get the Base URL, handle potential errors gracefully
            if (apiBaseUrl != null && Uri.TryCreate(apiBaseUrl, UriKind.Absolute, out var baseUri))
            {
                _httpClient.BaseAddress = baseUri;
            }
            else
            {
                // Log or handle the missing configuration setting
                Console.WriteLine("Invalid API Base URL format or 'WebAppSettings:APIBaseUri' or 'WebAppSettings:APIBasePort' not found.");
            }
        }
    }
}
