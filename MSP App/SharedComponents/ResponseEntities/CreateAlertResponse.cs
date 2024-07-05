using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedComponents.ResponseEntities
{
    public class CreateAlertResponse
    {
        public string? Name { get; set; }
        public string? Severity { get; set; }
        public string? Message { get; set; }
        public DateTime? Time { get; set; }
    }
}
