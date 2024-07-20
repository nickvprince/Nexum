
namespace SharedComponents.Entities.WebEntities.Requests.UserRequests
{
    public class UserCreateRequest
    {
        public string? UserName { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public int? TenantId { get; set; }
    }
}
