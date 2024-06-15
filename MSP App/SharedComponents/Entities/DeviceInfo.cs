﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedComponents.Entities
{
    public class DeviceInfo
    {
        public int Id { get; set; }
        [Required]
        public string? Name { get; set; }
        [Required]
        public int ClientId { get; set; }
        [Required]
        public string? Uuid { get; set; }
        [Required]
        public string? IpAddress { get; set; }
        [Required]
        public int? Port { get; set; }
        public string? Type { get; set; }
        [Required]
        public ICollection<string>? MacAddresses { get; set; }
        public int DeviceId { get; set; }
        public Device? Device { get; set; }
    }
}
