using SharedComponents.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedComponents.Services
{
    public interface IAccountService
    {
        public Task<ApplicationUser> LoginAsync(string username, string password);
    }
}
