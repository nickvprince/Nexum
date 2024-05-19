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

        public async Task<List<Tenant>> GetAllAsync()
        {
            var responseObject = await ProcessResponse(await _httpClient.GetAsync("api/Tenant/Get"));
            var objectProperty = responseObject.GetType().GetProperty("Object");
            var objectValue = objectProperty.GetValue(responseObject);
            JArray tenantArray = JArray.Parse(objectValue.ToString());

            List<Tenant> tenants = new List<Tenant>();

            foreach (JObject tenantObject in tenantArray)
            {
                Tenant tenant = new Tenant
                {
                    
                    Id = (int)tenantObject["id"],
                    Name = (string)tenantObject["name"],
                    ApiKey = (string)tenantObject["apiKey"],
                    IsActive = (bool)tenantObject["isActive"],
                };
                tenants.Add(tenant);
            }

            return tenants;
        }
    }
}
