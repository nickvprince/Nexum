using SharedComponents.Entities.WebEntities.Responses.UserResponses;

namespace App.Models
{
    public class UserViewModel
    {
        public ICollection<UserResponse>? Users { get; set; }
    }
}
