using Newtonsoft.Json;
using SharedComponents.Entities.DbEntities;
using SharedComponents.Entities.WebEntities.Requests.RoleRequests;
using SharedComponents.Services.APIRequestServices;
using SharedComponents.Services.APIRequestServices.Interfaces;
using System.Text;

namespace App.Services.APIRequestServices
{
    public class APIRequestRoleService : BaseAPIRequestService, IAPIRequestRoleService
    {
        public APIRequestRoleService(IConfiguration config, HttpClient httpClient, IHttpContextAccessor httpContextAccessor) : base(config, httpClient, httpContextAccessor)
        {
            AppendBaseAddress("Role/");
        }

        public async Task<ApplicationRole?> CreateAsync(RoleCreateRequest request)
        {
            try
            {
                var content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("", content);
                var responseData = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<ApplicationRole>(responseData);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<ApplicationRole?> UpdateAsync(RoleUpdateRequest request)
        {
            try
            {
                var content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
                var response = await _httpClient.PutAsync("", content);
                var responseData = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<ApplicationRole>(responseData);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<bool> DeleteAsync(string? id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"{id}");
                var responseData = await response.Content.ReadAsStringAsync();
                response.EnsureSuccessStatusCode();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<ApplicationRole?> GetAsync(string? id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{id}");
                var responseData = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<ApplicationRole>(responseData);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<ICollection<ApplicationRole>?> GetAllAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("");
                var responseData = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<ICollection<ApplicationRole>>(responseData);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<ICollection<ApplicationRole>?> GetAllByUserIdAsync(string? userId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"By-User/{userId}");
                var responseData = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<ICollection<ApplicationRole>>(responseData);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<ICollection<ApplicationUserRole>?> GetAllUserRolesByUserIdAsync(string? userId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"UserRole/{userId}");
                var responseData = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<ICollection<ApplicationUserRole>>(responseData);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<ICollection<ApplicationRolePermission>?> GetAllRolePermissionByIdAsync(string? roleId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"Permission/{roleId}");
                var responseData = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<ICollection<ApplicationRolePermission>>(responseData);
            }
            catch (Exception)
            {
                return null;
            }
        }
        public async Task<bool> AssignAsync(RoleAssignRequest request)
        {
            try
            {
                var content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("Assign", content);
                var responseData = await response.Content.ReadAsStringAsync();
                response.EnsureSuccessStatusCode();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> UnassignAsync(RoleUnassignRequest request)
        {
            try
            {
                var content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("Unassign", content);
                var responseData = await response.Content.ReadAsStringAsync();
                response.EnsureSuccessStatusCode();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> AssignPermissionAsync(RoleAssignPermissionRequest request)
        {
            try
            {
                var content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("Assign-Permission", content);
                var responseData = await response.Content.ReadAsStringAsync();
                response.EnsureSuccessStatusCode();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> UnassignPermissionAsync(RoleUnassignPermissionRequest request)
        {
            try
            {
                var content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("Unassign-Permission", content);
                var responseData = await response.Content.ReadAsStringAsync();
                response.EnsureSuccessStatusCode();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
