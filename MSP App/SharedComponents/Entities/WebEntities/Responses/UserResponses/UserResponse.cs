using SharedComponents.Entities.DbEntities;

namespace SharedComponents.Entities.WebEntities.Responses.UserResponses
{
    public class UserResponse
    {
        public string? Id { get; set; }
        public string? UserName { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public AccountType Type { get; set; }
    }
}
