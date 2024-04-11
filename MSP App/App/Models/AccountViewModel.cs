using System.ComponentModel.DataAnnotations;

namespace App.Models
{
    public class AccountViewModel
    {
        public string? Username { get; set; }
        [Required]
        public string? Password { get; set; }
        public string? Email { get; set; }
    }
}
