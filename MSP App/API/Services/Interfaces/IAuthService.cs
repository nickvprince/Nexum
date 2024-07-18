using Microsoft.AspNetCore.Mvc;
using SharedComponents.Entities;

namespace API.Services.Interfaces
{
    public interface IAuthService
    {
        public Task<bool> UserHasPermissionAsync<T>(string token, int tenantId, [System.Runtime.CompilerServices.CallerMemberName] string methodName = "") where T : ControllerBase;
        public Task<ICollection<int>> GetUserAccessibleTenantsAsync(string token);
    }
}
