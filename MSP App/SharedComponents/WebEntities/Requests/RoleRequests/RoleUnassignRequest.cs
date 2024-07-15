using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedComponents.WebEntities.Requests.RoleRequests
{
    public class RoleUnassignRequest
    {
        public string? UserId { get; set; }
        public string? RoleId { get; set; }
    }
}
