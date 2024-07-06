using SharedComponents.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedComponents.WebRequestEntities.InstallationKeyRequests
{
    public class InstallationKeyCreateRequest
    {
        public int TenantId { get; set; }
        public InstallationKeyType Type { get; set; }
    }
}
