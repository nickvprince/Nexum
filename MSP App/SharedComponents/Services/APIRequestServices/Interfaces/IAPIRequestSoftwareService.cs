using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedComponents.Services.APIRequestServices.Interfaces
{
    public interface IAPIRequestSoftwareService
    {
        public Task<byte[]?> GetNexumInstallerAsync();
    }
}
