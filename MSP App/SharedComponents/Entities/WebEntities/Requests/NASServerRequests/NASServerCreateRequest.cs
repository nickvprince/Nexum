﻿using System.ComponentModel.DataAnnotations;

namespace SharedComponents.Entities.WebEntities.Requests.NASServerRequests
{
    public class NASServerCreateRequest
    {
        [Required(ErrorMessage = "Name is required.")]
        public string? Name { get; set; }
        [Required(ErrorMessage = "Path is required.")]
        [RegularExpression(@"^\\.*$", ErrorMessage = "Invalid path. It should start with a '\\'.")]
        public string? Path { get; set; }
        public string? NASUsername { get; set; }
        public string? NASPassword { get; set; }
        [Required]
        public int TenantId { get; set; }
    }
}
