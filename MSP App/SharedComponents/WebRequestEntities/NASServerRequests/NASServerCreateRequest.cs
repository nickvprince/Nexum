using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedComponents.WebRequestEntities.NASServerRequests
{
    public class NASServerCreateRequest
    {
        public string? Name { get; set; }
        public string? Path { get; set; }
        public string? NASUsername { get; set; }
        public string? NASPassword { get; set; }
        public int BackupServerId { get; set; }
        public int TenantId { get; set; }
    }
}
