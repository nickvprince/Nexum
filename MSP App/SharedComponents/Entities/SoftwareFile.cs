using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedComponents.Entities
{
    public class SoftwareFile
    {
        public int Id { get; set; }
        public string? UploadedFileName { get; set; }
        public string? Version { get; set; }
        public string? Tag { get; set; }
        public SoftwareFileType? FileType { get; set; }
    }

    public enum SoftwareFileType
    {
        Nexum,
        NexumServer,
        NexumService
    }
}
