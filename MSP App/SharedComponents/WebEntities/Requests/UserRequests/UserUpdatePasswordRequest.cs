using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedComponents.WebEntities.Requests.UserRequests
{
    public class UserUpdatePasswordRequest
    {
        public string? UserName { get; set; }
        public string? CurrentPassword { get; set; }
        public string? NewPassword { get; set; }
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation do not match")]
        public string? ConfirmNewPassword { get; set; }

    }
}
