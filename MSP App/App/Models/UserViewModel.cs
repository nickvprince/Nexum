using SharedComponents.Entities;
using SharedComponents.WebEntities.Responses.UserResponses;

namespace App.Models
{
    public class UserViewModel
    {
        public ICollection<UserResponse>? Users { get; set; }
    }
}
