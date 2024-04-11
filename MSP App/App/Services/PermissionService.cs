using Newtonsoft.Json.Linq;
using SharedComponents.Entities;
using SharedComponents.Services;
using System.Text.Json;

namespace App.Services
{
    public class PermissionService : BaseService, IPermissionService
    {
        public PermissionService(IConfiguration config, HttpClient httpClient) : base(config, httpClient)
        {
        }
        public Task<bool> AddAsync(Permission permission)
        {
            throw new NotImplementedException();
        }

        public Task<bool> EditAsync(Permission permission)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeleteAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<Permission> GetAsync(int id)
        {
            throw new NotImplementedException();
        }

        public async Task<List<Permission>> GetAllAsync()
        {
            var responseObject = await ProcessResponse(await _httpClient.GetAsync("api/Permission/Get"));
            var objectProperty = responseObject.GetType().GetProperty("Object");
            var objectValue = objectProperty.GetValue(responseObject);
            JObject data = JObject.Parse(objectValue.ToString());

            List<Permission> permissions = JsonSerializer.Deserialize<List<Permission>>(objectValue.ToString());
            return permissions;
        }
    }
}
