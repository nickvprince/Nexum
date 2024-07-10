using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedComponents.RequestEntities.HTTP
{
    public class GetDeviceFilesRequest
    {
        public int Client_Id { get; set; }
        public string? Path { get; set; }
    }
}
