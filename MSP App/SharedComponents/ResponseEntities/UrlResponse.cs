using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedComponents.ResponseEntities
{
    public class UrlResponse
    {
        public string? PortalUrl { get; set; }
        public string? PortalUrlLocal { get; set; }
        public string? NexumUrl { get; set; }
        public string? NexumUrlLocal { get; set; }
        public string? NexumServerUrl { get; set; }
        public string? NexumServerUrlLocal { get; set; }
        public string? NexumServiceUrl { get; set; }
        public string? NexumServiceUrlLocal { get; set; }
    }
}
