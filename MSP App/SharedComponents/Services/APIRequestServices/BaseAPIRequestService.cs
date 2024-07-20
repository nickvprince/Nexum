using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;

namespace SharedComponents.Services.APIRequestServices
{
    public abstract class BaseAPIRequestService
    {
        public readonly HttpClient _httpClient;
        public readonly IConfiguration _config;
        public readonly IHttpContextAccessor _httpContextAccessor;
        public BaseAPIRequestService(IConfiguration config, HttpClient httpClient, IHttpContextAccessor httpContextAccessor)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));

            SetBaseAddress();
            SetBearerToken();
        }
        private void SetBaseAddress(string additionalPath = "")
        {
            string? apiBaseUrl = _config.GetSection("WebAppSettings")?.GetValue<string>("APIBaseUri") + ":" +
                                 _config.GetSection("WebAppSettings")?.GetValue<string>("APIBasePort") + "/api/";

            if (apiBaseUrl != null && Uri.TryCreate(apiBaseUrl, UriKind.Absolute, out var baseUri))
            {
                _httpClient.BaseAddress = new Uri(baseUri, additionalPath);
            }
            else
            {
                Console.WriteLine("Invalid API Base URL format or 'WebAppSettings:APIBaseUri' or 'WebAppSettings:APIBasePort' not found.");
            }
        }
        protected void AppendBaseAddress(string additionalPath)
        {
            SetBaseAddress(additionalPath);
        }

        private void SetBearerToken()
        {
            var bearerToken = _httpContextAccessor.HttpContext?.User?.Claims.FirstOrDefault(c => c.Type == "Token")?.Value;
            if (!string.IsNullOrEmpty(bearerToken))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
            }
        }
    }
}
