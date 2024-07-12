using System.ComponentModel.DataAnnotations;

namespace SharedComponents.WebEntities.Requests.AuthRequests
{
    public class AuthLoginRequest
    {
        [Required]
        public string? Username { get; set; }
        [Required]
        public string? Password { get; set; }
    }
}
