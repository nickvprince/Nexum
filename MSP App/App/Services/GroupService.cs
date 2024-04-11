using Newtonsoft.Json.Linq;
using SharedComponents.Entities;
using SharedComponents.Services;

namespace App.Services
{
    public class GroupService : BaseService, IGroupService
    {
        public GroupService(IConfiguration config, HttpClient httpClient) : base(config, httpClient)
        {
        }

        public Task<bool> CreateAsync(Group permission)
        {
            throw new NotImplementedException();
        }

        public Task<bool> EditAsync(Group permission)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeleteAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<Group> GetAsync(int id)
        {
            throw new NotImplementedException();
        }

        public async Task<List<Group>> GetAllAsync()
        {
            var responseObject = await ProcessResponse(await _httpClient.GetAsync("api/Group/Get"));
            var objectProperty = responseObject.GetType().GetProperty("Object");
            var objectValue = objectProperty.GetValue(responseObject);
            JArray groupArray = JArray.Parse(objectValue.ToString());

            List<Group> groups = new List<Group>();

            foreach (JObject groupObject in groupArray)
            {
                Group group = new Group
                {
                    Id = (int)groupObject["id"],
                    Name = (string)groupObject["name"],
                    Description = (string)groupObject["description"],
                    IsActive = (bool)groupObject["isActive"],
                };
                groups.Add(group);
            }

            return groups;
        }
    }
}
