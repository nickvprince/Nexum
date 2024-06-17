using SharedComponents.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedComponents.RequestEntities
{
    public class VerifyInstallationRequest
    {
        public string? Uuid { get; set; }
        public string? InstallationKey { get; set; }
    }
}
