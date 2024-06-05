using SharedComponents.Entities;

namespace App.Models
{
    public class UserViewModel
    {
        public ICollection<User>? Users { get; set; }
    }
}
