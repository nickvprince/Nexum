using Microsoft.AspNetCore.Mvc;

namespace SharedComponents.Services.APIServices.Interfaces
{
    public interface IAPIAuthService
    {
        public Task<bool> UserHasPermissionAsync<T>(string token, int tenantId, [System.Runtime.CompilerServices.CallerMemberName] string methodName = "") where T : ControllerBase;
        public Task<IEnumerable<int?>?> GetUserAccessibleTenantsAsync(string token);
    }
}
