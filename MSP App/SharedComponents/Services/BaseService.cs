using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Nodes;

namespace SharedComponents.Services
{
    public abstract class BaseService
    {
        public readonly HttpClient _httpClient;
        public readonly IConfiguration _config;
        public readonly IHttpContextAccessor _httpContextAccessor;
        public BaseService(IConfiguration config, HttpClient httpClient, IHttpContextAccessor httpContextAccessor)
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
            var bearerToken = _httpContextAccessor.HttpContext.Session.GetString("Token");
            if (!string.IsNullOrEmpty(bearerToken))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
            }
        }
    }
}
