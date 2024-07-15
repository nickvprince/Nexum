using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedComponents.ResponseEntities.HTTP
{
    public class GetNASServerResponse
    {
        public string? Name { get; set; }
        public string? Path { get; set; }
        public string? Username { get; set; }
        public string? Password { get; set; }
    }
}
