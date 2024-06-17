﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedComponents.Entities
{
    public class Device
    {
        public int Id { get; set; }
        [Required]
        public int TenantId { get; set; }
        public Tenant? Tenant { get; set; }
        [Required]
        public DeviceInfo? DeviceInfo { get; set; }
        public DeviceStatus? Status { get; set; }
        public string? StatusMessage { get; set; }
        public bool IsVerified { get; set; }
        public bool IsActive { get; set; }
    }

    public enum DeviceStatus
    {
        Online,
        Offline,
        BackupInProgress,
    }
}
