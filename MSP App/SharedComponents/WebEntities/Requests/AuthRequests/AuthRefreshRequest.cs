using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedComponents.WebEntities.Requests.AuthRequests
{
    public class AuthRefreshRequest
    {
        public string? Token { get; set; }
        public string? RefreshToken { get; set; }
    }
}
