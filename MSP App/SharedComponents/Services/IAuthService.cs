using SharedComponents.Entities;
using SharedComponents.WebEntities.Requests.AuthRequests;
using SharedComponents.WebEntities.Responses.AuthResponses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedComponents.Services
{
    public interface IAuthService
    {
        public Task<AuthLoginResponse?> LoginAsync(AuthLoginRequest request);
        public Task<AuthLoginResponse?> RefreshAsync(AuthRefreshRequest request);
    }
}
