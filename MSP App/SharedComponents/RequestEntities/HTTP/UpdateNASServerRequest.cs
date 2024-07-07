using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedComponents.RequestEntities.HTTP
{
    public class UpdateNASServerRequest
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Path { get; set; }
        public string? NASUsername { get; set; }
        public string? NASPassword { get; set; }
    }
}
