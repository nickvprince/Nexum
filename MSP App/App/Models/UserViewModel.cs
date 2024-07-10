using SharedComponents.Entities;

namespace App.Models
{
    public class UserViewModel
    {
        public ICollection<ApplicationUser>? Users { get; set; }
    }
}
