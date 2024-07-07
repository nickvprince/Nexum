using API.DataAccess;
using API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using SharedComponents.Entities;
using SharedComponents.RequestEntities.HTTP;
using SharedComponents.ResponseEntities.HTTP;
using System.Text;

namespace API.Services
{
    public class HTTPNASServerService : BaseHTTPService, IHTTPNASServerService
    {
        public HTTPNASServerService(IConfiguration config, HttpClient httpClient, AppDbContext appDbContext) : base(config, httpClient, appDbContext)
        {
            //Use if tenant server api is updated with proper routes
            /*if (_httpClient.BaseAddress != null)
            {
                _httpClient.BaseAddress = new Uri(_httpClient.BaseAddress, "NASServer/");
            }
            else
            {
                throw new InvalidOperationException("BaseAddress is not set.");
            }*/
        }

        public async Task<CreateNASServerResponse?> CreateAsync(int tenantId, CreateNASServerRequest? request)
        {
            try
            {
                if (await InitiallizeHttpClient(tenantId))
                {
                    var content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
                    var response = await _httpClient.PostAsync("make_smb", content);
                    var responseData = await response.Content.ReadAsStringAsync();
                    response.EnsureSuccessStatusCode();
                    return JsonConvert.DeserializeObject<CreateNASServerResponse>(responseData);
                }
            }
            catch (Exception)
            {
                Console.WriteLine("Error creating NAS server on the server.");
            }
            return null;
        }

        public async Task<bool?> UpdateAsync(int tenantId, UpdateNASServerRequest? request)
        {
            try
            {
                if (await InitiallizeHttpClient(tenantId))
                {
                    var content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
                    var response = await _httpClient.PostAsync("make_smb", content);
                    var responseData = await response.Content.ReadAsStringAsync();
                    response.EnsureSuccessStatusCode();
                    return true;
                }
            }
            catch (Exception)
            {
                Console.WriteLine("Error updating NAS server on the server.");
            }
            return null;
        }
    }
}
