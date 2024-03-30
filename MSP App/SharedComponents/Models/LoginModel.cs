using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedComponents.Models
{
    public class LoginModel
    {
        [Required]
        [StringLength(100, ErrorMessage = "Username is too long.")]
        public string? Username { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "Password is too long.")]
        public string? Password { get; set; }
    }
}
