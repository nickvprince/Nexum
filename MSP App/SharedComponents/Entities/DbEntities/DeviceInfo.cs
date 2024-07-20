﻿using System.ComponentModel.DataAnnotations;

namespace SharedComponents.Entities.DbEntities
{
    public class DeviceInfo
    {
        public int Id { get; set; }
        public string? Nickname { get; set; }
        [Required]
        public string? Name { get; set; }
        public int ClientId { get; set; }
        [Required]
        public string? Uuid { get; set; }
        [Required]
        public string? IpAddress { get; set; }
        [Required]
        public int Port { get; set; }
        [Required]
        public DeviceType? Type { get; set; }
        [Required]
        public ICollection<MACAddress>? MACAddresses { get; set; }
        public int DeviceId { get; set; }
        [System.Text.Json.Serialization.JsonIgnore]
        public Device? Device { get; set; }
    }
    public enum DeviceType
    {
        Server,
        Desktop,
        Laptop
    }
}