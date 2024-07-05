using SharedComponents.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedComponents.RequestEntities
{
    public class CheckForUpdatesRequest
    {
        public string? NexumVersion { get; set; }
        public string? NexumTag { get; set; }
        public string? NexumServerVersion { get; set; }
        public string? NexumServerTag { get; set; }
        public string? NexumServiceVersion { get; set; }
        public string? NexumServiceTag { get; set; }
    }
}
