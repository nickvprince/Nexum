using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedComponents.ResponseEntities
{
    public class MACAddressResponse
    {
        public int Id { get; set; }
        public string? Address { get; set; }
    }
}
