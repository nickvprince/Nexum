using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedComponents.ResponseEntities.HTTP
{
    public class GetDeviceFilesResponse
    {
        public ICollection<string>? Files { get; set; }
    }
}
