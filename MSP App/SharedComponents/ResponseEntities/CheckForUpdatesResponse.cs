using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedComponents.ResponseEntities
{
    public class CheckForUpdatesResponse
    {
        public bool NexumUpdateAvailable { get; set; }
        public bool NexumServerUpdateAvailable { get; set; }
        public bool NexumServiceUpdateAvailable { get; set; }
    }
}
