using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedComponents.Entities
{
    public class UserPermissionSet
    {
        public string UserId { get; set; }
        public User? User { get; set; }
        public int PermissionSetId { get; set; }
        public PermissionSet? PermissionSet { get; set; }
    }
}
