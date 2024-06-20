using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedComponents.DbServices
{
    public interface IDbSecurityService
    {
        public Task<bool> ValidateAPIKey(string apikey);
    }
}
