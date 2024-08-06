using SharedComponents.Services.APIRequestServices.Interfaces;
using SharedComponents.Services.APIRequestServices;
using Newtonsoft.Json;
using SharedComponents.Entities.DbEntities;
using System.Text;

namespace App.Services.APIRequestServices
{
    public class APIRequestSoftwareService : BaseAPIRequestService, IAPIRequestSoftwareService
    {
        public APIRequestSoftwareService(IConfiguration config, HttpClient httpClient, IHttpContextAccessor httpContextAccessor) : base(config, httpClient, httpContextAccessor)
        {
            AppendBaseAddress("Software/");
        }

        public async Task<byte[]?> GetNexumInstallerAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("Installer");
                var responseData = await response.Content.ReadAsByteArrayAsync();
                response.EnsureSuccessStatusCode();
                return responseData;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
