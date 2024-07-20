using Newtonsoft.Json;
using SharedComponents.Entities.TenantServerHttpEntities.Requests;
using SharedComponents.Entities.TenantServerHttpEntities.Responses;
using SharedComponents.Services.APIRequestServices.Interfaces;
using SharedComponents.Services.DbServices.Interfaces;
using SharedComponents.Services.TenantServerAPIServices;
using SharedComponents.Services.TenantServerAPIServices.Interfaces;
using System.Text;

namespace API.Services.TenantServerAPIServices
{
    public class TenantServerAPINASServerService : BaseTenantServerAPIService, ITenantServerAPINASServerService
    {
        public TenantServerAPINASServerService(IConfiguration config, HttpClient httpClient, IDbTenantService dbTenantService) : base(config, httpClient, dbTenantService)
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

        public async Task<bool?> DeleteAsync(int tenantId, DeleteNASServerRequest? request)
        {
            try
            {
                if (await InitiallizeHttpClient(tenantId))
                {
                    var response = await _httpClient.DeleteAsync($"delete_smb/{request.Id}");
                    var responseData = await response.Content.ReadAsStringAsync();
                    response.EnsureSuccessStatusCode();
                    return true;
                }
            }
            catch (Exception)
            {
                Console.WriteLine("Error deleting NAS server on the server.");
            }
            return null;
        }

        public async Task<GetNASServerResponse?> GetAsync(int tenantId, int id)
        {
            try
            {
                if (await InitiallizeHttpClient(tenantId))
                {
                    var response = await _httpClient.GetAsync($"get_smb/{id}");
                    var responseData = await response.Content.ReadAsStringAsync();
                    response.EnsureSuccessStatusCode();
                    return JsonConvert.DeserializeObject<GetNASServerResponse>(responseData);
                }
            }
            catch (Exception)
            {
                Console.WriteLine("Error getting NAS server from the server.");
            }
            return null;
        }
    }
}
