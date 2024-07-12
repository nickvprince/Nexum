using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedComponents.WebEntities.Requests.BackupRequests
{
    public class BackupCreateRequest
    {
        public int DeviceId { get; set; }
        public string? Name { get; set; }
        public string? Path { get; set; }
        public DateTime? Date { get; set; }
        public int NASServerId { get; set; }
    }
}
