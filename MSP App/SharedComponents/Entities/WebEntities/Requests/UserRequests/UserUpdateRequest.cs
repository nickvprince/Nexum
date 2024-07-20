
namespace SharedComponents.Entities.WebEntities.Requests.UserRequests
{
    public class UserUpdateRequest
    {
        public string? UserName { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public string? CurrentPassword { get; set; }
    }
}
