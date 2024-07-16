﻿using Newtonsoft.Json;

namespace SharedComponents.Entities
{
    public class TenantInfo
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? Zip { get; set; }
        public string? Country { get; set; }
        public int TenantId { get; set; }
        [JsonIgnore]
        public Tenant? Tenant { get; set; }
    }
}
