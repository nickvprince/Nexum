using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;
using System.Text.Json.Nodes;

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
                _config.GetSection("WebAppSettings")?.GetValue<string>("APIBasePort") + "/api/";

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

        public async Task<dynamic> ProcessResponse(HttpResponseMessage response)
        {
            if (response.IsSuccessStatusCode)
            {
                // Read the response content as a dynamic object
                //dynamic content = await response.Content.ReadFromJsonAsync<dynamic>();
                JsonNode content = await JsonNode.ParseAsync(await response.Content.ReadAsStreamAsync());
                // Return the extracted properties
                return new
                {
                    Object = content["data"],
                    Message = content["message"],
                };
            }
            else
            {
                return null;
            }
        }


        public async Task<dynamic?> ProcessResponsev2(HttpResponseMessage response)
        {
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadFromJsonAsync<dynamic>();
                return data?.data?.ToObject<List<dynamic>>();
            }
            else
            {
                return null;
            }
        }
    }
}
