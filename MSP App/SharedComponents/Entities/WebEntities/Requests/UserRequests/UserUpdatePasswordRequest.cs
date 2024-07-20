using System.ComponentModel.DataAnnotations;

namespace SharedComponents.Entities.WebEntities.Requests.UserRequests
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
