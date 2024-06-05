using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SharedComponents.Entities;
using SharedComponents.Services;

namespace App.Services
{
    public class TenantService : BaseService, ITenantService
    {
        public TenantService(IConfiguration config, HttpClient httpClient) : base(config, httpClient)
        {
        }

        public Task<bool> CreateAsync(Tenant tenant)
        {
            throw new NotImplementedException();
        }

        public Task<bool> EditAsync(Tenant tenant)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeleteAsync(string id)
        {
            throw new NotImplementedException();
        }

        public Task<Tenant?> GetAsync(string id)
        {
            throw new NotImplementedException();
        }

        public async Task<ICollection<Tenant>?> GetAllAsync()
        {
            var response = await _httpClient.GetAsync("api/Tenant/Get");
            var responseData = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<ICollection<Tenant>>(responseData);
        }
    }
}
