using System.ComponentModel.DataAnnotations;

namespace App.Models
{
    public class LoginViewModel
    {
        [Required]
        [StringLength(100, ErrorMessage = "Username is too long.")]
        public string? Username { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "Password is too long.")]
        public string? Password { get; set; }
    }
}
