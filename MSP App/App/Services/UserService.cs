using Newtonsoft.Json.Linq;
using SharedComponents.Entities;
using SharedComponents.Services;
using System.Text.Json;

namespace App.Services
{
    public class UserService : BaseService, IUserService
    {
        public UserService(IConfiguration config, HttpClient httpClient) : base(config, httpClient)
        {
        }

        public Task<bool> DeleteAsync(string username)
        {
            throw new NotImplementedException();
        }

        public Task<bool> EditAsync(User user)
        {
            throw new NotImplementedException();
        }

        public Task<bool> CreateAsync(User user)
        {
            throw new NotImplementedException();
        }

        public Task<User> GetAsync(string username)
        {
            throw new NotImplementedException();
        }

        public async Task<List<User>> GetAllAsync()
        {
            var responseObject = await ProcessResponse(await _httpClient.GetAsync("api/User/Get"));
            var objectProperty = responseObject.GetType().GetProperty("Object");
            var objectValue = objectProperty.GetValue(responseObject);
            JArray usersArray = JArray.Parse(objectValue.ToString());

            List<User> users = new List<User>();

            foreach (JObject userObject in usersArray)
            {
                string passwordHash = (string)userObject["passwordHash"].ToString();
                User user = new User
                {
                    Id = (string)userObject["id"], //not currently working because identiyUser does not allow for manual id management
                    UserName = (string)userObject["userName"],
                    PasswordHash = passwordHash.Substring(0, 25),
                    Email = (string)userObject["email"],
                };
                users.Add(user);
            }

            return users;
        }
    }
}
