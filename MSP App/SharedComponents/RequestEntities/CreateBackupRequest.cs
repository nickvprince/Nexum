using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedComponents.RequestEntities
{
    public class CreateBackupRequest
    {
        public int Client_Id { get; set; }
        public string? Uuid { get; set; }
        public string? Name { get; set; }
        public string? Path { get; set; }
        public DateTime? Date { get; set; }
        public int BackupServerId { get; set; }
    }
}
