using Microsoft.AspNetCore.Mvc;

namespace API.Services.Interfaces
{
    public interface IAuthService
    {
        public Task<bool> UserHasPermissionAsync<T>(string token, int tenantId, [System.Runtime.CompilerServices.CallerMemberName] string methodName = "") where T : ControllerBase;
    }
}
