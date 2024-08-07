﻿using App.Models;
using Newtonsoft.Json.Linq;
using SharedComponents.Entities;
using SharedComponents.WebRequestEntities;
using SharedComponents.Services;
using System.Text.Json;

namespace App.Services
{
    public class AccountService : BaseService, IAccountService
    {
        public AccountService(IConfiguration config, HttpClient httpClient) : base(config, httpClient)
        {
        }

        public async Task<ApplicationUser> LoginAsync(string username, string password)
        {
            LoginRequest loginRequest = new LoginRequest
            {
                Username = username,
                Password = password
            };

            var responseObject = await ProcessResponse(await _httpClient.PostAsJsonAsync("Auth/Login", loginRequest));
            var objectProperty = responseObject.GetType().GetProperty("Object");
            var objectValue = objectProperty.GetValue(responseObject);
            JObject data = JObject.Parse(objectValue.ToString());

            //Use this for other object, not for User because IdentityUser has different cases (Must use camelCase)
            //var users = JsonSerializer.Deserialize<User>(objectValue.ToString());
            ApplicationUser user = new ApplicationUser
            {
                UserName = (string)data["userName"],
                PasswordHash = (string)data["passwordHash"],
                Email = (string)data["email"],
            };
            return user;
        }
    }
}
